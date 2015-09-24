using Microsoft.ApplicationInsights;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace AppProxyRedirector
{
    public class AppLookupRedirectModule : IHttpModule
    {
        private readonly TelemetryClient _telemetry = new TelemetryClient();

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            var handler = new EventHandlerTaskAsyncHelper(OnBeginRequestAsync);
            context.AddOnBeginRequestAsync(handler.BeginEventHandler, handler.EndEventHandler);
        }

        private async Task OnBeginRequestAsync(object sender, EventArgs e)
        {
            var name = GetApplicationNameFromRequest();
            var applications = await GetAppProxyApplications();
            var application = applications.FirstOrDefault(a => IsMatchingApplication(a, name));
            var redirect = Constants.DefaultRedirect;
            if (application != null)
            {
                redirect = application.Homepage;
                _telemetry.TrackEvent("RedirectingToApplication", new Dictionary<string, string> {
                    { "DisplayName", application.DisplayName },
                    { "Homepage", application.Homepage }
                });
            }
            else
            {
                _telemetry.TrackEvent("RedirectingToDefault", new Dictionary<string, string> {
                    { "NameUsed", name }
                });
            }
            RedirectTo(redirect);
        }

        private void RedirectTo(string redirect)
        {
            HttpContext.Current.Response.Redirect(redirect, true);
            HttpContext.Current.Response.End();
        }

        private async Task<IReadOnlyList<IApplication>> GetAppProxyApplications()
        {
            var applications = (IReadOnlyList<IApplication>)HttpContext.Current.Cache.Get("AppProxyApplications");
            if (applications == null)
            {
                var aadclient = GetActiveDirectoryClient();
                var apps = await aadclient.Applications.ExecuteAsync();
                var allApps = new List<IApplication>();
                do
                {
                    allApps.AddRange(apps.CurrentPage.Where(IsAppProxyApplication));
                    if (apps.MorePagesAvailable)
                    {
                        apps = await apps.GetNextPageAsync();
                    }
                    else
                    {
                        apps = null;
                    }
                } while (apps != null);
                applications = allApps.AsReadOnly();
                HttpContext.Current.Cache.Add("AppProxyApplications",
                    applications,
                    null,
                    DateTime.UtcNow.AddSeconds(Constants.AppCacheSeconds),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Default,
                    null);
            }
            return applications;
        }

        private string GetApplicationNameFromRequest()
        {
            return HttpContext.Current.Request.Url.AbsolutePath.Substring(1);
        }

        private bool IsAppProxyApplication(IApplication application)
        {
            return application.Homepage.EndsWith(".msappproxy.net/");
        }

        private bool IsMatchingApplication(IApplication application, string name)
        {
            return string.Compare(application.DisplayName, name, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
        
        private ActiveDirectoryClient GetActiveDirectoryClient()
        {
            var baseServiceUri = new Uri(Constants.GraphUrl);
            return new ActiveDirectoryClient(new Uri(baseServiceUri, Constants.TenantId),
                    async () => await AcquireTokenAsync());
        }

        private async Task<string> AcquireTokenAsync()
        {
            var authContext = new AuthenticationContext(Constants.Authority, true);
            var result = await authContext.AcquireTokenAsync(
                Constants.GraphUrl,
                new ClientCredential(Constants.ClientId, Constants.AppKey));
            return result.AccessToken;
        }
    }
}
