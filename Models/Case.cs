using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Q.TESTAPI2.Models
{
    public class Case
    {
        public Guid? CaseId { get; set; }
        public DateTime? IntakeDateTime { get; set; }
        public int? UserId { get; set; }
        public DateTime? IncidentDate { get; set; }
        public Guid? CaseTypeId { get; set; }
        public string CaseStatus { get; set; }
        public Guid? AttorneyId { get; set; }
        public string CaseDescription { get; set; }
        public int? PassengerCount { get; set; }
        public string PcName { get; set; }
        public string PcPhone { get; set; }
        public string PcEmail { get; set; }
        public string PcContactName { get; set; }
        public string PcContactPhone { get; set; }
        public string PcContactName2 { get; set; }
        public string PcContactPhone2 { get; set; }
        public string PcBirthDate { get; set; }
        public string PcSsn { get; set; }
        public string PcAddress { get; set; }
        public string PcCity { get; set; }
        public string PcState { get; set; }
        public string PcZipCode { get; set; }
        public string PcInsuranceCompany { get; set; }
        public string PcClaimNo { get; set; }
        public string IncidentLocation { get; set; }
        public string PcAtFault { get; set; }
        public string PcInjured { get; set; }
        public string PcPursuingTreatment { get; set; }
        public string PcWillingForTreatment { get; set; }
        public string PcEmployer { get; set; }
        public string InvestigatingPd { get; set; }
        public string InjuriesAndTreatment { get; set; }
        public string DefendantName { get; set; }
        public string DefendantClaimNo { get; set; }
        public string PropertyDamageDescription { get; set; }
        public decimal? PropertyDamageAmount { get; set; }
        public string PriorAttorneyName { get; set; }
        public string PriorSettlement { get; set; }
        public int? AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? CampaignId { get; set; }
        public string EntryStatus { get; set; }
        public string PcFirstName { get; set; }
        public string PcMiddleName { get; set; }
        public string PcLastName { get; set; }
        public string AdverseInsurance { get; set; }
        public string PoliceReportNo { get; set; }
        public string PcVehicleInfo { get; set; }
        public string PcVehicleLocation { get; set; }
        public string AttorneyStatus { get; set; }
        public string PcHealthInsurance { get; set; }
        public DateTime? CalcDate { get; set; }
        public string PersonTicketed { get; set; }
        public string PersonTicketedComments { get; set; }
        public decimal? AttorneyFee { get; set; }
        public string PcEmployerAddress { get; set; }
        public string PcEmployerCity { get; set; }
        public string PcEmployerState { get; set; }
        public string PcEmployerZipCode { get; set; }
        public string DisengageReason { get; set; }
        public string DisengageNotes { get; set; }
        public DateTime? DisengageDateTime { get; set; }
        public string DefendantVehicleInfo { get; set; }
        public DateTime? EmailDateTime { get; set; }
        public decimal? OverrideAttorneyFee { get; set; }
        public Guid? AffiliateId { get; set; }
        public DateTime? PendingDisengageDateTime { get; set; }
        public string AccidentState { get; set; }
        public DateTime? OverrideIntakeDateTime { get; set; }
        public bool? ResolveBillingNote { get; set; }
        public string DriverLicense { get; set; }
        public string PropertyDamagePics { get; set; }
        public bool? PremiumCase { get; set; }
        public string AttCaseId { get; set; }
        public bool? Disputed { get; set; }
        public string LiveTransfer { get; set; }
        public string Gclid { get; set; }
        public bool? Converted { get; set; }
        public string NotaCaseReason { get; set; }
        public string RecapTime { get; set; }
        public string MatchTime { get; set; }
        public DateTime? RetainedTimeStamp { get; set; }
        public bool? RejectLead { get; set; }
        public string RejectLeadReason { get; set; }
        public DateTime? RejectLeadDate { get; set; }
        public int? RejectLeadBy { get; set; }
        public string TransferName { get; set; }
        public string TransferTime { get; set; }
        public string LeadID { get; set; }

        public string API_Key { get; set; }
    }

}