namespace Nop.Plugin.Widgets.AzPersonalizer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Nop.Core;
    using Nop.Services.Cms;
    using Nop.Services.Configuration;
    using Nop.Services.Localization;
    using Nop.Services.Logging;
    using Nop.Services.Plugins;
    using Nop.Web.Framework.Infrastructure;

    /// <summary>
    ///     Represents the plugin implementation
    /// </summary>
    public class AzPersonalizerPlugin : BasePlugin, IWidgetPlugin
    {

        #region fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;

        #endregion

        #region ctor

        public AzPersonalizerPlugin (ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            ILogger logger)
        {
            _localizationService = localizationService;
            _logger = logger;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        #endregion

        #region methods

        /// <summary>
        ///     Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation
        ///     The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync () =>
            Task.FromResult<IList<string>>(new List<string> {PublicWidgetZones.ProductDetailsEssentialBottom});

        /// <summary>
        ///     Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl () =>
            _webHelper.GetStoreLocation() + "Admin/AzPersonalizer/Configure";

        /// <summary>
        ///     Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName (string widgetZone) => "WidgetsAzPersonalizer";

        /// <summary>
        ///     Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async override Task InstallAsync ()
        {
            //settings
            AzPersonalizerSettings settings = new() {APIkey = "", Endpoint = ""};
            try
            {
                await _settingService.SaveSettingAsync(settings);

                await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string> {
                   ["Plugins.Widgets.AzPersonalizer.ApiKey"] = "Api key",
                   ["Plugins.Widgets.AzPersonalizer.Endpoint"] = "Endpoint"
                });
                await base.InstallAsync();
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync($"failed to install plugin. Cause is Exception:{e.Message}", e);
            }
        }

        /// <summary>
        ///     Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async override Task UninstallAsync ()
        {
            try
            {
                //settings
                await _settingService.DeleteSettingAsync<AzPersonalizerSettings>();

                //locales
                await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.AzPersonalizer");

                await base.UninstallAsync();
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync($"failed to install plugin. Cause is Exception:{e.Message}", e);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}
