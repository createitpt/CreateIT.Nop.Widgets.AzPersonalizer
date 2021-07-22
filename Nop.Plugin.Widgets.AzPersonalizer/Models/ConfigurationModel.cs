namespace Nop.Plugin.Widgets.AzPersonalizer.Models
{
    using Nop.Web.Framework.Models;
    using Nop.Web.Framework.Mvc.ModelBinding;

    /// <summary>
    ///     Represents configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {

       [NopResourceDisplayName("Plugins.Widgets.AzPersonalizer.ApiKey")]
        public string APIkey { get; set; }

        public bool APIKey_OverrideForStore { get; set; }

       [NopResourceDisplayName("Plugins.Widgets.AzPersonalizer.Endpoint")]
        public string Endpoint { get; set; }

        public bool Endpoint_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
