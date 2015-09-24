using System.Configuration;
using System.Globalization;

namespace AppProxyRedirector
{
    internal class Constants
    {
        public static string GraphUrl = ConfigurationManager.AppSettings["apr:GraphUrl"];
        public static string ClientId = ConfigurationManager.AppSettings["apr:ClientId"];
        public static string AppKey = ConfigurationManager.AppSettings["apr:AppKey"];
        public static string Authority = string.Format(CultureInfo.InvariantCulture,
            ConfigurationManager.AppSettings["apr:Authority"],
            ConfigurationManager.AppSettings["apr:TenantId"]);
        public static string TenantId = ConfigurationManager.AppSettings["apr:TenantId"];
        public static string DefaultRedirect = ConfigurationManager.AppSettings["apr:DefaultRedirect"];
        public static int AppCacheSeconds = int.Parse(ConfigurationManager.AppSettings["apr:AppCacheSeconds"]);
    }
}