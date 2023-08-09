using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Q.TESTAPI2.Models
{
    public class CaseLead
    {
            public DateTime IncidentDate { get; set; }
            public string CaseTypeId { get; set; }
            public string CaseType { get; set; } //change this a dbcall
            public string CaseStatus { get; set; }
            public string AttorneyId { get; set; }
            public string CaseDescription { get; set; }
            public int? PassengerCount { get; set; }
            public string PcName { get; set; }
            public string PcPhone { get; set; }
            public string PcEmail { get; set; }
            public string PcContactName { get; set; }
            public string PcContactPhone { get; set; }
            public string PcBirthdate { get; set; }
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
            public double PropertyDamageAmount { get; set; }
            public string PriorAttorneyName { get; set; }
            public string PriorSettlement { get; set; }
            public string EntryStatus { get; set; }
            public string PcFirstName { get; set; }
            public string PcMiddleName { get; set; }
            public string PcLastName { get; set; }
            public string AdverseInsurance { get; set; }
            public string PoliceReportNo { get; set; }
            public string PcVehicleInfo { get; set; }
            public string PcVehicleLocation { get; set; }
            public string PcHealthInsurance { get; set; }
            public string PersonTicketed { get; set; }
            public string PersonTicketedComments { get; set; }
            public string PcEmployerAddress { get; set; }
            public string PcEmployerCity { get; set; }
            public string PcEmployerState { get; set; }
            public string PcEmployerZipCode { get; set; }
            public string DefendantVehicleInfo { get; set; }
            public string AffiliateId { get; set; }
            public string AccidentState { get; set; }
            public string DriverLicense { get; set; }

       

    }
}