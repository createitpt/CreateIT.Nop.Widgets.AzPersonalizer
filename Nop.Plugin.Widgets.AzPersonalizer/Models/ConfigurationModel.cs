using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.AzPersonalizer.Models
{
    /// <summary>
    /// Represents configuration model
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