using Q.TESTAPI2.Models;
using QuintessaMarketing.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public interface ICaseLeadRepository
{
    Task<string> AddCaseLead(CaseLeadData data, Guid affiliateId, string api);
    Task<string> AddCaseLead(CaseLeadData data, Guid affiliateId, string api, Dictionary<string,string> extraData);
    DataTable GetAffiliateWithApiKey(Guid apiKey);
    Task LogAPIInformationAsync(Guid logId, DateTime dateTime, string information);
    CaseLead GetLead(string api,string caseid);

    Tuple<int, string> UpdateCase(dynamic caseUpdate);
}
