namespace Nop.Plugin.Widgets.AzPersonalizer.Components
{

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.CognitiveServices.Personalizer.Models;
    using Nop.Core;
    using Nop.Core.Domain.Catalog;
    using Nop.Core.Domain.Logging;
    using Nop.Plugin.Widgets.AzPersonalizer.Controllers;
    using Nop.Plugin.Widgets.AzPersonalizer.Models;
    using Nop.Plugin.Widgets.AzPersonalizer.Services;
    using Nop.Services.Catalog;
    using Nop.Services.Logging;
    using Nop.Web.Factories;
    using Nop.Web.Framework.Components;
    using Nop.Web.Framework.Infrastructure;
    using Nop.Web.Models.Catalog;

    /// <summary>
    ///     Represents the view component to place a widget into pages
    /// </summary>
   [ViewComponent(Name = "WidgetsAzPersonalizer")]
    public class WidgetsAzPersonalizerViewComponent : NopViewComponent
    {

        #region fields

        private readonly IWebHelper _webHelper;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ILogger _logger;
        private readonly PersonalizerClientService _personalizerClientService;

        #endregion

        #region ctor

        public WidgetsAzPersonalizerViewComponent (PersonalizerClientService personalizerClientService,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IWebHelper webHelper,
            ILogger logger)
        {
            _logger = logger;
            _webHelper = webHelper;
            _personalizerClientService = personalizerClientService;
            _productService = productService;
            _productModelFactory = productModelFactory;
        }

        #endregion

        #region methods

        /// <summary>
        ///     Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        ///     A task that represents the asynchronous operation
        ///     The task result contains the view component result
        /// </returns
        public async Task<IViewComponentResult> InvokeAsync (string widgetZone, object additionalData)
        {
            if (widgetZone == null)
            {
                await _logger.ErrorAsync("Widget Zone was null");
                throw new ArgumentNullException(widgetZone);
            }

            if (widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialBottom,
                StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    string[] url = _webHelper.GetThisPageUrl(true)?.Split("?");
                    if (TempData["RewardID"] != null
                        && TempData["OrderedIDs"] != null)
                    {
                        string id = TempData["RewardID"] is string rewardID ? rewardID : null;
                        string pos = TempData["OrderedIDs"] is string position ? position : null;

                        await ProcessRewardAsync(id, pos, additionalData);
                    }

                    IViewComponentResult list = await RecommendedListAsync(additionalData);
                    if (list == null)
                    {
                        await _logger.ErrorAsync("Confirm everything is corret in the plugin settings.");
                        return View("~/Plugins/Widgets.AzPersonalizer/Views/Default.cshtml");
                    }

                    return list;
                }
                catch (Exception e)
                {
                    await _logger.ErrorAsync(e.Message, e);
                    return View("~/Plugins/Widgets.AzPersonalizer/Views/Default.cshtml");
                }
            }

            // While there is only one widget zone in the plugin this shouldn't return anything. If you want to add
            // recommendations to multiple widget zones, then you should make this an else if and return the appropriate view.
            return View("~/Plugins/Widgets.AzPersonalizer/Views/Default.cshtml");
        }


        /// <summary>
        ///     Prepares the view component with a list of Ranked Actions
        /// </summary>
        /// <param name="azPersonalizerSettings"></param>
        /// <param name="additionalData"></param>
        /// <returns>
        ///     A task that represents the asynchronous operation
        ///     The task result contains the view component result
        /// </returns>
        private async Task<IViewComponentResult> RecommendedListAsync (object additionalData)
        {
            try
            {
                RankResponse rankedActions =
                    await _personalizerClientService?.GetRankedActionsAsync(additionalData);
                IList<RankedAction> model = rankedActions.Ranking.OrderByDescending(r => r.Probability).ToList();
                TempDataTokenController tdt = new(rankedActions.EventId);
                TempData["RewardID"] = rankedActions.EventId;
                IList<Product> products = new List<Product>(model.Count);
                StringBuilder odds = new(rankedActions.EventId + ": ");

                for (int i = 0; i < model.Count; i++)
                {
                    odds.Append("(" + model[i]?.Id + "," + model[i].Probability + ");");
                    products.Insert(i,
                        await _productService?.GetProductByIdAsync(int.Parse(model[i]?.Id,
                            new CultureInfo("en-UK"))));
                    tdt.AddId(model[i]?.Id);
                }

                TempData["OrderedIDs"] = tdt.OrderedIDs;

                await _logger.InsertLogAsync(LogLevel.Debug, odds.ToString());

                return View("~/Plugins/Widgets.AzPersonalizer/Views/Recommendations.cshtml",
                    new ProductOverviewToRankingList(
                        (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList(),
                        rankedActions.EventId));
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(
                    $"Failed to get recommendations: Exception type:{e.GetType()} and message:{e.Message}", e);
                return null;
            }
        }

        /// <summary>
        ///     Calculates and sends a reward to a given Event.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="orderIDs"></param>
        /// <param name="additionalData"></param>
        /// <returns>An empty task.</returns>
        private async Task ProcessRewardAsync (string eventID, string orderIDs, object additionalData)
        {
            try
            {
                ProductDetailsModel product = additionalData is ProductDetailsModel model ? model : null;
                if (string.IsNullOrEmpty(eventID) || string.IsNullOrEmpty(orderIDs) || product == null)
                {
                    return;
                }

                string[] products = orderIDs.Split("-");
                for (int i = 0; i < products.Length; i++)
                {
                    if (products[i].Equals(product.Id.ToString(new CultureInfo("en-UK")),
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        await _personalizerClientService.RewardAnActionAsync(eventID, i);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync($"Failed to reward event with EventId:{eventID}", e);
            }
        }

        #endregion
    }
}
