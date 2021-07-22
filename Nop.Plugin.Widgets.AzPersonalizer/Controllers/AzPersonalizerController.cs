namespace Nop.Plugin.Widgets.AzPersonalizer.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Nop.Core;
    using Nop.Plugin.Widgets.AzPersonalizer.Models;
    using Nop.Services.Configuration;
    using Nop.Services.Localization;
    using Nop.Services.Media;
    using Nop.Services.Messages;
    using Nop.Services.Security;
    using Nop.Web.Framework;
    using Nop.Web.Framework.Controllers;
    using Nop.Web.Framework.Mvc.Filters;

   [AuthorizeAdmin]
   [Area(AreaNames.Admin)]
   [AutoValidateAntiforgeryToken]
    public class AzPersonalizerController : BasePluginController
    {

        #region fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region ctor

        public AzPersonalizerController (ILocalizationService localizationService ,
            INotificationService notificationService ,
            IPermissionService permissionService ,
            ISettingService settingService ,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region methods

        public async Task<IActionResult> Configure ()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
            {
                return AccessDeniedView();
            }

            //load settings for a chosen store scope
            int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            AzPersonalizerSettings azPersonalizerSettings =
                await _settingService.LoadSettingAsync<AzPersonalizerSettings>(storeScope);
            ConfigurationModel model = new()
            {
                APIkey = azPersonalizerSettings.APIkey,
                Endpoint = azPersonalizerSettings.Endpoint,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.APIKey_OverrideForStore =
                    await _settingService.SettingExistsAsync(azPersonalizerSettings, x => x.Endpoint,
                        storeScope);
                model.Endpoint_OverrideForStore =
                    await _settingService.SettingExistsAsync(azPersonalizerSettings, x => x.APIkey, storeScope);
            }

            return View("~/Plugins/Widgets.AzPersonalizer/Views/Configure.cshtml", model);
        }

       [HttpPost]
        public async Task<IActionResult> Configure (ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
            {
                return AccessDeniedView();
            }

            //load settings for a chosen store scope
            int storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            AzPersonalizerSettings azPersonalizerSettings =
                await _settingService.LoadSettingAsync<AzPersonalizerSettings>(storeScope);


            azPersonalizerSettings.Endpoint = model?.Endpoint;
            azPersonalizerSettings.APIkey = model?.APIkey;


            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(azPersonalizerSettings, x => x.Endpoint,
                model.Endpoint_OverrideForStore, storeScope);
            await _settingService.SaveSettingOverridablePerStoreAsync(azPersonalizerSettings, x => x.APIkey,
                model.APIKey_OverrideForStore, storeScope);


            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //get current picture identifiers

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion
    }
}
