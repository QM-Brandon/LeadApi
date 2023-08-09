using FluentValidation;
using System;
using System.Data.SqlTypes;

namespace QuintessaMarketing.API.Validator
{
    public class CaseLeadParameterValidator : AbstractValidator<CaseLeadParameter>
    {
        public CaseLeadParameterValidator()
        {

            RuleFor(s => s.PotentialClientPhoneNumber).NotEmpty().WithMessage("Phone Number").WithMessage("Invalid {PropertyName}");
            RuleFor(model => model).Custom((model, context) =>
            {
                if (string.IsNullOrWhiteSpace(model.IncidentDate) == true)
                {
                    model.IncidentDate = SqlDateTime.Null.ToString();
                }
                else
                {
                    DateTime incidentdate;
                    var sqlMinDate = SqlDateTime.MinValue;

                    if (DateTime.TryParse(model.IncidentDate, out incidentdate) == false || incidentdate < SqlDateTime.MinValue.Value)
                    {
                        context.AddFailure("IncidentDate", "Invalid IncidentDate.");
                    }
                }
            });
            //var statesDictionary = new Dictionary<string, string>
            //{
            //    { "AL", "Alabama" },
            //    { "AK", "Alaska" },
            //    { "AZ", "Arizona" },
            //    { "AR", "Arkansas" },
            //    { "CA", "California" },
            //    { "CO", "Colorado" },
            //    { "CT", "Connecticut" },
            //    { "DE", "Delaware" },
            //    { "DC", "District Of Columbia" },
            //    { "FL", "Florida" },
            //    { "GA", "Georgia" },
            //    { "HI", "Hawaii" },
            //    { "ID", "Idaho" },
            //    { "IL", "Illinois" },
            //    { "IN", "Indiana" },
            //    { "IA", "Iowa" },
            //    { "KS", "Kansas" },
            //    { "KY", "Kentucky" },
            //    { "LA", "Louisiana" },
            //    { "ME", "Maine" },
            //    { "MD", "Maryland" },
            //    { "MA", "Massachusetts" },
            //    { "MI", "Michigan" },
            //    { "MN", "Minnesota" },
            //    { "MS", "Mississippi" },
            //    { "MO", "Missouri" },
            //    { "MT", "Montana" },
            //    { "NE", "Nebraska" },
            //    { "NV", "Nevada" },
            //    { "NH", "New Hampshire" },
            //    { "NJ", "New Jersey" },
            //    { "NM", "New Mexico" },
            //    { "NY", "New York" },
            //    { "NC", "North Carolina" },
            //    { "ND", "North Dakota" },
            //    { "OH", "Ohio" },
            //    { "OK", "Oklahoma" },
            //    { "OR", "Oregon" },
            //    { "PA", "Pennsylvania" },
            //    { "RI", "Rhode Island" },
            //    { "SC", "South Carolina" },
            //    { "SD", "South Dakota" },
            //    { "TN", "Tennessee" },
            //    { "TX", "Texas" },
            //    { "UT", "Utah" },
            //    { "VT", "Vermont" },
            //    { "VA", "Virginia" },
            //    { "WA", "Washington" },
            //    { "WV", "West Virginia" },
            //    { "WI", "Wisconsin" },
            //    { "WY", "Wyoming" },
            //    { "PR", "Puerto Rico" },
            //    { "VI", "Virgin Islands" },
            //    { "GU", "Guam" }
            //};
            //RuleFor(s => s.API_Key).NotEmpty().WithName("API KEY").WithMessage("{PropertyName} is required");
            //RuleFor(s => s.PotentialClientEmail).EmailAddress().WithMessage("Email").WithMessage("Invalid email address");
            //RuleFor(s => s.PotentialClientState).Transform(value =>
            //    value.Length > 2 && string.IsNullOrWhiteSpace(statesDictionary.FirstOrDefault(s => s.Value == value).Key) ?
            //        value :
            //        statesDictionary.FirstOrDefault(s => s.Value == value).Key)
            //    .Custom((model, context) =>
            //    {
            //    if (!statesDictionary.Any(s => s.Key == model))
            //    {
            //        context.AddFailure("PotentialClientState", "Invalid State Name");
            //    }
            //});
        }
    }
}