namespace Nop.Plugin.Widgets.AzPersonalizer.Data
{

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Nop.Core.Domain.Catalog;
    using Nop.Services.Catalog;
    using Nop.Services.Logging;

    /// <summary>
    ///     Obtains a List of products directly specified as 'Related' on the admin page.
    /// </summary>
    public class RelatedProductsRequester : ProductListRequester
    {

        #region fields

        private readonly IProductService _productService;
        private readonly int _productID;
        private readonly ILogger _logger;

        #endregion

        #region ctor

        public RelatedProductsRequester (int productID, ILogger logger, IProductService productService)
        {
            _logger = logger;
            _productID = productID;
            _productService = productService;
        }

        #endregion

        #region methods

        /// <summary>
        ///     Returns a List of products directly specified as 'Related' on the admin page.
        /// </summary>
        /// <returns>List of products.</returns>
        public async override Task<List<Product>> HandleAsync ()
        {
            IList<RelatedProduct> relatedProducts =
                await _productService.GetRelatedProductsByProductId1Async(_productID);

            if (relatedProducts.Count == 0)
            {
                return await base.HandleAsync();
            }

            try
            {
                List<Product> productsList = new();
                foreach (RelatedProduct rp in relatedProducts)
                {
                    productsList.Add(await _productService?.GetProductByIdAsync(rp.ProductId2));
                }

                return productsList;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(
                    $"failed fetching product list of products with same Category. The cause was{e.GetType()}", e);
                throw;
            }
        }

        #endregion
    }
}
