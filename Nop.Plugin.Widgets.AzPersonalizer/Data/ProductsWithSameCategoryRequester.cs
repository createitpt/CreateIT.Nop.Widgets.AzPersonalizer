namespace Nop.Plugin.Widgets.AzPersonalizer.Data
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Nop.Core;
    using Nop.Core.Domain.Catalog;
    using Nop.Services.Catalog;
    using Nop.Services.Logging;

    /// <summary>
    ///     Obtains a list of products with the same category
    /// </summary>
    public class ProductsWithSameCategoryRequester : ProductListRequester
    {

        #region fields

        private const int MAX_NUMBER_OF_ELEMENTS = 50;

        private readonly int _productID;
        private readonly ICategoryService _productCategoryService;
        private readonly IProductService _productService;
        private readonly ILogger _logger;

        #endregion

        #region ctor

        public ProductsWithSameCategoryRequester (int productID,
            ILogger logger,
            IProductService productService,
            ICategoryService productCategoryService)
        {
            _logger = logger;
            _productID = productID;
            _productService = productService;
            _productCategoryService = productCategoryService;
        }

        #endregion

        #region methods

        /// <summary>
        ///     Returns a list of products with the same category.
        ///     If the product we are getting the list for is the only one in all his categories, then
        ///     this method advances to the next handler.
        /// </summary>
        /// <returns>List of products</returns>
        public async override Task<List<Product>> HandleAsync ()
        {
            try
            {
                IList<ProductCategory> categories =
                    await _productCategoryService.GetProductCategoriesByProductIdAsync(_productID);
                if (categories.Count > 0)
                {
                    List<Product> productsWithSameCategories = new();
                    for (int i = 0;
                        i < categories.Count && productsWithSameCategories.Count < MAX_NUMBER_OF_ELEMENTS;
                        i++)
                    {
                        // In a subsequent processing I make shure there are only 50 products. See PersonalizerClientService.GetRankableActions

                        IList<Product> featuredProducts =
                            await _productService.GetCategoryFeaturedProductsAsync(categories[i].CategoryId);
                        if (featuredProducts.Count == 0 || featuredProducts.Count == 1)
                        {
                            IPagedList<ProductCategory> productsAlsoWithPCList =
                                await _productCategoryService.GetProductCategoriesByCategoryIdAsync(
                                    categories[i].CategoryId);
                            for (int j = 0; j < productsAlsoWithPCList.Count && productsWithSameCategories.Count < MAX_NUMBER_OF_ELEMENTS; j++)
                            {
                                ProductCategory productAlsoWithPC = productsAlsoWithPCList[j];
                                if (productsWithSameCategories.Count >= MAX_NUMBER_OF_ELEMENTS)
                                {
                                    break;
                                }

                                if (productAlsoWithPC.ProductId != _productID)
                                {
                                    productsWithSameCategories.Add(
                                        await _productService.GetProductByIdAsync(productAlsoWithPC.ProductId));
                                }
                            }
                        }
                        else
                        {
                            productsWithSameCategories = productsWithSameCategories.Union(featuredProducts).ToList();
                        }
                    }

                    if (productsWithSameCategories.Count > 0)
                    {
                        return productsWithSameCategories;
                    }
                }

                return await base.HandleAsync();
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync($"Failed fetching list of products in the same category. The cause was{e.GetType()}", e);
                throw;
            }
        }

        #endregion

    }
}
