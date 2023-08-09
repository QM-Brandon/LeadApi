using System.Linq;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Swashbuckle.Application;

[assembly: OwinStartup(typeof(QuintessaMarketing.API.Startup))]

namespace QuintessaMarketing.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);

            var config = new HttpConfiguration();

            config.EnableSwagger(x =>
            {
                x.SingleApiVersion("v1", "Quintessa Marketing");
                x.DescribeAllEnumsAsStrings();
                x.ResolveConflictingActions(desc => desc.First());
                x.IncludeXmlComments(string.Format(@"{0}\bin\QuintessaMarketing.API.xml", System.AppDomain.CurrentDomain.BaseDirectory));
            })
            .EnableSwaggerUi();

            ConfigureContainer(config, app);

            ConfigureWebAPI(config);

            app.UseWebApi(config);
        }
    }
}
