using System;

namespace QuintessaMarketing.API
{
    public class CaseLeadData : CaseLeadParameter
    {
        public Guid CaseTypeId { get; set; }
        public Guid CampaignId { get; set; }
        public string PotentialClientFullName { get; set; }
        public Nullable<DateTime> ProcessedIncidentDate { get; set; }

    }
}