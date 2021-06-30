using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.AzPersonalizer.Models
{
    /// <summary>
    /// Represents a product and information for the reward on Personalizer.
    /// </summary>
    public record ProductOverviewReccomendation
    {
        public ProductOverviewModel ProductOverviewModel;
        public string RewardID { get; set; }

        public int Position { get; set; }

        public ProductOverviewReccomendation(ProductOverviewModel productOverviewModel, string rewardID, int position)
        {
            ProductOverviewModel = productOverviewModel;
            RewardID = rewardID;
            Position = position;
        }

    }
}
