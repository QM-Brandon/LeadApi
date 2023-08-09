using System.Collections.Generic;
using System.Web;

namespace QuintessaMarketing.API.Common
{
    public static class Common
    {
        private static readonly Dictionary<string, string> statesDictionary = new Dictionary<string, string>
            {
                { "AL", "Alabama" },
                { "AK", "Alaska" },
                { "AZ", "Arizona" },
                { "AR", "Arkansas" },
                { "CA", "California" },
                { "CO", "Colorado" },
                { "CT", "Connecticut" },
                { "DE", "Delaware" },
                { "DC", "District Of Columbia" },
                { "FL", "Florida" },
                { "GA", "Georgia" },
                { "HI", "Hawaii" },
                { "ID", "Idaho" },
                { "IL", "Illinois" },
                { "IN", "Indiana" },
                { "IA", "Iowa" },
                { "KS", "Kansas" },
                { "KY", "Kentucky" },
                { "LA", "Louisiana" },
                { "ME", "Maine" },
                { "MD", "Maryland" },
                { "MA", "Massachusetts" },
                { "MI", "Michigan" },
                { "MN", "Minnesota" },
                { "MS", "Mississippi" },
                { "MO", "Missouri" },
                { "MT", "Montana" },
                { "NE", "Nebraska" },
                { "NV", "Nevada" },
                { "NH", "New Hampshire" },
                { "NJ", "New Jersey" },
                { "NM", "New Mexico" },
                { "NY", "New York" },
                { "NC", "North Carolina" },
                { "ND", "North Dakota" },
                { "OH", "Ohio" },
                { "OK", "Oklahoma" },
                { "OR", "Oregon" },
                { "PA", "Pennsylvania" },
                { "RI", "Rhode Island" },
                { "SC", "South Carolina" },
                { "SD", "South Dakota" },
                { "TN", "Tennessee" },
                { "TX", "Texas" },
                { "UT", "Utah" },
                { "VT", "Vermont" },
                { "VA", "Virginia" },
                { "WA", "Washington" },
                { "WV", "West Virginia" },
                { "WI", "Wisconsin" },
                { "WY", "Wyoming" },
                { "PR", "Puerto Rico" },
                { "VI", "Virgin Islands" },
                { "GU", "Guam" }
            };

        public static Dictionary<string, string> States
        {
            get { return statesDictionary; }
        }
    }


    public static class Utilities
    {
        /// <summary>
        /// Retrieve the URL for the website
        /// </summary>
        /// <remarks>
        /// The returned value is guaranteed to end with a forward slash.
        /// </remarks>
        /// <returns>
        /// The website URL setting for the web site.
        /// </returns>
        public static string GetWebsiteURL()
        {
            string WebsiteURL = HttpContext.Current.Request.Url.Scheme + "://" +
                HttpContext.Current.Request.Url.Authority +
                HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/";
            return WebsiteURL;
        }
    }

}