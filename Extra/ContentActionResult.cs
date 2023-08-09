using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace QuintessaMarketing.API
{
    public class ContentActionResult<T> : IHttpActionResult where T : class
    {

        //public to make these available in the generated client
        public readonly HttpStatusCode status;
        public readonly string message;
        public readonly T data;
        private readonly HttpRequestMessage request;

        public ContentActionResult(HttpStatusCode status, string message, T data, HttpRequestMessage request)
        {
            this.status = status;
            this.message = message;
            this.data = data;
            this.request = request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = this.request.CreateResponse(status);
            var formatter = request.GetConfiguration().Formatters.JsonFormatter;
            formatter.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
            formatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;

            response.Content = new ObjectContent<object>(new
            {
                //Case of attributes in response should be same as the properties above (lowercase) otherwise
                //it does not deserialize the response in the client generated from swagger
                status = this.status,
                message = this.message,
                data = this.data,
                requestId = System.Diagnostics.Trace.CorrelationManager.ActivityId
            }, formatter);

            return Task.FromResult(response);
        }
    }

}