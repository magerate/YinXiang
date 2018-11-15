using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(YinXiang.Startup))]
namespace YinXiang
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
