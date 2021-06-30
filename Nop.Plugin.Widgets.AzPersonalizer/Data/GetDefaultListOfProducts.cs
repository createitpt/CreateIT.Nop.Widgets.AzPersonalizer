using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Widgets.AzPersonalizer.Data
{
    /// <summary>
    /// Obtains a list of any 50 products
    /// </summary>
    class GetDefaultListOfProducts : AbstractGetProductList
    {
       
        #region fields
        #endregion

        #region ctor
        public GetDefaultListOfProducts(int productID,
            ILogger logger,
            IStoreContext storeContext,
            IWorkContext workContext,
            ISettingService settingService,
            IProductService productService,
            IProductTagService productTagService,
            ICategoryService productCategoryService) : base(productID, logger, storeContext, workContext, settingService, productService, productTagService, productCategoryService)
        {
            ProductTagService = productTagService;
        }

        public IProductTagService ProductTagService { get; }
        #endregion

        #region methods
        /// <summary>
        /// Returns a list of 50 random products.
        /// </summary>
        /// <returns>List of products</returns>
        public async override Task<List<Product>> Handle()
        {
            var products = await _productService.SearchProductsAsync(0, 50);
            return products.ToList();
        }
        #endregion
    }
}
