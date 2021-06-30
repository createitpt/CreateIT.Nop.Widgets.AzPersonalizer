using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.AzPersonalizer.Services;
using Nop.Plugin.Widgets.AzPersonalizer.Utils.Handler;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Widgets.AzPersonalizer.Data
{
    /// <summary>
    /// Obtains a list of ptoducts using chain of responsibility pattern
    /// </summary>
    class SimilarProductsRequester : Handler<String>, IHandler<String>
    {
        #region fields
        PersonalizerClientService _personalizerClientService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IProductTagService _productTagService;
        private readonly ICategoryService _productCategoryService;
        #endregion

        #region ctor
        public SimilarProductsRequester(
            ILogger logger,
            IStoreContext storeContext,
            IWorkContext workContext,
            ISettingService settingService,
            IProductService productService,
            IProductTagService productTagService,
            ICategoryService productCategoryService)
        {

            _logger = logger;
            _storeContext = storeContext;
            _workContext = workContext;
            _settingService = settingService;
            _productService = productService;
            _productCategoryService = productCategoryService;
            _productTagService = productTagService;
        }
        #endregion

        #region methods
        /// <summary>
        /// Returns a list of products.
        /// </summary>
        /// <param name="productID">Product ID to which similar products should be returned.</param>
        /// <returns>List of products.</returns>
        public async Task<List<Product>> Register(int productID)
        {
            var handler = new GetRelatedProducts(productID, _logger, _storeContext, _workContext, _settingService, _productService, _productTagService, _productCategoryService);
            handler.SetNext(new GetProductsWithSameCategory(productID, _logger, _storeContext, _workContext, _settingService, _productService, _productTagService, _productCategoryService))
                .SetNext(new GetDefaultListOfProducts(productID, _logger, _storeContext, _workContext, _settingService, _productService, _productTagService, _productCategoryService));
            
           return await handler.Handle();
        }
    #endregion
    }
}
