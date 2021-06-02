using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GWCU_USSD_WebApp.Startup))]
namespace GWCU_USSD_WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
