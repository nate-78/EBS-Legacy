using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ACAfiling_Web.Startup))]
namespace ACAfiling_Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
