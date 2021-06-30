using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Personalizer;
using Microsoft.Azure.CognitiveServices.Personalizer.Models;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Widgets.AzPersonalizer.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Vendors;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.AzPersonalizer.Services
{
    /// <summary>
    /// Personalizer Service
    /// </summary>
    public record PersonalizerClientService
    {

        private const double WHEIGHT = 10.0;
        #region fields
        private PersonalizerClient _client;
        private AzPersonalizerSettings _azPersonalizerSettings; 

        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IProductTagService _productTagService;
        private readonly ICategoryService _productCategoryService;
        private readonly IVendorService _vendorService;
        
        #endregion

        #region ctor
        public  PersonalizerClientService(
            ILogger logger,
            IStoreContext storeContext,
            IVendorService vendorService,
            IWorkContext workContext,
            ISettingService settingService,
            IProductService productService,
            IProductTagService productTagService,
            ICategoryService productCategoryService
)
        {

            _logger = logger;
            _storeContext = storeContext;
            _workContext = workContext;
            _settingService = settingService;
            _productService = productService;
            _productCategoryService = productCategoryService;
            _productTagService = productTagService;
            _vendorService = vendorService;

        }


        #endregion

        #region methods

        /// <summary>
        /// Prepares the necessary information to call Rank. This Includes a list up to 50 products, user information and user's enviornment information
        /// </summary>
        /// <param name="additionalData">Additional Data from the Product page</param>
        /// <returns>A Task with the RankResponse</returns>
        public async Task<RankResponse> getRankedActions(object additionalData)
        {
           
            var product = additionalData is ProductDetailsModel model ? model : null;
            if (product == null)
                return null;
            await SetClientAsync(); 
            int productID = product.Id;
            SimilarProductsRequester similarProductsRequester = new SimilarProductsRequester(_logger, _storeContext, _workContext, _settingService, _productService, _productTagService, _productCategoryService);

            //Get the Actions
            IList<Product> aux = await similarProductsRequester.Register(productID); 
            IList<RankableAction> temp = await GetRankableActions(aux, productID); // Lista de Id's de Produtos

            //Get User Context
            IList<object> context = await GetUserContext(productID);


            var request = new RankRequest(temp, context, new List<string> {product?.Id.ToString()}); // nota ja nao preciso de excluded actions
            var aux2 = _client.Rank(request);
            return aux2;
        }


        public async Task RewardAnAction(string id, int pos)
        {
            await SetClientAsync();
            double reward = 1.0/(Convert.ToDouble(pos)*WHEIGHT + 1.0);

            string msg = "Reward to Ranking Id: " + id + " is " + reward;
            await _logger.WarningAsync(msg);

            await _client.RewardAsync(id, new RewardRequest(reward));
        }

        /// <summary>
        /// Updates the Personalizer Client.
        /// </summary>
        /// <returns></returns>
        private async Task SetClientAsync()
        {
            _azPersonalizerSettings = await _settingService.LoadSettingAsync<AzPersonalizerSettings>((await _storeContext.GetCurrentStoreAsync()).Id);
            if (string.IsNullOrEmpty(_azPersonalizerSettings.APIkey.Trim()) || string.IsNullOrEmpty(_azPersonalizerSettings.Endpoint.Trim()))
                throw new Exception("Plugin Not Configured.");
            _client = new PersonalizerClient(
                                new ApiKeyServiceClientCredentials(_azPersonalizerSettings.APIkey))
                                { 
                                     Endpoint = _azPersonalizerSettings.Endpoint 
                                };
        }


        //Relevant for feature selection: https://docs.microsoft.com/en-us/azure/cognitive-services/personalizer/concepts-features


        /// <summary>
        /// Transforms a List of Products in a list of RankableActions.
        /// </summary>
        /// <param name="related"></param>
        /// <returns>List with the RankableActions</returns>
        private async Task<IList<RankableAction>> GetRankableActions(IList<Product> related, int productID)
        {
            IList<RankableAction> ranked = new List<RankableAction>();
            foreach(Product r in related)
            {
                if (ranked.Count == 50) break;
                if(r.Id != productID)
                { 
           
                    var categories = await _productCategoryService.GetProductCategoriesByProductIdAsync(r.Id);
                    var categoriesIndex = new List<int>();
                    foreach(ProductCategory c in categories)
                    {
                        categoriesIndex.Add( c.CategoryId );
                    }

                
                    var priceTier = await _productService.GetTierPriceByIdAsync(r.Id);
                    decimal price = (priceTier != null)? priceTier.Price : -1;
                
                    Vendor vendor = (await _vendorService.GetVendorByIdAsync(r.VendorId));

                    ranked.Add(new RankableAction
                    {
                        Id = r.Id.ToString(),
                        Features = new List<object> {
                            new {
                                Categories = categoriesIndex,
                                price = price,
                                has_discount = r.HasDiscountsApplied,
                                vendor = (vendor==null)? "No Vendor Available": vendor.Name,
                                rating = r.ApprovedRatingSum
                            }
                        }
                    });
                   
                }
            }

            return ranked;
        }

        /// <summary>
        /// Prepares the user context information. Should only return non personal information.
        /// </summary>
        /// <param name="productID"></param>
        /// <returns>List with user context features as objects</returns>
        private async Task<IList<object>> GetUserContext(int productID)
        {
            Customer customer = await _workContext.GetCurrentCustomerAsync();
            Language lang = await _workContext.GetWorkingLanguageAsync();
            Currency currency = await _workContext.GetWorkingCurrencyAsync();
            Store store = await _storeContext.GetCurrentStoreAsync();

            IList<object> features;
            double last_act = Math.Floor(DateTime.Now.Subtract(customer.LastActivityDateUtc).TotalDays);
            if (customer != null)
            {
                features = new List<object>()
                {
                   new {store = (store != null)? store.Id.ToString() : "Unknown" },
                   new {current_product= productID},
                   new {currency_used = currency.Name},
                   new {language = lang.Name},
                   new {logged = customer.Active},
                   new {Last_activity = (last_act < 1.0)? "Today":last_act.ToString()}
                  
                };

            }
            else
            {
                features = new List<object>
                {
                    new {current_product= productID},
                    new {logged = false},
                    new {currency_used = currency.Name},
                    new {language = lang.Name}
                };
            }

            return features;
        }
        #endregion
    }
}
