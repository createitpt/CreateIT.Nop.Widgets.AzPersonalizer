namespace Nop.Plugin.Widgets.AzPersonalizer.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Nop.Core;
    using Nop.Core.Domain.Catalog;
    using Nop.Services.Catalog;

    /// <summary>
    ///     Obtains a list of any 50 products
    /// </summary>
    public class DefaultListOfProductsRequester : ProductListRequester
    {

        #region methods

        /// <summary>
        ///     Returns a list of 50 random products.
        /// </summary>
        /// <returns>List of products</returns>
        public async override Task<List<Product>> HandleAsync ()
        {
            IPagedList<Product> products = await _productService.SearchProductsAsync(0, MAX_NUMBER_OF_ELEMENTS);
            return products.ToList();
        }

        #endregion

        #region fields

        private const int MAX_NUMBER_OF_ELEMENTS = 50;
        private readonly IProductService _productService;

        #endregion

        #region ctor

        public DefaultListOfProductsRequester (IProductService productService)
        {
            _productService = productService;
        }

        public IProductTagService ProductTagService { get; }

        #endregion
    }
}
