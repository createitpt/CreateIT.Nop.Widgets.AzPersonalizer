using System.Collections.Generic;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.AzPersonalizer.Models
{
    /// <summary>
    /// Represents a ranking and the list of the products ranked
    /// </summary>
    public record ProductOverviewToRankingList
    {
        public string RankingID;
        public List<ProductOverviewModel> ProductOverviewList;

        public ProductOverviewToRankingList(List<ProductOverviewModel> productOverviewList, string rankingID)
        {
            ProductOverviewList = productOverviewList;
            RankingID = rankingID;
        }

        

    }
}
