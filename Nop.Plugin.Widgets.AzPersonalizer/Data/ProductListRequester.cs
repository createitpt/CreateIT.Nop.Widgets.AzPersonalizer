namespace Nop.Plugin.Widgets.AzPersonalizer.Data
{

    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Nop.Core.Domain.Catalog;
    using Nop.Plugin.Widgets.AzPersonalizer.Utils.Handler;

    /// <summary>
    ///     Obtains a list of ptoducts using chain of responsibility pattern
    /// </summary>
    public abstract class ProductListRequester : Handler<List<Product>>, IHandler<List<Product>>
    {

        #region methods

        /// <summary>
        ///     Returns a list of products
        /// </summary>
        /// <returns>List of products</returns>
        public async override Task<List<Product>> HandleAsync () => await base.HandleAsync();

        #endregion

    }
}
