using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.AzPersonalizer.Controllers;
using Nop.Plugin.Widgets.AzPersonalizer.Models;
using Nop.Plugin.Widgets.AzPersonalizer.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.AzPersonalizer.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages 
    /// </summary>
    [ViewComponent(Name = "WidgetsAzPersonalizer")]
    public class WidgetsAzPersonalizerViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ILogger _logger;
        private readonly PersonalizerClientService _personalizerClientService;
        private static bool debugging = true;


        public WidgetsAzPersonalizerViewComponent(IStoreContext storeContext, 
            ISettingService settingService,
            PersonalizerClientService personalizerClientService,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IWebHelper webHelper,
            ILogger logger)
        {
            _logger = logger;
            _storeContext = storeContext;
            _settingService = settingService;
            _webHelper = webHelper;
            _personalizerClientService = personalizerClientService;
            _productService = productService;
            _productModelFactory = productModelFactory;
        }

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {    
            var azPersonalizerSettings = await _settingService.LoadSettingAsync<AzPersonalizerSettings>((await _storeContext.GetCurrentStoreAsync()).Id);
            if (widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialBottom))
            {
                string[] url = (_webHelper.GetThisPageUrl(true))?.Split("?");
                if (TempData["RewardID"] != null
                    && TempData["OrderedIDs"] != null
                    && url.Length > 1 && url[1].Contains("azrec")) // TODO remover url?
                {
                    string id = TempData["RewardID"] is string rewardID ? rewardID : null;
                    string pos = TempData["OrderedIDs"] is string position ? position : null;
                    
                    await ProcessRewardAsync(id, pos, additionalData);
                }      
                return await ReccomendedList(azPersonalizerSettings, additionalData);

            }
            else
            {
                // While there is only one widget zone with reccomendations doesn't return anything. But to add
                // recomendations to multiple widget zones should add else if's.
                return View();
            }
            
        }


        /// <summary>
        /// Prepares the view component with a list of Ranked Actions
        /// </summary>
        /// <param name="azPersonalizerSettings"></param>
        /// <param name="additionalData"></param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        private async Task<IViewComponentResult> ReccomendedList(AzPersonalizerSettings azPersonalizerSettings, object additionalData)
        {
            
            try
            {
                var a = await _personalizerClientService?.getRankedActions(additionalData);
                var model = a.Ranking;
                TempDataTokenController tdt = new TempDataTokenController(a.EventId); // Design mau?
                TempData["RewardID"] = a.EventId;
                IList<Product> products = new List<Product>(model.Count);
                string odds = a.EventId + ": ";
                for(int i  = 0; i< model.Count; i++)
                {
                   odds +=("(" + model[i]?.Id + "," + model[i].Probability +");");
                   products.Insert(i,await _productService?.GetProductByIdAsync(int.Parse(model[i]?.Id))); 
                   tdt.AddId(model[i]?.Id);
                }
                TempData["OrderedIDs"] = tdt.OrderedIDs;
                if(debugging) await _logger.WarningAsync(odds);

                return View("~/Plugins/Widgets.AzPersonalizer/Views/Reccomendations.cshtml", new ProductOverviewToRankingList( (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList(), a.EventId));
        
            }catch(Exception e)
            {
                await _logger.ErrorAsync("Failed to get reccomendations");
                return View();
            }
            
        }

        /// <summary>
        /// Calculates and sends a reward to a given Event.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="OrderIDs"></param>
        /// <param name="additionalData"></param>
        /// <returns>An empty task.</returns>
        private async Task ProcessRewardAsync(string eventID, string  OrderIDs, object additionalData)
        {
            var product = additionalData is ProductDetailsModel model ? model : null;
            if (string.IsNullOrEmpty(eventID) || string.IsNullOrEmpty(OrderIDs) || product == null)
                return;
            string[] products = OrderIDs.Split("-");
            for(int i = 0;i<products.Length; i++)
            {
                if(products[i] == product.Id.ToString()) // TODO mudar para .Contains?
                {
                    await _personalizerClientService.RewardAnAction(eventID, i);
                    return;
                }
            }
            
        }

    }
}
