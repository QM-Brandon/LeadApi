using System.Reflection;
using System.Web.Http;
using Elmah.Contrib.WebApi;
using LightInject;
using Owin;

namespace QuintessaMarketing.API
{
    public partial class Startup
    {
        public void ConfigureContainer(HttpConfiguration config, IAppBuilder app)
        {
            var container = new ServiceContainer();
            container.RegisterApiControllers();
            container.EnableWebApi(config);
            var assemblies = new AssemblyLoader().Load("Quintessa*.dll");

            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.Contains("API"))
                {
                    RegisterValidators(container, assembly);
                }
            }

            container.Register<ValidationActionFilterAttribute>();
            config.Filters.Add(container.GetInstance<ValidationActionFilterAttribute>());

            container.Register<ElmahHandleErrorApiAttribute>();
            config.Filters.Add(container.GetInstance<ElmahHandleErrorApiAttribute>());
        }

        private void RegisterValidators(ServiceContainer container, Assembly assembly)
        {
            var validators = FluentValidation.AssemblyScanner.FindValidatorsInAssembly(assembly);
            validators.ForEach(validator => {
                container.Register(validator.InterfaceType, validator.ValidatorType);
            });
        }
    }
}