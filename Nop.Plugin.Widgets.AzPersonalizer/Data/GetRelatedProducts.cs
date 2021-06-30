using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Widgets.AzPersonalizer.Data
{
    /// <summary>
    /// Obtains a List of products directly specified as 'Related' on the admin page.
    /// </summary>
    class GetRelatedProducts : AbstractGetProductList
    {
        #region fields
        #endregion

        #region ctor
        public GetRelatedProducts(int productID,
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
        /// Returns a List of products directly specified as 'Related' on the admin page.
        /// </summary>
        /// <returns>List of products.</returns>
        public async override Task<List<Product>> Handle()
        {

            IList<RelatedProduct> aux =  (await base._productService.GetRelatedProductsByProductId1Async(base._productID));
           
            if(aux.Count == 0){
                return await base.Handle();
            }
            else
            {
                List<Product> productsList = new List<Product>();
                foreach (RelatedProduct rp in aux)
                {
                    productsList.Add(await _productService?.GetProductByIdAsync(rp.ProductId2));
                }


                return productsList;
            }

        }   

        #endregion
    }
}
