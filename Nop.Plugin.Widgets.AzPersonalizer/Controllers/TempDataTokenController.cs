namespace Nop.Plugin.Widgets.AzPersonalizer.Controllers
{
    using Microsoft.AspNetCore.Mvc;


    public class TempDataTokenController : Controller
    {

       [TempDataAttribute] public string RewardID { set; get; }
       [TempDataAttribute] public string OrderedIDs { set; get; }

        public TempDataTokenController (string rewardID)
        {
            RewardID = rewardID;
            OrderedIDs = "";
        }

        /// <summary>
        ///     Builds the Id's to store on the temp data.
        /// </summary>
        /// <param name="id"></param>
        public void AddId (string id)
        {
            if (string.IsNullOrEmpty(OrderedIDs))
            {
                OrderedIDs = "" + id;
            }
            else
            {
                OrderedIDs = OrderedIDs + "-" + id;
            }
        }
    }
}
