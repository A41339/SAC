using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(FGA.Startup))]
namespace FGA
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}