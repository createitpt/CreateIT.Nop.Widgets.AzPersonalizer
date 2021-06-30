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
    /// Obtains a list of products with the same category
    /// </summary>
    class GetProductsWithSameCategory : AbstractGetProductList
    {
        #region fields
       
        #endregion

        #region ctor
        public GetProductsWithSameCategory(int productID,
            ILogger logger,
            IStoreContext storeContext,
            IWorkContext workContext,
            ISettingService settingService,
            IProductService productService,
            IProductTagService productTagService,
            ICategoryService productCategoryService) : base(productID, logger, storeContext, workContext, settingService, productService, productTagService, productCategoryService)
        {
           
        }
        #endregion

        #region methods
        /// <summary>
        /// Returns a list of products with the same category.
        /// If the product we are getting the list for is the only one in all his categories, then 
        /// this method advances to the next handler.
        /// </summary>
        /// <returns>List of products</returns>
        public async override Task<List<Product>> Handle()
        {
            IList<ProductCategory> categories = await base._productCategoryService.GetProductCategoriesByProductIdAsync(base._productID);
            if(categories.Count > 0)
            {
                int i = 0;
                List<Product> aux = new List<Product>();
                foreach(ProductCategory pc in categories)
                {
                    if (aux.Count >= 50) break; // In a subsequent processing I make shure there are only 50 products. See PersonalizerClientService.GetRankableActions

                    var aux2 = await _productService.GetCategoryFeaturedProductsAsync(pc.CategoryId);
                    if (aux2.Count == 0 || aux2.Count == 1)
                    {
                       foreach(ProductCategory productAlsoWithPC in await _productCategoryService.GetProductCategoriesByCategoryIdAsync(pc.CategoryId))
                       {
                            if (aux.Count >= 50) break;
                            if(productAlsoWithPC.ProductId != base._productID)
                                aux.Add(await _productService.GetProductByIdAsync(productAlsoWithPC.ProductId));
                       }
                    }
                    else
                    {
                        aux.Union(aux2 );
                    }

                }
                if (aux.Count > 0)
                    return aux;
            }

            return await base.Handle();
        }
        #endregion
    }
}
