using System.Net.Http;

namespace QuintessaMarketing.API
{
    public class ContentHelper
    {
        public static ObjectContent<object> GetContent(HttpRequestMessage request, System.Net.HttpStatusCode code, string message, object data)
        {
            return new ObjectContent<object>(new
            {
                status = code,
                message = message,
                data = data,
                requestId = System.Diagnostics.Trace.CorrelationManager.ActivityId
            }, request.GetConfiguration().Formatters.JsonFormatter);
        }

    }
}