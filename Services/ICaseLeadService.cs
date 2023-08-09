using Q.TESTAPI2.Models;
using QuintessaMarketing.API;
using System;
using System.Threading.Tasks;

public interface ICaseLeadService
{
    Task ProcessCaseLead(CaseLeadParameter caseLead, Guid affiliateId);
    Tuple<bool, Guid> GetAPIKeyData(CaseLeadParameter data);
    Tuple<bool, Guid> GetAPIKeyData(string api);

    CaseLead GetCaseLead(string caseid, Guid affiliateid);

    Tuple<int, string> UpdateCase(dynamic caseUpdate);
}
