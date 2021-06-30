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
    public abstract class AbstractGetProductList : Handler<List<Product>>, IHandler<List<Product>>
    {
        #region fields
        protected int _productID;
        protected PersonalizerClientService _personalizerClientService;
        protected readonly IProductService _productService;
        protected readonly ISettingService _settingService;
        protected readonly IProductTagService _productTagService;
        protected readonly ICategoryService _productCategoryService;
        protected readonly IStoreContext _storeContext;
        protected readonly IWorkContext _workContext;
        protected readonly ILogger _logger;
        #endregion

        #region ctor
        public AbstractGetProductList(int productID,
            ILogger logger,
            IStoreContext storeContext,
            IWorkContext workContext,
            ISettingService settingService,
            IProductService productService,
            IProductTagService productTagService,
            ICategoryService productCategoryService)
        {

            _productID = productID;
           
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
        /// Returns a list of products
        /// </summary>
        /// <returns>List of products</returns>
        public async override Task<List<Product>> Handle()
        {
            return await base.Handle();
        }
        #endregion
    }
}
