using System;
using System.Collections.Generic;
using System.Data;
//using System.Linq;
using System.Web.Configuration;
//using Twilio;
//using Twilio.Rest.Api.V2010.Account;
//using Twilio.Types;

namespace Support.Miscellaneous
{
    public class AppUtils
    {
        public static string Age(DateTime? birthdate)
        {
            string Age = "";
            if (birthdate != null)
            {
                DateTime today = DateTime.Today;
                DateTime yesterday = (birthdate ?? DateTime.Today);
                int Years = today.Year - yesterday.Year;
                if (yesterday > today.AddYears(-Years)) Years--;
                Age = Years.ToString();
            }
            return Age;
        }

        // Returns the attorneyid attached to the logged-in user's record if it's not null.
        // Returns empty guid if it's null.
        public static Guid GetAttorneyId()
        {
            Guid gAttorneyid = default(Guid);
            DataRow dr = Utilities.GetUserRecord();
            if (dr != null && !dr.IsNull("attorneyid"))
            {
                gAttorneyid = (Guid)dr["attorneyid"];
            }
            return gAttorneyid;
        }

        //public static string GetDrivingDirections(DataRow Row)
        //{
        //    string strDirections = "";
        //    if (!string.IsNullOrEmpty(Row["maplink"].ToString()))
        //    {
        //        strDirections = "<p/><a href='" + Row["maplink"] + "' target='_new'>View driving directions</a></p>";
        //    }
        //    else if (!string.IsNullOrEmpty(Row["lat"].ToString().Trim()) & !string.IsNullOrEmpty(Row["lng"].ToString().Trim()))
        //    {
        //        GoogleMap gm = new GoogleMap();
        //        string strAddress = gm.EncodeAddress((string)Row["address"], (string)Row["city"], (string)Row["state"], "");
        //        string strLatLng = gm.EncodeLatLng(Row["lat"].ToString(), Row["lng"].ToString());
        //        if (!string.IsNullOrEmpty(Row["startlat"].ToString().Trim()) & !string.IsNullOrEmpty(Row["startlng"].ToString().Trim()))
        //        {
        //            string strStartAddress = gm.EncodeAddress((string)Row["startaddress"], (string)Row["startcity"], (string)Row["startstate"], "");
        //            string strStartLatLng = gm.EncodeLatLng(Row["startlat"].ToString(), Row["startlng"].ToString());
        //            strDirections = "<p><a href='" + gm.GetDrivingDirectionsLink(strAddress, strLatLng, strStartAddress, strLatLng) +
        //                "' target='_new'>View driving directions</a></p>";
        //        }
        //        else
        //        {
        //            strDirections = "<p><a href='" + gm.GetDrivingDirectionsLink(strAddress, strLatLng) +
        //                "' target='_new'>View driving directions</a></p>";
        //        }
        //    }

        //    return strDirections;
        //}

        //public static void LogCaseNote(Guid gCaseid, string strAction, string strNotes, bool isInternalNote = false)
        //{
        //    var log = new BusinessObjects.CaseLogBO();
        //    log.AddEntry(gCaseid, strAction, strNotes, isInternalNote);
        //}

        public static string RenderText(string strText, string strUsername)
        {
            strText = Utilities.ReplaceText(strText, "{websitename}", WebConfigurationManager.AppSettings["websitename"]);
            strText = Utilities.ReplaceText(strText, "{websiteurl}", WebConfigurationManager.AppSettings["websiteurl"]);
            strText = Utilities.ReplaceText(strText, "{defaultpage}", WebConfigurationManager.AppSettings["defaultpage"]);

            DataRow dr = Utilities.GetUserRecord(strUsername);
            return RenderText(strText, dr);
        }

        public static string RenderText(string strText, DataRow row)
        {
            string strName = null;
            string strPlaceholder = null;

            for (int xx = 0; xx <= row.Table.Columns.Count - 1; xx++)
            {
                strName = row.Table.Columns[xx].ColumnName.ToLower();
                strPlaceholder = "{" + strName + "}";

                if (strText.Contains(strPlaceholder))
                {
                    if (!row.IsNull(strName))
                    {
                        strText = Utilities.ReplaceText(strText, strPlaceholder, row[strName].ToString());
                    }
                    else
                    {
                        strText = Utilities.ReplaceText(strText, strPlaceholder, "");
                    }
                }
            }

            return strText;
        }

        public static string RenderText(string strText, Dictionary<string, string> data)
        {
            string strPlaceholder = null;

            foreach (string key in data.Keys)
            {
                string value = data[key];
                strPlaceholder = "{" + key + "}";
                if (strText.Contains(strPlaceholder))
                {
                    if (value != null)
                    {
                        strText = Utilities.ReplaceText(strText, strPlaceholder, value);
                    }
                    else
                    {
                        strText = Utilities.ReplaceText(strText, strPlaceholder, "");
                    }
                }
            }

            return strText;
        }

        //public static void SendSMS(string phoneNumberList, string message)
        //{
        //    bool bTestmode = false;
        //    bool.TryParse(WebConfigurationManager.AppSettings["testmode"], out bTestmode);
        //    //if (bTestmode) return;
        //    var sid = WebConfigurationManager.AppSettings["twilio_sid"];
        //    var authToken = WebConfigurationManager.AppSettings["twilio_auth_token"];
        //    var from = WebConfigurationManager.AppSettings["twilio_from"];

        //    TwilioClient.Init(sid, authToken);

        //    string[] phoneNumberArray = phoneNumberList.Split(',');
        //    for (int xx = 0; xx <= phoneNumberArray.Length - 1; xx++)
        //    {
        //        string phoneNumber = phoneNumberArray[xx];
        //        var smsMessage = MessageResource.Create(
        //            to: new PhoneNumber(phoneNumber),
        //            from: new PhoneNumber(from),
        //            body: message);
        //        if (smsMessage.ErrorCode.HasValue && smsMessage.ErrorCode != 0)
        //        {
        //            throw new Exception("Error Sending SMS Message: " +
        //                smsMessage.ErrorCode + " - " + smsMessage.ErrorMessage);
        //        }
        //    }
        //}
    }
}