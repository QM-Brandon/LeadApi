using Newtonsoft.Json;
using Q.TESTAPI2.Models;
using QuintessaMarketing.API;
using Support.Email;
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
                    await CaseLeadService.ProcessCaseLead(caseLead, data.Item2,data.Item2.ToString());

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

            if (data.Item1)
            {
                CaseLead lead = CaseLeadService.GetCaseLead(api,caseid);

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
           // var data = CaseLeadService.GetAPIKeyData(dataObject.API_Key);

            if(dataObject.CaseId==null&& dataObject.LeadID == "")
            {
                return new ContentActionResult<string>(HttpStatusCode.BadRequest, "CaseID and LeadID are empty","", Request);
            }

            var data = CaseLeadService.GetAPIKeyData(dataObject.API_Key);

            if (data.Item1)
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


        [Route("SubmitLeadCID")]
        [ResponseType(typeof(ContentActionResult<string>))]
        [HttpPost]
        public async Task<IHttpActionResult> SubmitLeadCID([FromBody] dynamic jsonData)
        {
            dynamic dataObject = JsonConvert.DeserializeObject<dynamic>(jsonData.ToString());
            // var data = CaseLeadService.GetAPIKeyData(dataObject.API_Key);
            if (dataObject.API_Key == null)
            {
                return new ContentActionResult<string>(HttpStatusCode.Unauthorized, "Invalid api key", "", Request);
            }
            var data = CaseLeadService.GetAPIKeyData(dataObject.API_Key.ToString());
            
            Dictionary<string, string> incomingParams =new Dictionary<string, string>();
            foreach (var parameter in Request.GetQueryNameValuePairs())
            {
                incomingParams.Add( parameter.Key,parameter.Value);
            }
            //if(true)
            if (data.Item1)
            {
                //await CaseLeadService.ProcessCaseLead(caseLead, data.Item2, data.Item2.ToString());
                CaseLeadData temp = new CaseLeadData();
                Dictionary<string, string> extraData = new Dictionary<string, string>();
                try
                {
                    temp.PotentialClientFirstName = dataObject.PotentialClientLastName;
                    temp.PotentialClientLastName = dataObject.PotentialClientLastName;
                    temp.PotentialClientPhoneNumber = dataObject.PotentialClientPhoneNumber;
                    temp.PotentialClientEmail = dataObject.PotentialClientEmail;
                    temp.CaseDescription = dataObject.CaseDescription;
                    temp.IncidentDate = dataObject.IncidentDate;
                    temp.CaseType = dataObject.CaseType;
                    temp.AccidentState = dataObject.AccidentState;
                    temp.PotentialClientZip = dataObject.PotentialClientZip;
                    temp.Source = dataObject.Source;
                    extraData.Add("atFault",dataObject.atFault.ToString());
                    extraData.Add("work_vehicle", dataObject.work_vehicle.ToString());
                    extraData.Add("injured", dataObject.injured.ToString());
                    extraData.Add("medical", dataObject.medical.ToString());
                    extraData.Add("retainedAttorney", dataObject.retainedAttorney.ToString());
                    extraData.Add("c", dataObject.c.ToString());
                    extraData.Add("s1", dataObject.s1.ToString());
                    extraData.Add("s2", dataObject.s2.ToString());
                    extraData.Add("s3", dataObject.s3.ToString());

                    if (temp != null)
                    {
                        await CaseLeadService.ProcessCaseLead(temp, data.Item2, data.Item2.ToString(), extraData);
                    }
                    else
                    {

                    }
                }
                catch (Exception e)
                {

                }
                return new ContentActionResult<string>(HttpStatusCode.OK, "OK", null, Request);
            }

            return new ContentActionResult<string>(HttpStatusCode.Unauthorized, "Invalid api key", dataObject.API_Key, Request);
        }
    }
}
