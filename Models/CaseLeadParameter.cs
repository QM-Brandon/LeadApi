using FluentValidation.Attributes;
using QuintessaMarketing.API.Validator;
using System;

namespace QuintessaMarketing.API
{
    [Validator(typeof(CaseLeadParameterValidator))]
    public class CaseLeadParameter
    {
        public string API_Key { get; set; }
        public string Source { get; set; }
        public string PotentialClientFirstName { get; set; }
        public string PotentialClientMiddleName { get; set; }
        public string PotentialClientLastName { get; set; }
        public string PotentialClientPhoneNumber { get; set; }
        public string PotentialClientEmail { get; set; }
        public string CaseDescription { get; set; }
        public string IncidentDate { get; set; }
        public string CaseType { get; set; }
        public string AccidentState { get; set; }
        public string PotentialClientStreetAddress { get; set; }
        public string PotentialClientCity { get; set; }
        public string PotentialClientState { get; set; }
        public string PotentialClientZip { get; set; }
        public string LeadId { get; set; }

        public override string ToString()
        {
            return $"API_Key: {API_Key}\n" +
                   $"Source: {Source}\n" +
                   $"PotentialClientFirstName: {PotentialClientFirstName}\n" +
                   $"PotentialClientMiddleName: {PotentialClientMiddleName}\n" +
                   $"PotentialClientLastName: {PotentialClientLastName}\n" +
                   $"PotentialClientPhoneNumber: {PotentialClientPhoneNumber}\n" +
                   $"PotentialClientEmail: {PotentialClientEmail}\n" +
                   $"CaseDescription: {CaseDescription}\n" +
                   $"IncidentDate: {IncidentDate}\n" +
                   $"CaseType: {CaseType}\n" +
                   $"AccidentState: {AccidentState}\n" +
                   $"PotentialClientStreetAddress: {PotentialClientStreetAddress}\n" +
                   $"PotentialClientCity: {PotentialClientCity}\n" +
                   $"PotentialClientState: {PotentialClientState}\n" +
                   $"PotentialClientZip: {PotentialClientZip}\n" +
                   $"LeadId: {LeadId}";
        }
    }


}
