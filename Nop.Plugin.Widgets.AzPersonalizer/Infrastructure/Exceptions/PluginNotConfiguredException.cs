namespace Nop.Plugin.Widgets.AzPersonalizer.Infrastructure.Exceptions
{

    using System;

    public class PluginNotConfiguredException : Exception
    {

        public PluginNotConfiguredException (string message) : base(message)
        {
        }

        public PluginNotConfiguredException (string message, Exception innerException) : base(message, innerException)
        {
        }

        public PluginNotConfiguredException ()
        {
        }
    }
}
