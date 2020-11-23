using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OSCAL_SSP_Mapper.Startup))]
namespace OSCAL_SSP_Mapper
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
