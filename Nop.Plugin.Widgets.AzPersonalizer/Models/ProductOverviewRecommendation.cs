namespace Nop.Plugin.Widgets.AzPersonalizer.Models
{

    using Nop.Web.Models.Catalog;

    /// <summary>
    ///     Represents a product and information for the reward on Personalizer.
    /// </summary>
    public record ProductOverviewRecommendation
    {

        public ProductOverviewModel ProductOverviewModel { get; set; }
        public string RewardID { get; set; }
        public int Position { get; set; }

        public ProductOverviewRecommendation (ProductOverviewModel productOverviewModel, string rewardID,
            int position)
        {
            ProductOverviewModel = productOverviewModel;
            RewardID = rewardID;
            Position = position;
        }

    }
}
