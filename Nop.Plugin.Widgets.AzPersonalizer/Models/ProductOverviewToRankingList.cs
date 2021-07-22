namespace Nop.Plugin.Widgets.AzPersonalizer.Models
{

    using System.Collections.Generic;
    using Nop.Web.Models.Catalog;

    /// <summary>
    ///     Represents a ranking and the list of the products ranked
    /// </summary>
    public record ProductOverviewToRankingList
    {

        public string RankingID { get; set; }
        public List<ProductOverviewModel> ProductOverviewList { get; }

        public ProductOverviewToRankingList (List<ProductOverviewModel> productOverviewList, string rankingID)
        {
            ProductOverviewList = productOverviewList;
            RankingID = rankingID;
        }
    }
}
