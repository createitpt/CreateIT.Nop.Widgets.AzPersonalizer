namespace Nop.Plugin.Widgets.AzPersonalizer
{
    using Nop.Core.Configuration;

    /// <summary>
    ///     Represents plugin settings
    /// </summary>
    public class AzPersonalizerSettings : ISettings
    {
        /// <summary>
        ///     Gets or sets the azure personalizer resource Api key
        /// </summary>
        public string APIkey { get; set; }

        /// <summary>
        ///     Gets or sets the azure personalizer resource endpoint
        /// </summary>
        public string Endpoint { get; set; }
    }
}
