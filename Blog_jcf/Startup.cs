using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Blog_jcf.Startup))]
namespace Blog_jcf
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
