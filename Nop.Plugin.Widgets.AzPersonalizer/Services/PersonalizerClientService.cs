namespace Nop.Plugin.Widgets.AzPersonalizer.Services
{

    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Personalizer;
    using Microsoft.Azure.CognitiveServices.Personalizer.Models;
    using Nop.Core;
    using Nop.Core.Domain.Catalog;
    using Nop.Core.Domain.Customers;
    using Nop.Core.Domain.Directory;
    using Nop.Core.Domain.Localization;
    using Nop.Core.Domain.Logging;
    using Nop.Core.Domain.Stores;
    using Nop.Core.Domain.Vendors;
    using Nop.Plugin.Widgets.AzPersonalizer.Data;
    using Nop.Plugin.Widgets.AzPersonalizer.Infrastructure.Exceptions;
    using Nop.Services.Catalog;
    using Nop.Services.Configuration;
    using Nop.Services.Logging;
    using Nop.Services.Vendors;
    using Nop.Web.Models.Catalog;

    /// <summary>
    ///     Personalizer Service
    /// </summary>
    public record PersonalizerClientService : IDisposable
    {

        #region fields
        private const double WHEIGHT = 10.0;
        private const int MAX_RANKABLE_ACTIONS = 50;

        // Note: Field is disposable, but I restore the connection whenever I need it.
        private PersonalizerClient _client;
        private AzPersonalizerSettings _azPersonalizerSettings;

        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly ICategoryService _productCategoryService;
        private readonly IVendorService _vendorService;


        private bool _isDisposed;
        private IntPtr _nativeResource = Marshal.AllocHGlobal(100);

        #endregion

        #region ctor

        public PersonalizerClientService (
            ILogger logger,
            IStoreContext storeContext,
            IVendorService vendorService,
            IWorkContext workContext,
            ISettingService settingService,
            IProductService productService,
            ICategoryService productCategoryService
        )
        {
            _logger = logger;
            _storeContext = storeContext;
            _workContext = workContext;
            _settingService = settingService;
            _productService = productService;
            _productCategoryService = productCategoryService;
            _vendorService = vendorService;
        }

        #endregion

        #region methods

        /// <summary>
        ///     Prepares the necessary information to call Rank. This Includes a list up to 50 products, user information and
        ///     user's enviornment information
        /// </summary>
        /// <param name="additionalData">Additional Data from the Product page</param>
        /// <returns>A Task with the RankResponse</returns>
        public async Task<RankResponse> GetRankedActionsAsync (object additionalData)
        {
            try
            {
                ProductDetailsModel product = additionalData is ProductDetailsModel model ? model : null;
                if (product == null)
                {
                    return null;
                }

                await SetClientAsync();
                int productID = product.Id;
                ProductRequesterHandler similarProductsRequester =
                    new(_productService, _productCategoryService, _logger);

                IList<Product> productsList;
                IList<RankableAction> rankableActionsList;
                IList<object> context;

                // Get the Actions(Products)
                productsList = await similarProductsRequester.RegisterAsync(productID);
                // List with the rankable actions for Rank call
                rankableActionsList = await GetRankableActionsAsync(productsList, productID);
                //Get User Context
                context = await GetUserContextAsync(productID);
                RankRequest request = new(rankableActionsList, context, new List<string> {
                    product?.Id.ToString(new System.Globalization.CultureInfo("en-UK"))
                });

                // For Debugging purposes I dont imediatly return client.Rank(request);
                RankResponse rankResponse = _client.Rank(request);
                return rankResponse;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync("Failed while preparing the context for product", e);
                throw;
            }
        }


        public async Task RewardAnActionAsync (string id, int pos)
        {
            await SetClientAsync();
            double reward = 1.0 / (Convert.ToDouble(pos) * WHEIGHT + 1.0);

            string msg = "Reward to Ranking Id: " + id + " is " + reward;
            await _logger.InsertLogAsync(LogLevel.Debug, msg);

            try
            {
                await _client.RewardAsync(id, new RewardRequest(reward));
            }
            catch (Exception e)
            {
                // Nao encontro nenhuma excecao lancada pelo Rank, nem na documentacao da API, nem nos projetos exemplo em https://github.com/Azure-Samples/cognitive-services-personalizer-samples.git
                await _logger.ErrorAsync(
                    $"Reward of product with EventId:{id} failed on calling the Reward function from Personalizer API.", e);
                throw;
            }
        }

        /// <summary>
        ///     Updates the Personalizer Client.
        /// </summary>
        /// <returns>An empty task</returns>
        private async Task SetClientAsync ()
        {
            try
            {
                _azPersonalizerSettings =
                    await _settingService.LoadSettingAsync<AzPersonalizerSettings>(
                        (await _storeContext.GetCurrentStoreAsync()).Id);
                if (string.IsNullOrEmpty(_azPersonalizerSettings.APIkey.Trim()) ||
                    string.IsNullOrEmpty(_azPersonalizerSettings.Endpoint.Trim()))
                {
                    throw new PluginNotConfiguredException("Plugin Not Configured.");
                }

                _client = new PersonalizerClient(
                    new ApiKeyServiceClientCredentials(_azPersonalizerSettings.APIkey)) {
                    Endpoint = _azPersonalizerSettings.Endpoint
                };
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(
                    $"failed at Setting the connection to Personalizer Client. Exception:{e.GetType()}, {e.Message}", e);
                throw;
            }
        }


        //Relevant for feature selection: https://docs.microsoft.com/en-us/azure/cognitive-services/personalizer/concepts-features
        /// <summary>
        ///     Transforms a List of Products in a list of RankableActions. Related can only have up to 50 members.
        /// </summary>
        /// <param name="related">List with at most 50 members</param>
        /// <returns>List with the RankableActions</returns>
        private async Task<IList<RankableAction>> GetRankableActionsAsync (IList<Product> related, int productID)
        {
            try
            {
                IList<RankableAction> ranked = new List<RankableAction>();
                for (int i = 0; i < related.Count; i++)
                {
                    Product relatedProduct = related[i];
                    if (ranked.Count == MAX_RANKABLE_ACTIONS)
                    {
                        break;
                    }

                    if (relatedProduct.Id != productID)
                    {
                        IList<ProductCategory> categories =
                            await _productCategoryService.GetProductCategoriesByProductIdAsync(relatedProduct.Id);
                        List<int> categoriesIndex = new();

                        foreach (ProductCategory c in categories)
                        {
                            categoriesIndex.Add(c.CategoryId);
                        }

                        TierPrice priceTier = await _productService.GetTierPriceByIdAsync(relatedProduct.Id);
                        decimal price = (priceTier != null) ? priceTier.Price : -1;
                        Vendor vendor = (await _vendorService.GetVendorByIdAsync(relatedProduct.VendorId));

                        ranked.Add(new RankableAction {
                            Id = relatedProduct.Id.ToString(new System.Globalization.CultureInfo("en-UK")),
                            Features = new List<object> {
                                new {
                                    Categories = categoriesIndex,
#pragma warning disable IDE0037 // Use inferred member name
                                    price = price,
#pragma warning restore IDE0037 // Use inferred member name
                                    has_discount = relatedProduct.HasDiscountsApplied,
                                    vendor = (vendor == null) ? "No Vendor Available" : vendor.Name,
                                    rating = relatedProduct.ApprovedRatingSum
                                }
                            }
                        });
                    }
                }

                return ranked;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(
                    $"failed at getting Related Actions with Exception:{e.GetType()}, {e.Message}", e);
                throw;
            }
        }

        /// <summary>
        ///     Prepares the user context information. Should only return non personal information.
        /// </summary>
        /// <param name="productID"></param>
        /// <returns>List with user context features as objects</returns>
        private async Task<IList<object>> GetUserContextAsync (int productID)
        {
            try
            {
                Customer customer = await _workContext.GetCurrentCustomerAsync();
                Language lang = await _workContext.GetWorkingLanguageAsync();
                Currency currency = await _workContext.GetWorkingCurrencyAsync();
                Store store = await _storeContext.GetCurrentStoreAsync();

                IList<object> features;
                double last_act = Math.Floor(DateTime.Now.Subtract(customer.LastActivityDateUtc).TotalDays);
                if (customer != null)
                {
                    features = new List<object>() {
                        new {
                            store = (store != null)
                                ? store.Id.ToString(new System.Globalization.CultureInfo("en-UK"))
                                : "Unknown"
                        },
                        new {current_product = productID},
                        new {currency_used = currency.Name},
                        new {language = lang.Name},
                        new {logged = customer.Active},
                        new {
                            Last_activity = (last_act < 1.0)
                                ? "Today"
                                : last_act.ToString(new System.Globalization.CultureInfo("en-UK"))
                        }
                    };
                }
                else
                {
                    features = new List<object> {
                        new {current_product = productID},
                        new {logged = false},
                        new {currency_used = currency.Name},
                        new {language = lang.Name}
                    };
                }

                return features;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync($"Failed to obtain context, exception:{e.GetType()}, {e.Message}", e);
                return null;
            }
        }

        #region IDisposable Methods

        /// <summary>
        /// </summary>
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // free managed resources
                _client.Dispose();
            }

            // free native resources if there are any.
            if (_nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_nativeResource);
                _nativeResource = IntPtr.Zero;
            }

            _isDisposed = true;
        }

        #endregion

        #endregion
    }
}
