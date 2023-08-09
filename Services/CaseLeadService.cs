using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuintessaMarketing.API.Common;
using QuintessaMarketing.API;
using System.Web.Configuration;
using System.Data.SqlTypes;
using Q.TESTAPI2.Models;
using Support.Email;

public class CaseLeadService : ICaseLeadService
{
    private readonly ICaseLeadRepository _repository;
    private readonly string AFFILIATE_ID_COLUMN = "affiliateid";
    private readonly string CASE_ID_COLUMN = "caseid";

    public CaseLeadService()
    {
        _repository = new CaseLeadRepository();
    }

    public async Task ProcessCaseLead(CaseLeadParameter caseLead, Guid affiliateId)
    {
        var data = GetCaseDate(caseLead);
        SetPotentialClientFullName(data);
        SetStateName(data);
        var caseId = await _repository.AddCaseLead(data, affiliateId);
        await LogApiCall(caseLead, affiliateId);
        SendCaseLeadNotification(data, caseId);
    }

    public Tuple<bool, Guid> GetAPIKeyData(CaseLeadParameter data)
    {
        var isValidKey = false;
        var affiliateId = Guid.Empty;

        if (Guid.TryParse(data.API_Key, out Guid apiKey))
        {
            var dataTable = _repository.GetAffiliateWithApiKey(apiKey);
            isValidKey = dataTable.Rows.Count > 0;

            if (dataTable.Rows.Count > 0)
            {
                Guid.TryParse(dataTable.Rows[0][AFFILIATE_ID_COLUMN].ToString(), out affiliateId);
            }
        }

        return new Tuple<bool, Guid>(isValidKey, affiliateId);
    }

    public Tuple<bool, Guid> GetAPIKeyData(string apiKeyString)
    {
        var isValidKey = false;
        var affiliateId = Guid.Empty;

        if (Guid.TryParse(apiKeyString, out Guid apiKey))
        {
            var dataTable = _repository.GetAffiliateWithApiKey(apiKey);
            isValidKey = dataTable.Rows.Count > 0;

            if (dataTable.Rows.Count > 0)
            {
                Guid.TryParse(dataTable.Rows[0][AFFILIATE_ID_COLUMN].ToString(), out affiliateId);
            }
        }

        return new Tuple<bool, Guid>(isValidKey, affiliateId);
    }

    private CaseLeadData GetCaseDate(CaseLeadParameter data)
    {
        DateTime processIncidentDate;

        return new CaseLeadData()
        {
            API_Key = data.API_Key,
            CaseDescription = data.CaseDescription,
            CaseType = data.CaseType,
            IncidentDate = data.IncidentDate,
            PotentialClientCity = data.PotentialClientCity,
            PotentialClientEmail = data.PotentialClientEmail,
            PotentialClientFirstName = data.PotentialClientFirstName,
            PotentialClientLastName = data.PotentialClientLastName,
            PotentialClientMiddleName = data.PotentialClientMiddleName,
            PotentialClientPhoneNumber = data.PotentialClientPhoneNumber,
            PotentialClientState = data.PotentialClientState,
            AccidentState = data.AccidentState,
            PotentialClientStreetAddress = data.PotentialClientStreetAddress,
            PotentialClientZip = data.PotentialClientZip,
            Source = data.Source,
            LeadId = data.LeadId,
            ProcessedIncidentDate = DateTime.TryParse(data.IncidentDate, out processIncidentDate) ? processIncidentDate : (DateTime?) SqlDateTime.Null
        };
    }

    private void SetPotentialClientFullName(CaseLeadData data)
    {
        var potentialClientFullName = string.IsNullOrWhiteSpace(data.PotentialClientFirstName) ? string.Empty : data.PotentialClientFirstName.Trim();

        if (!string.IsNullOrWhiteSpace(data.PotentialClientMiddleName))
        {
            potentialClientFullName = string.IsNullOrWhiteSpace(potentialClientFullName) ?
                data.PotentialClientMiddleName :
                string.Concat(potentialClientFullName, " ", data.PotentialClientMiddleName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(data.PotentialClientLastName))
        {
            potentialClientFullName = string.IsNullOrWhiteSpace(potentialClientFullName) ?
                data.PotentialClientLastName :
                string.Concat(potentialClientFullName, " ", data.PotentialClientLastName.Trim());
        }

        data.PotentialClientFullName = potentialClientFullName;
    }

    private void SetStateName(CaseLeadData data)
    {
        if (data.PotentialClientState == null)
        {
            return;
        }
        if (data.PotentialClientState.Length > 2)
        {
            data.PotentialClientState = Common.States.FirstOrDefault(s => s.Value == data.PotentialClientState).Key;
        }

        if (data.AccidentState.Length > 2)
        {
            data.AccidentState = Common.States.FirstOrDefault(s => s.Value == data.AccidentState).Key;
        }
    }

    private async Task LogApiCall(CaseLeadParameter data, Guid affiliateId)
    {
        var today = DateTime.Now;
        var logId = Guid.NewGuid();
        var message = PrepareMessageForLog(data, affiliateId, today);
        await _repository.LogAPIInformationAsync(logId, today, message);
    }

    private string PrepareMessageForLog(CaseLeadParameter data, Guid affiliateId, DateTime dateTime)
    {
        var json = JsonConvert.SerializeObject(data);
        return string.Format("API call requested by affiliate with id '{0}' at '{1}' and data '{2}'.", affiliateId, dateTime, json);
    }
    private string PrepareMessageForLog(string caseid, Guid affiliateId, DateTime dateTime)
    {
        return string.Format("API GetLead call requested by affiliate with id '{0}' at '{1}' and caseid '{2}'.", affiliateId, dateTime, caseid);
    }

    private void SendCaseLeadNotification(CaseLeadData data, string caseId)
    {
        var url = Utilities.GetWebsiteURL();
        var notificationURL = url.Contains("leadsapi.accidentintakeforms.com") ?
            WebConfigurationManager.AppSettings["prodwebprojecturl"] :
            WebConfigurationManager.AppSettings["webprojecturl"];
        var emailAddress = WebConfigurationManager.AppSettings["prodaffiliateleadnotificationemail"];
        var subject = string.Format("A new lead has been added for PC: {0}", data.PotentialClientFullName);
        var message = string.Format(
            @"A new lead has been added for PC: {0}<br>\n
                <p>Case Description: {1}</p>\n
                <p>Case Type: {2}</p>\n
                <p>Incident Date: {3}</p>\n
                <p><a href='{4}caseedit.aspx?action=edit&id={5}'>Click here for the case link</a></p>"
            , data.PotentialClientFullName, data.CaseDescription, data.CaseType, data.IncidentDate, notificationURL, caseId);
        var emailSender = new Email();
        emailSender.SendEmailMessage(emailAddress, subject, message, string.Empty, string.Empty, string.Empty);
    }

    public CaseLead GetCaseLead(string caseid, Guid affiliateid)
    {
        var today = DateTime.Now;
        var logId = Guid.NewGuid();
        var message = PrepareMessageForLog(caseid, affiliateid, today);

        return _repository.GetLead(caseid, affiliateid);
    }

    public Tuple<int, string> UpdateCase(dynamic caseUpdate)
    {
        return _repository.UpdateCase(caseUpdate);
    }
}
