using Q.TESTAPI2.Models;
using QuintessaMarketing.API;
using System;
using System.Data;
using System.Threading.Tasks;

public interface ICaseLeadRepository
{
    Task<string> AddCaseLead(CaseLeadData data, Guid affiliateId);
    DataTable GetAffiliateWithApiKey(Guid apiKey);
    Task LogAPIInformationAsync(Guid logId, DateTime dateTime, string information);
    CaseLead GetLead(string caseid, Guid affiliateid);

    Tuple<int, string> UpdateCase(dynamic caseUpdate);
}
