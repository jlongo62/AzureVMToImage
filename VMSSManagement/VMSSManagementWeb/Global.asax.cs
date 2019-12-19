using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;

namespace VMSSManagementWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTgyMzA4QDMxMzcyZTMzMmUzMG5xZ0MwMzJ2RXVmamJWMGd2b0t2d3htbUNsamQ2bGxzSGcrTHJXVWVwdVk9;MTgyMzA5QDMxMzcyZTMzMmUzMFpic3dPV3dLMmtzaEYvbFYyVXF5VzNXTE1SaUFqU1Mvc002WXlhOFZ5MVk9;MTgyMzEwQDMxMzcyZTMzMmUzMGxDTlJCL1JYd0lMYlZkckdEUWs1MHVVWXdkSmUrVnJGSGRSZHhIV2lwK009;MTgyMzExQDMxMzcyZTMzMmUzMG5mbFRqS2ttY0xKS3RjK25kaVJZY1krdUE1OHdhMnVWNjN1THF4eThsK3M9;MTgyMzEyQDMxMzcyZTMzMmUzMER1WnZMODN6YW9nTURSY1FzWXJQR0FJSTdSUjlzMXJVV3JRbjM3QXBlK0k9;MTgyMzEzQDMxMzcyZTMzMmUzMGlxREF5ZS9qTDJPOFJwRVY0SDJzU0ZYSTZ1SEszeENQUEZLRVNla2l1dVE9;MTgyMzE0QDMxMzcyZTMzMmUzMGFwNW5LcFVKeEVVSE5HNithZzZhQU1DQmpmYUxwVmo2VXpLREJOc2lXUkU9;MTgyMzE1QDMxMzcyZTMzMmUzMGlseVM5QkhlWmtrRHhIYVlOSW1rbkphTjBwVHFHam82bmQ0b0ZpSWRyWjg9;MTgyMzE2QDMxMzcyZTMzMmUzMGMxNHovWVV2VDYrMWZBYU9ZVG1PbWJXZ2I3RjBwanQvaFM0ZHZoalhhQ0E9;NT8mJyc2IWhiZH1gfWN9YmdoYmF8YGJ8ampqanNiYmlmamlmanMDHmg5PCA2Izt9Pzw9NDwTPzolNn0wPD4=");
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "api/{controller}/{action}/{id}",
                 defaults: new { id = RouteParameter.Optional }
             );
        }
    }
}
