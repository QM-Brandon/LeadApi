using Q.TESTAPI2.Models;
using QuintessaMarketing.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICaseLeadService
{
    Task ProcessCaseLead(CaseLeadParameter caseLead, Guid affiliateId, string api);
    Task ProcessCaseLead(CaseLeadData caseLead, Guid affiliateId, string api, Dictionary<string,string> extraData);
    Tuple<bool, Guid> GetAPIKeyData(CaseLeadParameter data);
    Tuple<bool, Guid> GetAPIKeyData(string api);

    CaseLead GetCaseLead(string api,string caseid);

    Tuple<int, string> UpdateCase(dynamic caseUpdate);
}
