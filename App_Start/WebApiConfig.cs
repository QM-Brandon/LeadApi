using FluentValidation.WebApi;
using Swashbuckle.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace QuintessaMarketing.API
{
    public partial class Startup
    {
        public void ConfigureWebAPI(HttpConfiguration config)
        {
             // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            FluentValidationModelValidatorProvider.Configure(config);
            //config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());

            // Set Swagger as default start page
            config.Routes.MapHttpRoute(
                name: "swagger_root",
                routeTemplate: "",
                defaults: null,
                constraints: null,
                handler: new RedirectHandler((message => message.RequestUri.ToString()), "swagger")
            );
        }
    }
}
