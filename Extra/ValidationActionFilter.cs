using QuintessaMarketing.API;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace QuintessaMarketing.API
{
    [AttributeUsage(AttributeTargets.All)]
    public class ValidationActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var modelState = actionContext.ModelState;

            if (!modelState.IsValid)
            {
                var error = modelState.Keys.SelectMany(key => modelState[key].Errors.Select(ex => new ValidationError(key, ex.ErrorMessage)));
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = ContentHelper.GetContent(actionContext.Request, HttpStatusCode.BadRequest, "Validation Error", error),
                };
            }
        }
    }
}