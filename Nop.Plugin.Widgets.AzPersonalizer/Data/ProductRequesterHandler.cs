namespace Nop.Plugin.Widgets.AzPersonalizer.Data
{

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Nop.Core.Domain.Catalog;
    using Nop.Plugin.Widgets.AzPersonalizer.Utils.Handler;
    using Nop.Services.Catalog;
    using Nop.Services.Logging;

    /// <summary>
    ///     Obtains a list of ptoducts using chain of responsibility pattern
    /// </summary>
    public class ProductRequesterHandler : Handler<string>, IHandler<string>
    {

        #region fields

        private readonly IProductService _productService;
        private readonly ICategoryService _productCategoryService;
        private readonly ILogger _logger;

        #endregion

        #region ctor

        public ProductRequesterHandler (IProductService productService,
            ICategoryService productCategoryService,
            ILogger logger)
        {
            _logger = logger;
            _productService = productService;
            _productCategoryService = productCategoryService;
        }

        #endregion

        #region methods

        /// <summary>
        ///     Returns a list of products.
        /// </summary>
        /// <param name="productID">Product ID to which similar products should be returned.</param>
        /// <returns>List of products.</returns>
        public async Task<List<Product>> RegisterAsync (int productID)
        {
            try
            {
                RelatedProductsRequester handler = new(productID, _logger, _productService);
                handler.SetNext(new ProductsWithSameCategoryRequester(productID, _logger, _productService, _productCategoryService))
                    .SetNext(new DefaultListOfProductsRequester(_productService));
                return await handler.HandleAsync();
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(
                    $"Product Request Handler failed at aquiring product list. The cause was{e.GetType()}", e);
                throw;
            }
        }

        #endregion

    }
}
