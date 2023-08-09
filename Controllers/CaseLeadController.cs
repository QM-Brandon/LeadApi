using Newtonsoft.Json;
using Q.TESTAPI2.Models;
using QuintessaMarketing.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace QuintessaMarketing.API.Controllers
{
    /// <summary>
    /// Quintessa Case Leads
    /// </summary>
    [RoutePrefix("api/lead")]
    public class CaseLeadController : ApiController
    {
        public ICaseLeadService CaseLeadService { get; set; }

        public CaseLeadController()
        {
            CaseLeadService = new CaseLeadService();
        }

        /// <summary>
        /// Post Lead to Quintessa Marketing
        /// </summary>
        /// <remarks>Send Leads to Quintessa Marketing</remarks>
        /// <param name="caseLead">Lead Information</param>
        /// <returns>API Call Status</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [Route("submitlead")]
        [ResponseType(typeof(ContentActionResult<string>))]
        [HttpPost]
        public async Task<IHttpActionResult> Post(CaseLeadParameter caseLead)
        {

                var data = CaseLeadService.GetAPIKeyData(caseLead);

                if (data.Item1)
                {
                    await CaseLeadService.ProcessCaseLead(caseLead, data.Item2);
                    return new ContentActionResult<string>(HttpStatusCode.OK, "OK", null, Request);
                }
            

            return new ContentActionResult<string>(HttpStatusCode.Unauthorized, "Invalid api key", caseLead.API_Key, Request);
        }

        [Route("health")]
        [ResponseType(typeof(ContentActionResult<string>))]
        [HttpGet]
        public async Task<IHttpActionResult> CheckHealth()
        {
            return new ContentActionResult<string>(HttpStatusCode.OK, "OK", "OK", Request);
        }


        [Route("GetLead")]
        [ResponseType(typeof(ContentActionResult<string>))]
        [HttpGet]
        public async Task<IHttpActionResult> GetLead(string api, string caseid)
        {
          var data = CaseLeadService.GetAPIKeyData(api);

            if (true)
            {
                CaseLead lead = CaseLeadService.GetCaseLead(caseid, data.Item2);

                return new ContentActionResult<string>(HttpStatusCode.OK, "OK", JsonConvert.SerializeObject(lead), Request); //replace casedata

            }

            return new ContentActionResult<string>(HttpStatusCode.Unauthorized, "Invalid api key", api, Request);
        }

        [Route("UpdateLead")]
        [ResponseType(typeof(ContentActionResult<string>))]
        [HttpPatch]
        public async Task<IHttpActionResult> UpdateLead([FromBody] dynamic jsonData)
        {
            dynamic dataObject = JsonConvert.DeserializeObject<dynamic>(jsonData.ToString());
            string test = dataObject.API_Key;
           // var data = CaseLeadService.GetAPIKeyData(dataObject.API_Key);

            if(dataObject.CaseId==null&& dataObject.LeadID == "")
            {
                return new ContentActionResult<string>(HttpStatusCode.BadRequest, "CaseID and LeadID are empty","", Request);
            }

            if (true)
            {
                Tuple<int, string> returnCode = CaseLeadService.UpdateCase(dataObject);
                if (returnCode.Item1 ==-1)
                {
                    return new ContentActionResult<string>(HttpStatusCode.BadRequest, returnCode.Item2, "", Request);
                }else if (returnCode.Item1 == 0)
                {
                    return new ContentActionResult<string>(HttpStatusCode.Accepted, "Updated but log entry was not created", "", Request);
                }else if (returnCode.Item1 == 1)
                {
                    return new ContentActionResult<string>(HttpStatusCode.OK, "Case successfully Updated", "", Request);
                }

            }

            return new ContentActionResult<string>(HttpStatusCode.Unauthorized, "Invalid api key", dataObject.API_Key, Request);
        }
    }
}
