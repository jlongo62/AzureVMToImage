using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VMSSManagementWeb.Startup))]
namespace VMSSManagementWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
