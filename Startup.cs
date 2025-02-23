using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DaPlatform.Startup))]
namespace DaPlatform
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
