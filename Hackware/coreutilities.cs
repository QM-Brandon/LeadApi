using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI.WebControls;
using Support.Data;
using System.IO;
using System.Web.UI.HtmlControls;
using System.Linq;
using System.Collections.Generic;

namespace Support.Miscellaneous
{
    /// <summary>
    /// This module contains commonly used utilities that pertain to the framework
    /// as a whole. Functions that are application-specific should be added to apputilities.cs
    /// </summary>
    public class Utilities
    {
        private static readonly string DEFAULTPWCHARS =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#@$^*()";

        /// <summary>
        /// Trims and appends a trailing backslash onto a string (if one doesn't already exist).
        /// </summary>
        /// <param name="cPath">String to which backslash is appended.</param>
        /// <returns>A new trimmed string that is guaranteed to have a trailing backslash.</returns>
        public static string AddBS(string cPath)
        {
            if (cPath.Trim().EndsWith("\\"))
            {
                return cPath.Trim();
            }
            else
            {
                return cPath.Trim() + "\\";
            }
        }

        /// <summary>
        /// Trims and appends a trailing forward slash onto a string (if one doesn't already exist).
        /// </summary>
        /// <param name="cPath">String to which forward slash is appended.</param>
        /// <returns>A new trimmed string that is guaranteed to have a trailing forward slash.</returns>
        public static string AddFS(string cPath)
        {
            if (cPath.Trim().EndsWith("/"))
            {
                return cPath.Trim();
            }
            else
            {
                return cPath.Trim() + "/";
            }
        }

        /// <summary>
        /// Retrieves a boolean value indicating if the current user is allowed access 
        /// to the selected page.
        /// </summary>
        /// <param name="strPagename">Pagename for which access is desired.</param>
        /// <returns>True if the current user is allowed acces to the specified page, otherwise false.</returns>
        public static bool AccessAllowed(string strPagename)
        {
            bool bAccessAllowed = false;
            int intUserid = Support.Miscellaneous.Utilities.GetUserID();

            // Strip off any querystring parameters from the pagename
            if (strPagename.Contains("?"))
            {
                strPagename = strPagename.Substring(0, strPagename.IndexOf("?"));
            }

            // Load the specified page
            SQLDataAccess SQLData = new SQLDataAccess();
            DataTable dt = SQLData.GetDataTable("select pageaccess, roles from pagemaster where pagename='" + strPagename + "'");
            if (dt.Rows.Count != 0)
            {
                string strPageAccess = (string)dt.Rows[0]["pageaccess"];
                string strPageRoles = (string)dt.Rows[0]["roles"];
                //Anonymous access
                //if (string.IsNullOrEmpty(strPageRoles)) {
                if (strPageAccess == "All"
                    || (strPageAccess == "Authenticated" && intUserid > 0)
                    || (strPageAccess == "Unauthenticated" && intUserid < 0)
                )
                {
                    bAccessAllowed = true;
                }
                else
                {
                    // Load the Roles for the current user
                    string[] UserRoles = Roles.GetRolesForUser();
                    if (UserRoles.Length > 0)
                    {
                        // Cycle thru user's roles and see if allowed access to the page.
                        string Role = null;
                        for (int xx = 0; xx <= UserRoles.Length - 1; xx++)
                        {
                            Role = UserRoles[xx];
                            if (("," + strPageRoles + ",").ToString().Contains("," + Role.Trim() + ","))
                            {
                                bAccessAllowed = true;
                                break; // TODO: might not be correct. Was : Exit For
                            }
                        }
                    }
                }
            }
            return bAccessAllowed;
        }

        /// <summary>
        /// Retrieves a boolean value indicating if the specific user has access 
        /// to the selected role(s).
        /// </summary>
        /// <remarks>
        /// If the provided Username is null, then the currently logged in user is used.
        /// </remarks>
        /// <param name="strRole">Role for which access is checked</param>
        /// <param name="strUsername">Username for which the access is checked (optional: default is the current user)</param>
        /// <returns>
        /// A boolean value that indicates if the specific user has access 
        /// to the selected role(s).
        /// </returns>
        public static bool AccessToRolesAllowed(string strRole, string strUsername)
        {
            bool bAccessToRoleAllowed = false;

            // Load the Roles for the current user
            string[] strUserRoles = null;
            if (string.IsNullOrEmpty(strUsername))
            {
                strUserRoles = Roles.GetRolesForUser();
            }
            else
            {
                strUserRoles = Roles.GetRolesForUser(strUsername);
            }

            return AccessToRolesAllowed(strRole, strUserRoles);
        }

        /// <summary>
        /// Retrieves a boolean value indicating if the current user has access 
        /// to the selected role(s).
        /// </summary>
        /// <param name="strRole">Role for which access is checked</param>
        /// <returns>
        /// A boolean value that indicates if the current user has access 
        /// to the selected role(s)
        /// </returns>
        public static bool AccessToRolesAllowed(string strRole)
        {
            return AccessToRolesAllowed(strRole, "");
        }

        /// <summary>
        /// Retrieves a boolean value indicating if any role in the specified comma delimited
        /// string of roles exists within a select array of roles.
        /// </summary>
        /// <param name="strRoles">A single role, or a comma delimited string of roles.</param>
        /// <param name="strUserRoles">An array of roles.</param>
        /// <returns>
        /// A boolean value that indicates if at least one role in the specified comma delimited
        /// string of roles exists within the provided array of roles.
        /// </returns>
        public static bool AccessToRolesAllowed(string strRoles, string[] strUserRoles)
        {
            bool bAccessToRoleAllowed = false;

            string[] strRoleArray = strRoles.Split(',');
            string strRole = null;
            for (int xx = 0; xx <= strRoleArray.Length - 1; xx++)
            {
                strRole = strRoleArray[xx];
                if (Array.IndexOf(strUserRoles, strRole) > -1)
                {
                    bAccessToRoleAllowed = true;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }

            return bAccessToRoleAllowed;
        }

        /// <summary>
        /// Gets a string that contains the script code required to use CKEditor.
        /// </summary>
        /// <returns>A string that contains the script code required to use CKEditor.</returns>

        public static string GetCKEditorLinks()
        {
            return GetCKEditorLinks("images", "Images", "");
        }
        public static string GetCKEditorLinks(string strImageFolder, string strImageFolderName)
        {
            return GetCKEditorLinks(strImageFolder, strImageFolderName, "");
        }
        public static string GetCKEditorLinks(string strImageFolder, string strImageFolderName,
            string strCKCustomConfigFilename)
        {
            string VirtualRoot = GetWebsiteURL();
            if (strCKCustomConfigFilename != "")
            {
                strCKCustomConfigFilename = "CKEDITOR.config.customConfig = '" +
                    VirtualRoot + strCKCustomConfigFilename + "';\n";
            }
            string strLink =
                "<script type='text/javascript' src='" + VirtualRoot + "ckeditor/ckeditor.js'></script>\n" +
                "<script type='text/javascript' src='" + VirtualRoot + "ckeditor/adapters/jquery.js'></script>\n" +
                "<script type='text/javascript'>\n" +
                strCKCustomConfigFilename +
                "$(document).ready(function() {\n" +
                "   $('.enableckeditor').each(function() {\n" +
                "      objid = $(this).attr('id');\n" +
                "      CKEDITOR.replace(objid, {\n" +
                "         'extraPlugins' : 'imagebrowser',\n" +
                "         'imageBrowser_listUrl' : '" +
                             VirtualRoot + "GetImageList.ashx?folder=" + HttpUtility.UrlEncode(strImageFolder) +
                             "&foldername=" + HttpUtility.UrlEncode(strImageFolderName) +
                             "&fullpath=yes'" +
                "      });\n" +
                "   });\n" +
                "})\n" +
                "</script>\n";

            return strLink;
        }

        /// <summary>
        /// Returns the current datetime offset by the hours configured in the web.config.
        /// </summary>
        /// <remarks>
        /// Returns the default datetime if no offset configured in the web.config.
        /// </remarks>
        /// <returns>
        /// The current datetime offset by the number of hours specified in the web.config
        /// </returns>
        public static DateTime GetOffsetDateTime()
        {
            int offsetHours = 0;
            string strOffsetHours = WebConfigurationManager.AppSettings["timeoffsethours"];
            int.TryParse(strOffsetHours, out offsetHours);
            DateTime offsetDatetime;
            try
            {
                offsetDatetime = DateTime.Now.AddHours(offsetHours);
            }
            catch (Exception ex)
            {
                offsetDatetime = DateTime.Now;
            }

            return offsetDatetime;
        }

        /// <summary>
        /// Converts a DateTime struct to a date string, formatted as M/d/yyyy.
        /// </summary>
        /// <remarks>
        /// Will return an empty string if the DateTime date is equal to 1/1/1900.
        /// </remarks>
        /// <param name="tDatetime">A DateTime struct to be formatted as a date string.</param>
        /// <returns>
        /// A date string formatted as M/d/yyyy or an empty string if the date is equal to 1/1/1900.
        /// </returns>
        public static string GetDate(DateTime tDatetime)
        {
            string strDate = "";
            try
            {
                strDate = tDatetime.ToShortDateString();
                if (strDate == "1/1/1900")
                {
                    strDate = "";
                }
            }
            catch (Exception ex)
            {
                strDate = "";
            }

            return strDate;
        }

        /// <summary>
        /// Gets a formatted string that contans a start and end DateTime.
        /// The returned string will be formatted as follows :
        /// M/d/yyyy hh:mm tt thru M/d/yyyy hh:mm.
        /// </summary>
        /// <param name="dtStart">The start DateTime.</param>
        /// <param name="dtEnd">The end DateTime.</param>
        /// <returns>A formatted string that contans a start and end DateTime.</returns>
        public static string GetEventDateTimes(DateTime? dtStart, DateTime? dtEnd)
        {
            StringBuilder sbDate = new StringBuilder();
            if (dtStart.HasValue)
            {
                string strDate = Utilities.GetDate(dtStart.Value);
                string strTime = Utilities.GetTime(dtStart.Value);
                if (string.IsNullOrEmpty(strTime))
                {
                    sbDate.Append(strDate);
                }
                else
                {
                    sbDate.Append(strDate + " " + strTime);
                }
                if (dtEnd.HasValue)
                {
                    // Append ending date and time
                    string strDate2 = Utilities.GetDate(dtEnd.Value);
                    string strTime2 = Utilities.GetTime(dtEnd.Value);
                    if (!string.IsNullOrEmpty(strDate2) & dtStart != dtEnd)
                    {
                        if (strDate == strDate2)
                        {
                            sbDate.Append(" thru " + strTime2);
                        }
                        else
                        {
                            sbDate.Append(" thru " + strDate2 + " " + strTime2);
                        }
                    }
                }
            }

            return sbDate.ToString();
        }

        /// <summary>
        /// Converts a DateTime struct to a string, formatted as hh:mm tt.
        /// </summary>
        /// <remarks>
        /// Will return an empty string if the DateTime time is equal to 12:01 AM.
        /// </remarks>
        /// <param name="tDatetime">A DateTime struct to be formatted as a time string.</param>
        /// <returns>
        /// A time string formatted as hh:mm tt or an empty string if the time is equal to 12:01 AM.
        /// </returns>
        public static string GetTime(DateTime tDatetime)
        {
            string strTime = "";
            try
            {
                strTime = tDatetime.ToShortTimeString();
                if (strTime == "12:01 AM")
                {
                    // When time set to 12:01 AM it signals that now time was selected.
                    strTime = "";
                }
            }
            catch (Exception ex)
            {
                strTime = "";
            }

            return strTime;
        }

        /// <summary>
        /// Get a DateTime structure that is set to the last millisecond of the provided dateTime.
        /// </summary>
        /// <remarks>
        /// This method actually returns the 3 to last millisecond of the provided dateTime in order
        /// to work with MSSQL millisecond precision restrictions.
        /// </remarks>
        /// <param name="dateTime">The DateTime</param>
        /// <returns>A DateTime structure that is set to the last millisecond of the provided dateTime.</returns>
        public static DateTime GetEOD(DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddMilliseconds(-3);
            // MSSQL only supports 3 millisecond interval granularity
        }

        /// <summary>
        /// Get a DateTime structure that has a time set to 00:00:00.0000.
        /// </summary>
        /// <param name="datetime">The DateTime</param>
        /// <returns>A DateTime structure that has a time set to 00:00:00.0000.</returns>
        public static DateTime GetBOD(DateTime datetime)
        {
            return datetime.Date;
        }

        public static string FormatEndingSQLDateTime(DateTime dateTime)
        {
            return GetEOD(dateTime).ToString("MM/dd/yyyy HH:mm:ss.fff");
        }

        /// <summary>
        /// Get the last day of the month for the given dateTime structure.
        /// </summary>
        /// <param name="dateTime">The DateTime</param>
        /// <returns>The last day of the month for the given dateTime structure.</returns>
        public static DateTime GetLDOM(DateTime dateTime)
        {
            return (new DateTime(dateTime.Year, dateTime.Month, 1)).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        /// Get the first day of the month for the given dateTime structure.
        /// </summary>
        /// <param name="dateTime">The DateTime</param>
        /// <returns>The first day of the month for the given dateTime structure.</returns>
        public static DateTime getFDOM(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        // Retrieves the selected values in the ListBox instance and returns as a comma-delimited list of strings
        public static string GetListboxSelectedValues(ListBox listBox)
        {
            string values = string.Empty;
            foreach (var i in listBox.GetSelectedIndices())
            {
                values += "'" + listBox.Items[i].Value + "',";
            }
            values = values.TrimEnd(',');
            return values;
        }

        /// <summary>
        /// Gets the description of a value in the Lookup table.
        /// </summary>
        /// <param name="Type">The Lookup Type value.</param>
        /// <param name="Itemvalue">The Lookup Itemvalue value.</param>
        /// <returns>
        /// The description for the provided Lookup Type and ItemValue.
        /// </returns>
        public static string GetLookupDescription(string Type, string Itemvalue)
        {
            // Retrieve the description for a particular Lookup table item.
            string ReturnStr = "";
            Business.BusinessObject oData = new Business.BusinessObject();
            string CommandText = "select itemdescription from lookup where type='" + Type + "' and itemvalue='" + Itemvalue + "'";
            DataTable dt = oData.GetDataTable(CommandText);
            if (dt.Rows.Count > 0)
            {
                ReturnStr = (string)dt.Rows[0]["itemdescription"];
            }
            return ReturnStr;
        }

        /// <summary>
        /// Get a comma-delimited list of Roles assigned to a user.
        /// </summary>
        /// <param name="strUsername">A username for which Roles will be returned.</param>
        /// <returns>
        /// A comma-delimited list of the Roles assigned to the specified user.
        /// </returns>
		public static string GetUserRolesList(string strUsername)
        {
            StringBuilder sbRoles = new StringBuilder();
            string strRole = null;
            // Load the Roles for the current user
            string[] strRoles = Roles.GetRolesForUser(strUsername);
            if (strRoles.Length > 0)
            {
                for (int xx = 0; xx <= strRoles.Length - 1; xx++)
                {
                    strRole = strRoles[xx];
                    if (xx == 0)
                    {
                        sbRoles.Append(strRole);
                    }
                    else
                    {
                        sbRoles.Append("," + strRole);
                    }
                }
            }

            return sbRoles.ToString();
        }

        /// <summary>
        /// Gets the <c>QueryString</c> value for a specified parameter name.  If the specified
        /// parameter name does not exist, then a default value will be returned.
        /// </summary>
        /// <param name="QSRef">The name of a <c>QueryString</c> parameter.</param>
        /// <param name="DefaultValue">
        /// A default value to be returned if the provided <paramref name="QSRef"/> value
        /// was not passed as a <c>QueryString</c> parameter.
        /// </param>
        /// <param name="ConvertToLowerCase">
        /// A flag indicating if the returned value should be converted to lower case.
        /// </param>
        /// <returns>
        /// Either the <c>QueryString</c> value for the provided parameter name, or the value
        /// provided with <paramref name="DefaultValue"/> if <c>QueryString</c> does not have
        /// the specified parameter.
        /// </returns>
        public static string GetQueryString(string QSRef, string DefaultValue, bool ConvertToLowerCase)
        {
            string ReturnStr = DefaultValue;
            try
            {
                ReturnStr = HttpContext.Current.Request.QueryString[QSRef];
                //if (ReturnStr.Equals(System.DBNull.Value))
                if (ReturnStr == null)
                {
                    ReturnStr = DefaultValue;
                }
                else
                {
                    if (ConvertToLowerCase)
                    {
                        ReturnStr = ReturnStr.ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                ReturnStr = DefaultValue;
            }

            return ReturnStr;
        }

        /// <summary>
        /// An overloaded version of the <c>GetQueryString</c> method that uses
        /// an empty string as the default value and always converts the
        /// returned value to lower case.
        /// </summary>
        /// <param name="QSRef">The name of a <c>QueryString</c> parameter.</param>
        /// <returns>
        /// Either the <c>QueryString</c> value converted to lower case for the provided
        /// parameter name, or and empty string if <c>QueryString</c> does not have the
        /// specified parameter.
        /// </returns>
        public static string GetQueryString(string QSRef)
        {
            return GetQueryString(QSRef, "", true);
        }

        /// <summary>
        /// An overloaded version of the <c>GetQueryString</c> method that always converts
        /// the returned value to lower case.
        /// </summary>
        /// <param name="QSRef">The name of a <c>QueryString</c> parameter.</param>
        /// <param name="defaultValue">
        /// A default value to be returned if the provided <paramref name="QSRef"/> value
        /// was not passed as a <c>QueryString</c> parameter.
        /// </param>
        /// <returns>
        /// Either the <c>QueryString</c> value for the provided parameter name, or the value
        /// provided with <paramref name="defaultValue"/> if <c>QueryString</c> does not have
        /// the specified parameter.  In either case, the returned value will be converted to
        /// lower case.
        /// </returns>
        public static string GetQueryString(string QSRef, string defaultValue)
        {
            return GetQueryString(QSRef, defaultValue, true);
        }

        /// <summary>
        /// Converts a <c>QueryString</c> parameter to an <c>int</c>.
        /// </summary>
        /// <param name="QSRef">The name of a <c>QueryString</c> parameter.</param>
        /// <param name="DefaultValue">
        /// A default <c>int</c> value to be returned if the <c>QueryString</c> parameter
        /// can not be parsed as an <c>int</c>, or if it wasn't passed as a <c>QueryString</c>
        /// parameter.
        /// </param>
        /// <returns>
        /// A <c>QueryString</c> parameter value converted to an <c>int</c> or
        /// <paramref name="DefaultValue"/> if the parameter does not exist or
        /// cannot be converted to an <c>int</c>.
        /// </returns>
		public static int GetQueryString(string QSRef, int DefaultValue)
        {
            int ReturnNum = DefaultValue;
            try
            {
                ReturnNum = int.Parse(HttpContext.Current.Request.QueryString[QSRef],
                    System.Globalization.NumberStyles.Integer);
            }
            catch (Exception ex)
            {
                ReturnNum = DefaultValue;
            }

            return ReturnNum;
        }

        /// <summary>
        /// Returns a boolean value indicating whether the provided <c>TextBox</c> has a value.
        /// Whenever the value is empty, this method will also add the CSS class <c>inputerror</c>
        /// to the provided control and add the <paramref name="strMessage"/> message to
        /// <paramref name="sbErrorMessages"/> as an &ltli;&gt; item.  Otherwise, the CSS class
        /// <c>inputerror</c> will be removed from the control.
        /// </summary>
        /// <param name="Textbox">A TextBox.</param>
        /// <param name="sbErrorMessages">
        /// A <c>StringBuilder</c> to which <paramref name="strMessage"/> will be added if the
        /// provided <c>TextBox</c> is empty.
        /// </param>
        /// <param name="strMessage">
        /// A message that will be added to <paramref name="sbErrorMessages"/> as a &ltli;&gt;
        /// item if the provided <c>TextBox</c> is empty.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the provided <c>TextBox</c> has a value.
        /// </returns>
        public static bool CheckRequired(TextBox Textbox, StringBuilder sbErrorMessages, string strMessage)
        {
            bool bValid = (!string.IsNullOrEmpty(Textbox.Text));
            if (!bValid)
            {
                sbErrorMessages.AppendLine("<li>" + strMessage + "</li>");
                SetError(Textbox, true);
            }
            else
            {
                SetError(Textbox, false);
            }
            return bValid;
        }

        /// <summary>
        /// Returns a boolean value indicating whether the provided <c>DropDownList.SelectedValue</c>
        /// has a value.  Whenever the SelectedValue is empty, this method will also add the CSS class
        /// <c>inputerror</c> to the provided control and add the <paramref name="strMessage"/> message
        /// to <paramref name="sbErrorMessages"/> as an &ltli;&gt; item.  Otherwise, the CSS class
        /// <c>inputerror</c> will be removed from the control.
        /// </summary>
        /// <param name="DropDownRef">A DropDownList control.</param>
        /// <param name="sbErrorMessages">
        /// A <c>StringBuilder</c> to which <paramref name="strMessage"/> will be added if the
        /// provided <c>DropDownList.SelectedValue</c> is empty.
        /// </param>
        /// <param name="strMessage">
        /// A message that will be added to <paramref name="sbErrorMessages"/> as a &ltli;&gt;
        /// item if the provided <c>DropDownList.SelectedValue</c> is empty.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the provided <c>TextBox</c> has a value.
        /// </returns>
        public static bool CheckRequired(DropDownList DropDownRef, StringBuilder sbErrorMessages, string strMessage)
        {
            bool bValid = (!string.IsNullOrEmpty(DropDownRef.SelectedValue));
            if (!bValid)
            {
                sbErrorMessages.AppendLine("<li>" + strMessage + "</li>");
                SetError(DropDownRef, true);
            }
            else
            {
                SetError(DropDownRef, false);
            }
            return bValid;
        }

        /// <summary>
        /// Adds or removes the CSS class <c>inputerror</c> to the provided <c>WebControl</c>
        /// as determined by the <paramref name="lFlagAsError"/> parameter.
        /// </summary>
        /// <param name="oObject">A WebControl to mark or unmark as having an error.</param>
        /// <param name="lFlagAsError">
        /// A flag to indicate whether the CSS class <c>inputerror</c> should be added or
        /// removed from the provided WebControl.  A value of <c>true</c> will add the class
        /// and a value of <c>false</c> will remove it.
        /// </param>
		public static void SetError(System.Web.UI.WebControls.WebControl oObject, bool lFlagAsError)
        {
            string strClass = oObject.CssClass;
            if (lFlagAsError && !strClass.Contains("inputerror"))
            {
                strClass += " inputerror";
            }
            if (!lFlagAsError && strClass.Contains("inputerror"))
            {
                strClass = strClass.Replace("inputerror", "");
            }

            oObject.CssClass = strClass.Trim();
        }

        /// <summary>
        /// Retrieve the virtual root setting for the web site as specified in the
        /// web.config <c>virtualroot</c> <c>AppSetting</c> value.
        /// </summary>
        /// <returns>
        /// The virtual root setting for the web site as specified in the
        /// web.config <c>virtualroot</c> <c>AppSetting</c> value or "/" if
        /// <c>virtualroot</c> was not set.
        /// </returns>
		public static string GetVirtualRoot()
        {
            string Virtualroot = WebConfigurationManager.AppSettings["virtualroot"];
            if (string.IsNullOrEmpty(Virtualroot.Trim()))
            {
                Virtualroot = "/";
            }
            return Virtualroot;
        }

        // Append the selected options to the supplied where clause stringbuilder
        public static void AppendWhereClauseForListbox(ListBox ddListBox, List<int> selectedDropDownItems, StringBuilder whereClause, string columnName)
        {
            if (selectedDropDownItems != null && selectedDropDownItems.Count > 0)
            {
                var selectedList = new List<string>();
                var delimiter = "'";

                foreach (var id in selectedDropDownItems)
                {
                    if (!string.IsNullOrWhiteSpace(ddListBox.Items[id].Value))
                    {
                        selectedList.Add(string.Format("{0}{1}{0}", delimiter, ddListBox.Items[id].Value.ToString()));
                    }
                }

                if (selectedList.Count > 0)
                {
                    whereClause.Append((whereClause.Length > 0 ? " AND" : " WHERE") +
                        string.Format(" {0} IN ({1})", columnName, string.Join(",", selectedList)));
                }
            }
        }

        public static void AppendWhereClauseForDate(StringBuilder whereClause, string date, string columnName, string searchOperator)
        {
            if (!string.IsNullOrEmpty(date))
            {
                DateTime searchDate;

                if (DateTime.TryParse(date, out searchDate))
                {
                    whereClause.Append((whereClause.Length > 0 ? " AND" : " WHERE") +
                        string.Format(" {0} {1} '{2}'", columnName, searchOperator, searchDate.ToString("MM/dd/yyyy")));
                }
            }
        }


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

        /// <summary>
        /// Retrieve the fully qualified folder path for the current web page.
        /// </summary>
        /// <remarks>
        /// The returned value is guaranteed to end with a forward slash.
        /// </remarks>
        /// <returns>
        /// The fully qualified folder path for the current web page.
        /// </returns>
		public static string GetCurrentFolder()
        {
            string strFolder = AddFS(GetWebsiteURL() + HttpContext.Current.Items["currentfolder"]);
            return strFolder;
        }

        /// <summary>
        /// Get the DefaultRole of the Virtual record for the current folder.
        /// </summary>
        /// <returns>
        /// The DefaultRole of the Virtual record for the current folder if it exists.
        /// Otherwise, the value of the web.config <c>defaultrole</c> <c>AppSetting</c>
        /// is returned.
        /// </returns>
		public static string GetDefaultRole()
        {
            return GetDefaultRole("");
        }

        /// <summary>
        /// Gets the DefaultRole of the Virtual record for the provided
        /// <paramref name="strFoldername"/>.
        /// </summary>
        /// <param name="strFoldername">
        /// A virtual folder name used to lookup a Virtual record.
        /// </param>
        /// <returns>
        /// The DefaultRole of the Virtual record for the provided <paramref name="strFoldername"/> if
        /// it exists.  Otherwise, the value of the web.config <c>defaultrole</c> <c>AppSetting</c>
        /// is returned.
        /// </returns>
		public static string GetDefaultRole(string strFoldername)
        {
            string strDefaultRole = WebConfigurationManager.AppSettings["defaultrole"];
            if (string.IsNullOrEmpty(strFoldername))
            {
                // Load the virtual folder saved in URLRemapper for the incoming request.
                strFoldername = (string)HttpContext.Current.Items["currentfolder"];
            }
            // Load default role for the virtual folder.
            if (!string.IsNullOrEmpty(strFoldername))
            {
                Business.BusinessObject oData = new Business.BusinessObject();
                DataTable dt = oData.GetDataTable("select * from virtual where foldername='" + strFoldername + "'");
                if (dt.Rows.Count > 0 && !string.IsNullOrEmpty((string)dt.Rows[0]["defaultrole"]))
                {
                    strDefaultRole = (string)dt.Rows[0]["defaultrole"];
                }
            }

            return strDefaultRole;

        }

        //********* MOVED TO APPUTILITIES.VB
        //Public Shared Function GetDrivingDirections(ByVal Row As DataRow) As String

        //    Dim strDirections As String = ""
        //    If Row("maplink").ToString <> "" Then
        //        strDirections = "<p/><a href='" + Row("maplink") + _
        //            "' target='_new'>View driving directions</a></p>"
        //    ElseIf Row("lat").ToString.Trim <> "" And Row("lng").ToString.Trim <> "" Then
        //        Dim gm As New GoogleMap
        //        'Dim strAddress As String = gm.EncodeAddress(Row("location"), Row("address"), Row("city"), Row("state"), "")
        //        Dim strAddress As String = gm.EncodeAddress(Row("address"), Row("city"), Row("state"), "")
        //        Dim strLatLng As String = gm.EncodeLatLng(Row("lat"), Row("lng"))
        //        If Row("startlat").ToString.Trim <> "" And Row("startlng").ToString.Trim <> "" Then
        //            'Dim strStartAddress As String = gm.EncodeAddress(Row("startlocation"), Row("startaddress"), Row("startcity"), Row("startstate"), "")
        //            Dim strStartAddress As String = gm.EncodeAddress(Row("startaddress"), Row("startcity"), Row("startstate"), "")
        //            Dim strStartLatLng As String = gm.EncodeLatLng(Row("startlat"), Row("startlng"))
        //            strDirections = "<p><a href='" + gm.GetDrivingDirectionsLink(strAddress, strLatLng, strStartAddress, strLatLng) + _
        //                "' target='_new'>View driving directions</a></p>"
        //        Else
        //            strDirections = "<p><a href='" + gm.GetDrivingDirectionsLink(strAddress, strLatLng) + _
        //                "' target='_new'>View driving directions</a></p>"
        //        End If
        //    End If
        //    Return strDirections

        //End Function

        /// <summary>
        /// Get the HomePage of the Virtual record for the current folder.
        /// </summary>
        /// <returns>
        /// The HomePage of the Virtual record for the current folder if it exists.
        /// Otherwise, the value of the web.config <c>defaultpage</c> <c>AppSetting</c>
        /// is returned.
        /// </returns>
        public static string GetHomepage()
        {
            return GetHomepage("");
        }

        /// <summary>
        /// Gets the HomePage of the Virtual record for the provided
        /// <paramref name="strFoldername"/>.
        /// </summary>
        /// <param name="strFoldername">
        /// A virtual folder name used to lookup a Virtual record.
        /// </param>
        /// <returns>
        /// The HomePage of the Virtual record for the provided <paramref name="strFoldername"/> if
        /// it exists.  Otherwise, the value of the web.config <c>defaultpage</c> <c>AppSetting</c>
        /// is returned.
        /// </returns>
        public static string GetHomepage(string strFoldername)
        {
            string strHomepage = WebConfigurationManager.AppSettings["defaultpage"];
            if (string.IsNullOrEmpty(strFoldername))
            {
                // Load the virtual folder saved in URLRemapper for the incoming request.
                strFoldername = (string)HttpContext.Current.Items["currentfolder"];
            }
            // Load homepage for the virtual folder.
            if (!string.IsNullOrEmpty(strFoldername))
            {
                Business.BusinessObject oData = new Business.BusinessObject();
                DataTable dt = oData.GetDataTable("select * from virtual where foldername='" + strFoldername + "'");
                if (dt.Rows.Count > 0 && !string.IsNullOrEmpty((string)dt.Rows[0]["homepage"]))
                {
                    strHomepage = (string)dt.Rows[0]["homepage"];
                }
            }

            return strHomepage;
        }

        /// <summary>
        /// Returns the digits of a phone number minus any formatting or leading "1" for "+1" strings
        /// </summary>
        /// <param name="text">
        /// The text string containing the phone number to be cleaned
        /// </param>
        /// <returns>
        /// The cleaned-up phone number 
        /// </returns>
        public static string GetCleanPhonenumber(string text)
        {
            string numbers = JustNumbers(text); // Strip off non-numeric characters
            // Remove leading "1" from phone number when it exists
            if (numbers.Length == 11 && numbers.Substring(0, 1) == "1")
            {
                numbers = numbers.Substring(1, numbers.Length-1);
            }
            return numbers;
        }

        /// <summary>
        /// Gets a Template for the current Virtual folder and Page.
        /// </summary>
        /// <returns>
        /// A Template for the current Virtual folder and Page.
        /// See <see cref="GetTemplate(string,string,string)"/> for a description of the
        /// logic used to determine which Template will be returned.
        /// </returns>
		public static string GetTemplate()
        {
            return GetTemplate("", "", "template.master");
        }

        /// <summary>
        /// Gets a Template for the provided <paramref name="strFoldername"/> Virtual folder
        /// and <paramref name="strPagemane"/> Page.  If a Template is specified for the provided
        /// page in the PageMaster table, then that Template is returned, otherwise, the
        /// Template from the Virtual record corresponding to <paramref name="strFoldername"/>
        /// is returned.
        /// </summary>
        /// <param name="strFoldername">
        /// A virtual folder name used to lookup a Virtual record.  The current folder
        /// is used if this value is null.
        /// </param>
        /// <param name="strPagename">
        /// A page name used to lookup a PageMaster record.  The current page is used
        /// if this value is nulll.
        /// </param>
        /// <param name="strTemplate">
        /// A default Template value that will be returned if a Template isn't found for
        /// the Page or the Virtual folder.
        /// </param>
        /// <returns>
        /// A template for the provided <paramref name="strFoldername"/> Virtual folder and
        /// <paramref name="strPagename"/> Page.  If <paramref name="strPagename"/> is found
        /// in PageMaster and includes a Template, then that value is returned.  If it
        /// is not found in PageMaster or PageMaster does not include a Template, then
        /// the Template for the Virtual folder will be returned.  Otherwise, if neither
        /// the PageMaster record or the Virtual folder record contains a Template, then
        /// the default value <paramref name="strTemplate"/> will be returned.
        /// </returns>
        public static string GetTemplate(string strFoldername, string strPagename, string strTemplate)
        {
            if (string.IsNullOrEmpty(strFoldername))
            {
                // Load the virtual folder saved in URLRemapper for the incoming request.
                strFoldername = (string)HttpContext.Current.Items["currentfolder"];
            }
            if (string.IsNullOrEmpty(strPagename))
            {
                // Load the current url name saved in URLRemapper for the incoming request.
                strPagename = (string)HttpContext.Current.Items["currentpage"];
            }

            // Load default template for the virtual folder.
            if (!string.IsNullOrEmpty(strFoldername))
            {
                Business.BusinessObject oData = new Business.BusinessObject();
                DataTable dt = oData.GetDataTable("select * from virtual where foldername='" + strFoldername + "'");
                if (dt.Rows.Count > 0 && !string.IsNullOrEmpty((string)dt.Rows[0]["template"]))
                {
                    strTemplate = (string)dt.Rows[0]["template"];
                }
            }

            // Load pagemaster record and look for overrriding template.
            if (!string.IsNullOrEmpty(strPagename))
            {
                //LogMessage("strPagename=" + strPagename);
                int intPageid = 0;

                BusinessObjects.PagemasterBO oPagemaster = new BusinessObjects.PagemasterBO();
                if (strPagename.Contains("showpage.aspx"))
                {
                    intPageid = Convert.ToInt32(HttpContext.Current.Items["pageid"]);
                    oPagemaster.GetRecordByID(intPageid);
                }
                else
                {
                    oPagemaster.GetRecordByPage(strPagename);
                    if (oPagemaster.HasRows) intPageid = (int)oPagemaster.Row["pageid"];
                }
                if (oPagemaster.dt.Rows.Count > 0 && !oPagemaster.Row.IsNull("pagetemplate") &&
                    ((string)oPagemaster.Row["pagetemplate"]).Trim().Length > 0)
                {
                    strTemplate = (string)oPagemaster.Row["pagetemplate"];
                }
            }

            return strTemplate;
        }

        /// <summary>
        /// Gets the ASP.NET UserID as a string for the current user.
        /// </summary>
        /// <returns>
        /// The ASP.NET UserID as a string for the current user.
        /// </returns>
		public static string GetAspnetUserID()
        {
            return GetAspnetUserID("");
        }

        /// <summary>
        /// Gets the ASP.NET UserID as a string for the specified <paramref name="strUsername"/>.
        /// </summary>
        /// <param name="strUsername">
        /// The Username used to lookup the User record.
        /// </param>
        /// <returns>
        /// The ASP.NET UserID as a string for the specified <paramref name="strUsername"/>.
        /// </returns>
		public static string GetAspnetUserID(string strUsername)
        {
            string strAspnetUserid = "";
            try
            {
                if (string.IsNullOrEmpty(strUsername))
                {
                    // When not supplied, retrieve the currently logged-in username
                    MembershipUser mu = Membership.GetUser();
                    strUsername = mu.UserName;
                }

                MembershipUser User = default(MembershipUser);
                if (!string.IsNullOrEmpty(strUsername))
                {
                    User = Membership.GetUser(strUsername);
                    strAspnetUserid = User.ProviderUserKey.ToString();
                }
            }
            catch (Exception ex)
            {
            }

            return strAspnetUserid;
        }

        /// <summary>
        /// Gets the property UserId for the current user.
        /// </summary>
        /// <returns>
        /// The property UserId for the current user.
        /// </returns>
		public static int GetUserID()
        {
            return GetUserID("");
        }

        /// <summary>
        /// Gets the UserId for the User record corresponding to the
        /// provided <paramref name="strUsername"/>.
        /// </summary>
        /// <param name="strUsername">
        /// The Username used to lookup the User record.
        /// </param>
        /// <returns>
        /// The UserId for the User specified by <paramref name="strUsername"/>.
        /// </returns>
        public static int GetUserID(string strUsername)
        {
            int intUserid = -1;
            try
            {
                if (string.IsNullOrEmpty(strUsername))
                {
                    // When not supplied, retrieve the currently logged-in username
                    MembershipUser mu = Membership.GetUser();

                    if (mu != null)
                        strUsername = mu.UserName;
                }

                if (!string.IsNullOrEmpty(strUsername))
                {
                    BusinessObjects.UsersBO oUsers = new BusinessObjects.UsersBO();
                    oUsers.GetRecordByID(strUsername, "username");

                    intUserid = Convert.ToInt32(oUsers.Row["userid"]);
                }
            }
            catch (Exception ex)
            {
                intUserid = -1;
            }

            return intUserid;
        }

        /// <summary>
        /// Adds Membership columns to a <c>DataTable</c>.
        /// </summary>
        /// <remarks>
        /// This method is primarily used to add Membership columns to
        /// a DataTable that stores the User data.
        /// </remarks>
        /// <param name="dt">
        /// The <c>DataTable</c> that Membership columns will be added to.
        /// </param>
		public static void AddMembershipColumns(DataTable dt)
        {
            dt.Columns.Add("IsOnline", Type.GetType("System.Boolean"));
            dt.Columns.Add("PasswordQuestion", Type.GetType("System.String"));
            dt.Columns.Add("IsLockedOut", Type.GetType("System.Boolean"));
            dt.Columns.Add("CreationDate", Type.GetType("System.DateTime"));
            dt.Columns.Add("LastLoginDate", Type.GetType("System.DateTime"));
            dt.Columns.Add("Password", Type.GetType("System.String"));
        }

        /// <summary>
        /// Gets a DataRow that contains all of the data from the User table
        /// and the pertinent ASP.NET authentication data for the provided
        /// <paramref name="intUserid"/>.
        /// </summary>
        /// <param name="intUserid">
        /// The Userid for the User data to be returned.
        /// </param>
        /// <returns>
        /// A DataRow that contains all of the data from the User table
        /// and the pertinent ASP.NET authentication data.  If a record
        /// is not found for the provided Userid, then a DataRow with
        /// default values will be returned.
        /// </returns>
		public static DataRow GetUserRecord(int intUserid)
        {
            DataRow drUsers = null;

            BusinessObjects.UsersBO oUsers = new BusinessObjects.UsersBO();
            oUsers.GetRecordByID(intUserid, "userid");
            AddMembershipColumns(oUsers.dt);

            if (oUsers.dt.Rows.Count == 0)
            {
                oUsers.AddNewRow();
                drUsers = oUsers.dt.Rows[0];
                drUsers["userid"] = 0;
            }
            else
            {
                string strUsername = (string)oUsers.Row["username"];
                drUsers = oUsers.dt.Rows[0];
                MembershipUser User = Membership.GetUser(strUsername);
                drUsers["IsOnline"] = User.IsOnline;
                drUsers["PasswordQuestion"] = User.PasswordQuestion;
                drUsers["IsLockedOut"] = User.IsLockedOut;
                drUsers["IsApproved"] = User.IsApproved;
                drUsers["CreationDate"] = User.CreationDate;
                drUsers["LastLoginDate"] = User.LastLoginDate;
                drUsers["Password"] = User.GetPassword();
            }

            return drUsers;
        }

        /// <summary>
        /// Get a DataRow that contains all of the data from the User table
        /// and the pertinent ASP.NET authentication data for the current user.
        /// </summary>
        /// <returns>
        /// A DataRow that contains all of the data from the User table
        /// and the pertinent ASP.NET authentication data for the current user.
        /// </returns>
		public static DataRow GetUserRecord()
        {
            return GetUserRecord("");
        }

        /// <summary>
        /// Gets a DataRow that contains all of the data from the User table
        /// and the pertinent ASP.NET authentication data for the provided
        /// <paramref name="strUsername"/>.
        /// </summary>
        /// <param name="strUsername">
        /// The Username for the User data to be returned.
        /// </param>
        /// <returns>
        /// A DataRow that contains all of the data from the User table
        /// and the pertinent ASP.NET authentication data.  If a record
        /// is not found for the provided Username, then a DataRow with
        /// default values will be returned.
        /// </returns>
        public static DataRow GetUserRecord(string strUsername)
        {
            // Retrieve the login and profile information for the specified user.
            DataRow drUsers = null;

            // When not supplied, retrieve the currently logged-in username
            if (string.IsNullOrEmpty(strUsername))
            {
                MembershipUser mu = Membership.GetUser();
                strUsername = mu.UserName;
            }

            BusinessObjects.UsersBO oUsers = new BusinessObjects.UsersBO();
            oUsers.GetRecordByID(strUsername, "username");
            AddMembershipColumns(oUsers.dt);

            if (oUsers.dt.Rows.Count == 0)
            {
                oUsers.AddNewRow();
                drUsers = oUsers.dt.Rows[0];
                drUsers["userid"] = 0;
            }
            else
            {
                drUsers = oUsers.dt.Rows[0];

                MembershipUser User = Membership.GetUser(strUsername);
                drUsers["IsOnline"] = User.IsOnline;
                drUsers["PasswordQuestion"] = User.PasswordQuestion;
                drUsers["IsLockedOut"] = User.IsLockedOut;
                drUsers["IsApproved"] = User.IsApproved;
                drUsers["CreationDate"] = User.CreationDate;
                drUsers["LastLoginDate"] = User.LastLoginDate;
                drUsers["Password"] = User.GetPassword();
            }

            return drUsers;
        }

        /// <summary>
        /// Gets a <c>DataTable</c> of all Users.
        /// </summary>
        /// <returns>
        /// A <c>DataTable</c> of all Users that includes the data from the User table
        /// and the ASP.NET authentication data.
        /// </returns>
        public static DataTable GetUserTable()
        {
            return GetUserTable("");
        }

        /// <summary>
        /// Gets a <c>DataTable</c> of all Users that are assigned the provided
        /// <paramref name="strRole"/>.
        /// </summary>
        /// <param name="strRole">
        /// The Role used to filter the User records.  If strRole is null, then all User
        /// records are returned.
        /// </param>
        /// <returns>
        /// A <c>DataTable</c> that contains all Users that are assigned the provided
        /// <paramref name="strRole"/>.  If  strRole is null, then all User records are returned.
        /// The returned data includes both the User table data and the ASP.NET authentication
        /// data.
        /// </returns>
		public static DataTable GetUserTable(string strRole)
        {
            // Create empty datatable to where the user list is stored.
            BusinessObjects.UsersBO oUsers = new BusinessObjects.UsersBO();
            // Load all entries from Users table.
            string strSQL = null;
            if (string.IsNullOrEmpty(strRole))
            {
                strSQL = "select *, LastName + ', ' + FirstName as LastFirstName from users";
            }
            else
            {
                strSQL = "select *, LastName + ', ' + FirstName as LastFirstName " + "from users where ','+roles+',' like '%," + strRole + ",%'";
            }
            DataTable dtUsers = oUsers.GetDataTable(strSQL);

            // Add additional Aspnet fields
            AddMembershipColumns(dtUsers);
            dtUsers.PrimaryKey = new DataColumn[] { dtUsers.Columns["Aspnet_UserId"] };
            //dtUsers.PrimaryKey = New DataColumn() {dtUsers.Columns("UserId")}

            bool boolIncludeUser = true;
            DataRow dr = null;
            MembershipUserCollection mu = Membership.GetAllUsers();
            foreach (MembershipUser u in mu)
            {
                if (string.IsNullOrEmpty(strRole))
                {
                    boolIncludeUser = true;
                }
                else
                {
                    boolIncludeUser = Roles.IsUserInRole(u.UserName, strRole);
                }
                if (boolIncludeUser)
                {
                    dr = dtUsers.Rows.Find(u.ProviderUserKey.ToString());
                    if ((dr != null))
                    {
                        dr["IsOnline"] = u.IsOnline;
                        dr["PasswordQuestion"] = u.PasswordQuestion;
                        dr["IsLockedOut"] = u.IsLockedOut;
                        dr["IsApproved"] = u.IsApproved;
                        dr["CreationDate"] = u.CreationDate;
                        dr["LastLoginDate"] = u.LastLoginDate;
                        if (!u.IsLockedOut)
                        {
                            dr["Password"] = u.GetPassword();
                        }
                    }
                }
            }

            dtUsers.PrimaryKey = new DataColumn[] { dtUsers.Columns["UserId"] };
            return dtUsers;
        }

        /// <summary>
        /// Gets an HTML formatted error string.
        /// </summary>
        /// <param name="strMessage">
        /// The message to be formatted with HTML.
        /// </param>
        /// <remarks>
        /// This method wraps the provided <paramref name="strMessage"/> with
        /// &lt;div id='Error'&gt;&lt;/div&gt;
        /// </remarks>
        /// <returns>
        /// An HTML formatted error string.
        /// </returns>
        public static string ErrorDivWrapper(string strMessage)
        {
            StringBuilder sbReturn = new StringBuilder();
            if (!string.IsNullOrEmpty(strMessage))
            {
                sbReturn.Append("<div id='Error' class='alert alert-danger'>" + strMessage + "</div>");
            }
            return sbReturn.ToString();
        }

        /// <summary>
        /// Gets an HTML formatted information string.
        /// </summary>
        /// <param name="strMessage">
        /// The message to be formatted with HTML.
        /// </param>
        /// <remarks>
        /// This method wraps the provided <paramref name="strMessage"/> with
        /// &lt;div id='Info'&gt;&lt;/div&gt;
        /// </remarks>
        /// <returns>
        /// An HTML formatted information string.
        /// </returns>
        public static string InfoDivWrapper(string strMessage)
        {
            StringBuilder sbReturn = new StringBuilder();
            if (!string.IsNullOrEmpty(strMessage))
            {
                sbReturn.Append("<div id='Info' class='alert alert-info'>" + strMessage + "</div>");
            }
            return sbReturn.ToString();
        }

        /// <summary>
        /// Determines if a given <c>object</c> is contained within an
        /// array of <c>object</c>s.
        /// </summary>
        /// <param name="toExpression">The object to be located.</param>
        /// <param name="toItems">An array of objects to be searched.</param>
        /// <returns>
        /// A boolean value indicating if the provided <paramref name="toExpression"/>
        /// exists in the provided <paramref name="toItems"/> array.
        /// </returns>
		public static bool InList(object toExpression, params object[] toItems)
        {
            return Array.IndexOf(toItems, toExpression) > -1;
        }

        public static bool IsPictureFile(string strFilename)
        {
            string strImageExtensions = "gif,jpg,tif,png,bmp,";
            FileInfo fi = new FileInfo(strFilename);
            bool bIsPicture = strImageExtensions.IndexOf(fi.Extension.ToLower().Substring(1)) >= 0;
            return bIsPicture;
        }

        /// <summary>
        /// Parses just the FileName from the provided URL.
        /// </summary>
        /// <param name="strFileName">
        /// A file system path or URL.
        /// </param>
        /// <returns>
        /// The FileName from the provided URL or file system path.
        /// </returns>
		public static string JustFName(string strFileName)
        {
            //Return the filename of a string
            strFileName = strFileName.Substring(strFileName.LastIndexOf("/") + 1).ToLower();
            if (strFileName.Contains("?"))
            {
                strFileName = strFileName.Substring(0, strFileName.IndexOf("?"));
            }
            return strFileName;
        }

        /// <summary>
        /// Strips any non-numeric characters from a string and returns the results
        /// </summary>
        /// <param name="text">The text string from which non-numeric characters are stripped.</param>
        public static string JustNumbers(string text)
        {
            return new string(text.Where(c => char.IsDigit(c)).ToArray());
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the Lookup table for
        /// the provided LookupType.  The Lookup.ItemValue field is bound to DataValueField
        /// and Lookup.ItemDescription is bound to DataTextField.  An additional item is added
        /// as the first item in the dropdown with a value of " " and text of " - Select -".
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="LookupType">The Lookup Type used to populate the <c>DropDownList</c>.</param>
		public static void LoadLookupList(DropDownList DropdownRef, string LookupType)
        {
            LoadLookupList(DropdownRef, LookupType, " - Select -", " ", "itemdescription");
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the Lookup table for
        /// the provided LookupType.  The Lookup.ItemValue field is bound to DataValueField
        /// and Lookup.ItemDescription is bound to DataTextField.  An additional item is added
        /// as the first item in the dropdown using the provided FirstItemDesc and FirstItemValue.
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="LookupType">The Lookup Type used to populate the <c>DropDownList</c>.</param>
        /// <param name="FirstItemDesc">The description of the additional item that is added to the list.</param>
        /// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        public static void LoadLookupList(
            DropDownList DropdownRef, string LookupType, string FirstItemDesc, string FirstItemValue)
        {
            LoadLookupList(DropdownRef, LookupType, FirstItemDesc, FirstItemValue, "itemdescription"); // "itemvalue"
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the Lookup table for
        /// the provided LookupType.  The Lookup.ItemValue field is bound to DataValueField
        /// and the column name passed with <paramref name="DescField"/> is bound to DataTextField.
        /// An additional item is added as the first item in the dropdown using the provided
        /// FirstItemDesc and FirstItemValue.
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="LookupType">The Lookup Type used to populate the <c>DropDownList</c>.</param>
        /// <param name="FirstItemDesc">The description of the additional item that is added to the list.</param>
        /// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        /// <param name="DescField">The column name of the column to be bound to the DataTextField property.</param>
        public static void LoadLookupList(
            DropDownList DropdownRef, string LookupType, string FirstItemDesc,
            string FirstItemValue, string DescField)
        {
            LoadLookupList(DropdownRef, LookupType, FirstItemDesc, FirstItemValue, DescField, "itemvalue", "");
        }

        public static void LoadLookupList(
            DropDownList DropdownRef, string LookupType, bool activeOnly)
        {
            LoadLookupList(DropdownRef, LookupType, activeOnly, null);
        }

        public static void LoadLookupList(
            DropDownList DropdownRef, string LookupType, bool activeOnly, string includeInactiveValue)
        {
            string filter = null;
            if (activeOnly && !string.IsNullOrEmpty(includeInactiveValue))
            {
                filter = "(Active = 1 OR" +
                    " itemvalue = '" + includeInactiveValue.Replace("'", "''") + "')";
            }
            else if (activeOnly)
            {
                filter = "Active = 1";
            }

            LoadLookupList(DropdownRef, LookupType,
                " - Select - ", " ", "itemdescription", "itemvalue", filter);
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the Lookup table for
        /// the provided LookupType and filterd by the provided FilterKey value.  The column name provided
        /// with <paramref name="CodeField"/> is bound to DataValueField and the column name passed with
        /// <paramref name="DescField"/> is bound to DataTextField.  An additional item is added as the first
        /// item in the dropdown using the provided FirstItemDesc and FirstItemValue.
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="LookupType">The Lookup Type used to populate the <c>DropDownList</c>.</param>
        /// <param name="FirstItemDesc">The description of the additional item that is added to the list.</param>
        /// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        /// <param name="DescField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="CodeField">The column name of the column to be bound to the DataValueField property.</param>
        /// <param name="FilterKey">A value used to filter the Lookup items.</param>
        public static void LoadLookupList(
            DropDownList DropdownRef, string LookupType, string FirstItemDesc,
            string FirstItemValue, string DescField, string CodeField, string FilterKey)
        {
            string WhereClause = "where type='" + LookupType + "' and active=1";
            if (!string.IsNullOrEmpty(FilterKey))
            {
                WhereClause += " and " + FilterKey;
            }

            Business.BusinessObject oData = new Business.BusinessObject();
            string CommandText = "select " + DescField + " as descrip, "
                + CodeField + " as code, itemorder from lookup "
                + WhereClause + " group by " + DescField + "," + CodeField + ",itemorder "
                + " order by itemorder, descrip";
            DataTable dt = oData.GetDataTable(CommandText);

            //HttpContext.Current.Trace.Warn("CommandText= " + CommandText);
            //HttpContext.Current.Trace.Warn("SelectedValue='" + DropdownRef.SelectedValue + "'");
            //HttpContext.Current.Trace.Warn("SelectedValue Length=" + DropdownRef.SelectedValue.Length);

            if (FirstItemValue == "") FirstItemValue = " ";

            //HttpContext.Current.Trace.Warn("DropdownRef.ID='" + DropdownRef.ID.ToString() + "'");
            //HttpContext.Current.Trace.Warn("DropdownRef.SelectedValue='" + DropdownRef.SelectedValue + "'");
            //HttpContext.Current.Trace.Warn("FirstItemValue='" + FirstItemValue + "'");
            //HttpContext.Current.Trace.Warn("FirstItemDesc='" + FirstItemDesc + "'");

            if (dt.Rows.Count > 0)
            {
                DropdownRef.DataSource = dt;
                DropdownRef.DataTextField = "descrip";
                DropdownRef.DataValueField = "code";
                try
                {
                    DropdownRef.DataBind();
                }
                catch { }
            }

            // Add blank first item.
            ListItem li = new ListItem();
            li.Value = FirstItemValue;
            li.Text = FirstItemDesc;
            DropdownRef.Items.Insert(0, li);
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the Lookup table for
        /// the provided LookupType and filterd by the provided FilterKey value.  The column name provided
        /// with <paramref name="CodeField"/> is bound to DataValueField and the column name passed with
        /// <paramref name="DescField"/> is bound to DataTextField.  An additional item is added as the first
        /// item in the dropdown using the provided FirstItemDesc and FirstItemValue.
        /// </summary>
        /// <param name="ListBoxRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="LookupType">The Lookup Type used to populate the <c>DropDownList</c>.</param>
        /// <param name="FirstItemDesc">The description of the additional item that is added to the list.</param>
        /// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        /// <param name="DescField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="CodeField">The column name of the column to be bound to the DataValueField property.</param>
        /// <param name="FilterKey">A value used to filter the Lookup items.</param>

        // Short version with only the lookup type specified
        public static void LoadLookupList(ListBox ListBoxRef, string LookupType)
        {
            LoadLookupList(ListBoxRef, LookupType, null, null, "Itemdescription", "Itemvalue", null);
        }

        public static void LoadLookupList(
            ListBox ListBoxRef, string LookupType, string FirstItemDesc,
            string FirstItemValue, string DescField, string CodeField, string FilterKey)
        {
            string WhereClause = "where type='" + LookupType + "' and active=1";
            if (!string.IsNullOrEmpty(FilterKey))
            {
                WhereClause += " and " + FilterKey;
            }

            Business.BusinessObject oData = new Business.BusinessObject();
            string CommandText = "select " + DescField + " as descrip, "
                + CodeField + " as code, itemorder from lookup "
                + WhereClause + " group by " + DescField + "," + CodeField + ",itemorder "
                + " order by itemorder, descrip";
            DataTable dt = oData.GetDataTable(CommandText);
            //Utilities.LogMessage("CommandText= " + CommandText);
            //Utilities.LogMessage("Row Count= " + dt.Rows.Count.ToString());

            if (dt.Rows.Count > 0)
            {
                ListBoxRef.DataSource = dt;
                ListBoxRef.DataTextField = "descrip";
                ListBoxRef.DataValueField = "code";
                try
                {
                    ListBoxRef.DataBind();
                }
                catch { }
            }

            // Add first item, when supplied.
            if (!string.IsNullOrEmpty(FirstItemValue) && !string.IsNullOrEmpty(FirstItemDesc))
            {
                if (FirstItemValue == "") FirstItemValue = " ";
                ListItem li = new ListItem();
                li.Value = FirstItemValue;
                li.Text = FirstItemDesc;
                ListBoxRef.Items.Insert(0, li);
            }
        }
        public static void LoadLookupList(
            RadioButtonList rbl, string LookupType, string DescField, string CodeField, string FilterKey = null)
        {
            string WhereClause = "where type='" + LookupType + "'";
            if (!string.IsNullOrEmpty(FilterKey))
            {
                WhereClause += " and " + FilterKey;
            }

            Business.BusinessObject oData = new Business.BusinessObject();
            string CommandText = "select " + DescField + " as descrip, "
                + CodeField + " as code, itemorder from lookup "
                + WhereClause + " group by " + DescField + "," + CodeField + ",itemorder "
                + " order by itemorder, descrip";
            DataTable dt = oData.GetDataTable(CommandText);

            if (dt.Rows.Count > 0)
            {
                rbl.DataSource = dt;
                rbl.DataTextField = "descrip";
                rbl.DataValueField = "code";
                try
                {
                    rbl.DataBind();
                }
                catch
                {
                }
            }
        }

        /*
         * Load HtmlSelect objects from Lookup table 
         */
        public static void LoadLookupList(HtmlSelect select, string type)
        {
            LoadLookupList(select, type, "Itemdescription", "Itemvalue", null);
        }

        public static void LoadLookupList(
            HtmlSelect select, string type, string descField)
        {
            LoadLookupList(select, type, descField, "Itemvalue", null);
        }

        public static void LoadLookupList(
            HtmlSelect select, string type, bool activeOnly)
        {
            LoadLookupList(select, type, activeOnly, null);
        }

        public static void LoadLookupList(
            HtmlSelect select, string type, bool activeOnly, string includeInactiveValue)
        {
            string filter = null;
            if (activeOnly && !string.IsNullOrEmpty(includeInactiveValue))
            {
                filter = "(Active = 1 OR" +
                    " itemvalue = '" + includeInactiveValue.Replace("'", "''") + "')";
            }
            else if (activeOnly)
            {
                filter = "Active = 1";
            }

            LoadLookupList(select, type, "Itemdescription", "Itemvalue", filter);
        }

        public static void LoadLookupList(
            HtmlSelect select, string type, string descField, string codeField, string filterKey)
        {
            string whereClause = " WHERE type = '" + type.Replace("''", "'") + "'";
            if (!string.IsNullOrEmpty(filterKey))
            {
                whereClause += " AND " + filterKey;
            }

            Business.BusinessObject oData = new Business.BusinessObject();
            DataTable dt = oData.GetDataTable(
                "SELECT " + descField + " AS text, " + codeField + " AS code, itemorder" +
                " FROM lookup" +
                whereClause +
                " GROUP BY " + descField + ", " + codeField + ", itemorder" +
                " ORDER BY itemorder, text"
            );

            if (dt.Rows.Count > 0)
            {
                select.DataSource = dt;
                select.DataTextField = "text";
                select.DataValueField = "code";
                try
                {
                    select.DataBind();
                }
                catch
                { }
            }
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the table specified with the
        /// <paramref name="Table"/> parameter.  The column name provided with <paramref name="CodeField"/> is
        /// bound to DataValueField and the column name passed with <paramref name="DescField"/> is bound to
        /// DataTextField.  An additional item is added as the first item in the dropdown using " " as the
        /// value and " - Select -" as the Description.
        /// </summary>
        /// <remarks>
        /// The Description/Code values are guaranteed to be unique and are ordered by the DescField.
        /// </remarks>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="Table">The name of the table used to populate the DropDownList.</param>
        /// <param name="DescField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="CodeField">The column name of the column to be bound to the DataValueField property.</param>
        public static void LoadTableList(DropDownList DropdownRef, string Table, string DescField, string CodeField)
        {
            LoadTableList(DropdownRef, Table, DescField, CodeField, "", " - Select -", " ");
        }

        ///// <summary>
        ///// Populates the provided <c>DropDownList</c> with data from the table specified with the
        ///// <paramref name="Table"/> parameter.  The column name provided with <paramref name="CodeField"/> is
        ///// bound to DataValueField and the column name passed with <paramref name="DescField"/> is bound to
        ///// DataTextField.  An additional item is added as the first item in the dropdown using " " as the
        ///// value and " - Select -" as the Description.
        ///// </summary>
        ///// <remarks>
        ///// The Description/Code values are guaranteed to be unique and are ordered by the DescField.
        ///// </remarks>
        ///// <param name="ListBoxRef">The <c>DropDownList</c> to be populated.</param>
        ///// <param name="Table">The name of the table used to populate the DropDownList.</param>
        ///// <param name="DescField">The column name of the column to be bound to the DataTextField property.</param>
        ///// <param name="CodeField">The column name of the column to be bound to the DataValueField property.</param>
        //public static void LoadTableList(ListBox ListBoxRef, string Table, string DescField, string CodeField)
        //{
        //    LoadTableList(ListBoxRef, Table, DescField, CodeField, "", " - Select -", " ");
        //}
        ///// <summary>
        ///// Populates the provided <c>DropDownList</c> with data from the table specified with the
        ///// <paramref name="Table"/> parameter and filterd with the provided WhereClause.  The column name provided
        ///// with <paramref name="CodeField"/> is bound to DataValueField and the column name passed with
        ///// <paramref name="DescField"/> is bound to DataTextField.  An additional item is added as the first item of
        ///// the dropdown using the provided FirstItemDesc and FirstItemValue.
        ///// </summary>
        ///// <remarks>
        ///// The Description/Code values are guaranteed to be unique and are ordered by the DescField.
        ///// </remarks>
        ///// <param name="ListboxRef">The <c>DropDownList</c> to be populated.</param>
        ///// <param name="Table">The name of the table used to populate the DropDownList.</param>
        ///// <param name="DescField">The column name of the column to be bound to the DataTextField property.</param>
        ///// <param name="CodeField">The column name of the column to be bound to the DataValueField property.</param>
        ///// <param name="WhereClause">
        ///// A standard SQL where statement used to filter the items.  WHERE will automatically be prepended to the
        ///// provided statement and should not be included.
        ///// </param>
        ///// <param name="FirstItemDesc">The description of the additional item that is added to the list.</param>
        ///// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        //public static void LoadTableList(ListBox ListboxRef, string Table, string DescField, string CodeField,
        //    string WhereClause, string FirstItemDesc, string FirstItemValue)
        //{
        //    Business.BusinessObject oData = new Business.BusinessObject();
        //    if (!string.IsNullOrEmpty(WhereClause.Trim()))
        //    {
        //        WhereClause = "where " + WhereClause;
        //    }
        //    string CommandText = "select " + DescField + " as descrip, " + CodeField + " as code, 1 as sortorder from " + Table + " " + WhereClause + " group by " + DescField + "," + CodeField + " order by sortorder, descrip";
        //    DataTable dt = oData.GetDataTable(CommandText);

        //    if (dt.Rows.Count > 0)
        //    {
        //        ListboxRef.DataSource = dt;
        //        ListboxRef.DataTextField = "descrip";
        //        ListboxRef.DataValueField = "code";
        //        try
        //        {
        //            ListboxRef.DataBind();
        //        }
        //        catch { }
        //    }
        //    else
        //    {
        //        ListboxRef.Items.Clear();
        //        ListboxRef.DataSource = null;
        //        ListboxRef.DataBind();
        //    }

        //    // Add blank first item.
        //    ListItem li = new ListItem();
        //    li.Value = FirstItemValue;
        //    li.Text = FirstItemDesc;
        //    ListboxRef.Items.Insert(0, li);
        //}
        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the table specified with the
        /// <paramref name="Table"/> parameter and filterd with the provided WhereClause.  The column name provided
        /// with <paramref name="CodeField"/> is bound to DataValueField and the column name passed with
        /// <paramref name="DescField"/> is bound to DataTextField.  An additional item is added as the first item of
        /// the dropdown using the provided FirstItemDesc and FirstItemValue.
        /// </summary>
        /// <remarks>
        /// The Description/Code values are guaranteed to be unique and are ordered by the DescField.
        /// </remarks>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="Table">The name of the table used to populate the DropDownList.</param>
        /// <param name="DescField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="CodeField">The column name of the column to be bound to the DataValueField property.</param>
        /// <param name="WhereClause">
        /// A standard SQL where statement used to filter the items.  WHERE will automatically be prepended to the
        /// provided statement and should not be included.
        /// </param>
        /// <param name="FirstItemDesc">The description of the additional item that is added to the list.</param>
        /// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        public static void LoadTableList(DropDownList DropdownRef, string Table, string DescField, string CodeField,
            string WhereClause, string FirstItemDesc, string FirstItemValue)
        {
            Business.BusinessObject oData = new Business.BusinessObject();
            if (!string.IsNullOrEmpty(WhereClause.Trim()))
            {
                WhereClause = "where " + WhereClause;
            }
            string CommandText = "select " + DescField + " as descrip, " + CodeField + " as code, 1 as sortorder from " + Table + " " + WhereClause + " group by " + DescField + "," + CodeField + " order by sortorder, descrip";
            DataTable dt = oData.GetDataTable(CommandText);

            if (dt.Rows.Count > 0)
            {
                DropdownRef.DataSource = dt;
                DropdownRef.DataTextField = "descrip";
                DropdownRef.DataValueField = "code";
                try
                {
                    DropdownRef.DataBind();
                }
                catch { }
            }
            else
            {
                DropdownRef.Items.Clear();
                DropdownRef.DataSource = null;
                DropdownRef.DataBind();
            }

            // Add blank first item.
            ListItem li = new ListItem();
            li.Value = FirstItemValue;
            li.Text = FirstItemDesc;
            DropdownRef.Items.Insert(0, li);
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the <c>DataTable</c> specified with the
        /// <paramref name="dt"/> parameter.  The column name provided with <paramref name="ItemValueField"/> is bound
        /// to DataValueField and the column name provided with <paramref name="ItemTextField"/> is bound to DataTextField.
        /// An additional item is added as the first item of the dropdown using " - Select -" as the Text and " " as the
        /// value.
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="dt">The <c>DataTable</c> used to populate the DropDownList.</param>
        /// <param name="ItemTextField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="ItemValueField">The column name of the column to be bound to the DataValueField property.</param>
        public static void LoadTableList(DropDownList DropdownRef, DataTable dt, string ItemTextField, string ItemValueField)
        {
            LoadTableList(DropdownRef, dt, ItemTextField, ItemValueField, " - Select -", " ");
        }
        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the <c>DataTable</c> specified with the
        /// <paramref name="dt"/> parameter.  The column name provided with <paramref name="ItemValueField"/> is bound
        /// to DataValueField and the column name provided with <paramref name="ItemTextField"/> is bound to DataTextField.
        /// An additional item is added as the first item of the dropdown using " - Select -" as the Text and " " as the
        /// value.
        /// </summary>
        /// <param name="ListboxRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="dt">The <c>DataTable</c> used to populate the DropDownList.</param>
        /// <param name="ItemTextField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="ItemValueField">The column name of the column to be bound to the DataValueField property.</param>
        public static void LoadTableList(ListBox ListboxRef, DataTable dt, string ItemTextField, string ItemValueField)
        {
            LoadTableList(ListboxRef, dt, ItemTextField, ItemValueField, " - Select -", " ");
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the <c>DataTable</c> specified with the
        /// <paramref name="dt"/> parameter.  The column name provided with <paramref name="ItemValueField"/> is bound
        /// to DataValueField and the column name provided with <paramref name="ItemTextField"/> is bound to DataTextField.
        /// Additional first dummy item is not added
        /// </summary>
        /// <param name="ListboxRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="dt">The <c>DataTable</c> used to populate the DropDownList.</param>
        /// <param name="ItemTextField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="ItemValueField">The column name of the column to be bound to the DataValueField property.</param>
        public static void LoadTableListWithoutSelect(ListBox ListboxRef, DataTable dt, string ItemTextField, string ItemValueField)
        {
            if (dt.Rows.Count > 0)
            {
                ListboxRef.DataSource = dt;
                ListboxRef.DataTextField = ItemTextField;
                ListboxRef.DataValueField = ItemValueField;
                ListboxRef.DataBind();
            }
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the <c>DataTable</c> specified with the
        /// <paramref name="dt"/> parameter.  The column name provided with <paramref name="ItemValueField"/> is bound
        /// to DataValueField and the column name provided with <paramref name="ItemTextField"/> is bound to DataTextField.
        /// An additional item is added as the first item of the dropdown using the provided FirstItemText and FirstItemValue.
        /// </summary>
        /// <param name="ListboxRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="dt">The <c>DataTable</c> used to populate the DropDownList.</param>
        /// <param name="ItemTextField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="ItemValueField">The column name of the column to be bound to the DataValueField property.</param>
        /// <param name="FirstItemText">The description of the additional item that is added to the list.</param>
        /// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        public static void LoadTableList(
            ListBox ListboxRef, DataTable dt, string ItemTextField, string ItemValueField, string FirstItemText, string FirstItemValue)
        {
            if (dt.Rows.Count > 0)
            {
                ListboxRef.DataSource = dt;
                ListboxRef.DataTextField = ItemTextField;
                ListboxRef.DataValueField = ItemValueField;
                ListboxRef.DataBind();
            }

            // Add blank first item.
            ListItem li = new ListItem();
            li.Text = FirstItemText;
            li.Value = FirstItemValue;
            ListboxRef.Items.Insert(0, li);
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the <c>DataTable</c> specified with the
        /// <paramref name="dt"/> parameter.  The column name provided with <paramref name="ItemValueField"/> is bound
        /// to DataValueField and the column name provided with <paramref name="ItemTextField"/> is bound to DataTextField.
        /// An additional item is added as the first item of the dropdown using the provided FirstItemText and FirstItemValue.
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="dt">The <c>DataTable</c> used to populate the DropDownList.</param>
        /// <param name="ItemTextField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="ItemValueField">The column name of the column to be bound to the DataValueField property.</param>
        /// <param name="FirstItemText">The description of the additional item that is added to the list.</param>
        /// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        public static void LoadTableList(
            DropDownList DropdownRef, DataTable dt, string ItemTextField, string ItemValueField, string FirstItemText, string FirstItemValue)
        {
            if (dt.Rows.Count > 0)
            {
                DropdownRef.DataSource = dt;
                DropdownRef.DataTextField = ItemTextField;
                DropdownRef.DataValueField = ItemValueField;
                DropdownRef.DataBind();
            }

            // Add blank first item.
            ListItem li = new ListItem();
            li.Text = FirstItemText;
            li.Value = FirstItemValue;
            DropdownRef.Items.Insert(0, li);
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the <c>DataView</c> specified with the
        /// <paramref name="dv"/> parameter.  The column name provided with <paramref name="ItemValueField"/> is bound
        /// to DataValueField and the column name provided with <paramref name="ItemTextField"/> is bound to DataTextField.
        /// An additional item is added as the first item of the dropdown using " - Select -" as the Text and " " as the
        /// value.
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="dv">The <c>DataView</c> used to populate the DropDownList.</param>
        /// <param name="ItemTextField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="ItemValueField">The column name of the column to be bound to the DataValueField property.</param>
        public static void LoadTableList(DropDownList DropdownRef, DataView dv, string ItemTextField, string ItemValueField)
        {
            LoadTableList(DropdownRef, dv, ItemTextField, ItemValueField, " - Select -", " ");
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the <c>DataView</c> specified with the
        /// <paramref name="dv"/> parameter.  The column name provided with <paramref name="ItemValueField"/> is bound
        /// to DataValueField and the column name provided with <paramref name="ItemTextField"/> is bound to DataTextField.
        /// An additional item is added as the first item of the dropdown using the provided FirstItemText and FirstItemValue.
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="dv">The <c>DataView</c> used to populate the DropDownList.</param>
        /// <param name="ItemTextField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="ItemValueField">The column name of the column to be bound to the DataValueField property.</param>
        /// <param name="FirstItemText">The description of the additional item that is added to the list.</param>
        /// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        public static void LoadTableList(
            DropDownList DropdownRef, DataView dv, string ItemTextField, string ItemValueField, string FirstItemText, string FirstItemValue)
        {
            if (dv.Count > 0)
            {
                DropdownRef.DataSource = dv;
                DropdownRef.DataTextField = ItemTextField;
                DropdownRef.DataValueField = ItemValueField;
                DropdownRef.DataBind();
            }

            // Add blank first item.
            ListItem li = new ListItem();
            li.Text = FirstItemText;
            li.Value = FirstItemValue;
            DropdownRef.Items.Insert(0, li);
        }

        /// <summary>
        /// Populates the provided <c>ListBox</c> with data from the table specified with the
        /// <paramref name="Table"/> parameter.  The column name provided with <paramref name="CodeField"/>
        /// is bound to DataValueField and the column name provided with <paramref name="DescField"/>
        /// is bound to DataTextField.
        /// </summary>
        /// <remarks>
        /// The items are ordered by the DescField.
        /// </remarks>
        /// <param name="ListboxRef">The <c>ListBox</c> to be populated.</param>
        /// <param name="Table">The name of the table used to populate the ListBox.</param>
        /// <param name="DescField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="CodeField">The column name of the column to be bound to the DataValueField property.</param>
        public static void LoadTableList(ListBox ListboxRef, string Table, string DescField, string CodeField)
        {
            LoadTableList(ListboxRef, Table, DescField, CodeField, "", "", "");
        }

        /// <summary>
        /// Populates the provided <c>ListBox</c> with data from the table specified with the
        /// <paramref name="Table"/> parameter and if provided, filterd with WhereClause.  The column name provided
        /// with <paramref name="CodeField"/> is bound to DataValueField and the column name provided with
        /// <paramref name="DescField"/> is bound to DataTextField.  If not null, an additional item is added as the
        /// first item of the list using the provided FirstItemDesc and FirstItemValue.
        /// </summary>
        /// <remarks>
        /// The items are ordered by the DescField.
        /// </remarks>
        /// <param name="ListboxRef">The <c>ListBox</c> to be populated.</param>
        /// <param name="Table">The name of the table used to populate the ListBox.</param>
        /// <param name="DescField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="CodeField">The column name of the column to be bound to the DataValueField property.</param>
        /// <param name="WhereClause">
        /// A standard SQL where statement used to filter the items.  WHERE will automatically be prepended to the
        /// provided statement and should not be included.
        /// </param>
        /// <param name="FirstItemDesc">The description of the additional item that is added to the list.</param>
        /// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        public static void LoadTableList(
            ListBox ListboxRef, string Table, string DescField, string CodeField,
            string WhereClause, string FirstItemDesc, string FirstItemValue)
        {
            Business.BusinessObject oData = new Business.BusinessObject();
            if (!string.IsNullOrEmpty(WhereClause.Trim()))
            {
                WhereClause = "where " + WhereClause;
            }
            string CommandText = "select " + DescField + " as descrip, " + CodeField + " as code from " + Table + " " + WhereClause + " order by descrip";
            DataTable dt = oData.GetDataTable(CommandText);

            if (dt.Rows.Count > 0)
            {
                ListboxRef.DataSource = dt;
                ListboxRef.DataTextField = "descrip";
                ListboxRef.DataValueField = "code";
                ListboxRef.DataBind();
            }

            if (!string.IsNullOrEmpty(FirstItemDesc) || !string.IsNullOrEmpty(FirstItemValue))
            {
                // Add blank first item.
                ListItem li = new ListItem();
                li.Value = FirstItemValue;
                li.Text = FirstItemDesc;
                ListboxRef.Items.Insert(0, li);
            }
        }

        /// <summary>
        /// Populates the provided <c>DropDownList</c> with data from the <c>DataTable</c> specified with the
        /// <paramref name="dt"/> parameter.  The column name provided with <paramref name="CodeField"/> is bound
        /// to DataValueField and the column name provided with <paramref name="DescField"/> is bound to DataTextField.
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="dt">The <c>DataTable</c> used to populate the DropDownList.</param>
        /// <param name="DescField">The column name of the column to be bound to the DataTextField property.</param>
        /// <param name="CodeField">The column name of the column to be bound to the DataValueField property.</param>
        public static void LoadDatatableList(DropDownList DropdownRef, DataTable dt, string DescField, string CodeField)
        {
            if (dt.Rows.Count > 0)
            {
                DropdownRef.DataSource = dt;
                DropdownRef.DataTextField = DescField;
                DropdownRef.DataValueField = CodeField;
                DropdownRef.DataBind();
            }
        }

        /// <summary>
        /// Populate a <c>DropDownList</c> with 1 - 10.
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        public static void LoadNumberDropdown(DropDownList DropdownRef)
        {
            LoadNumberDropdown(DropdownRef, 1, 10, "", "", 1);
        }

        /// <summary>
        /// Populate a <c>DropDownList</c> with incremental numbers.
        /// </summary>
        /// <param name="DropdownRef">The <c>DropDownList</c> to be populated.</param>
        /// <param name="StartNumber">The starting number.</param>
        /// <param name="EndNumber">The ending number.</param>
        /// <param name="FirstItemDesc">The description of the additional item that is added to the list.</param>
        /// <param name="FirstItemValue">The value of the additional item that is added to the list.</param>
        /// <param name="Increment">The increment.</param>
        public static void LoadNumberDropdown(
            DropDownList DropdownRef, int StartNumber, int EndNumber, string FirstItemDesc, string FirstItemValue, int Increment)
        {
            ListItem li = new ListItem();
            for (int xx = StartNumber; xx <= EndNumber; xx += Increment)
            {
                li.Value = xx.ToString();
                li.Text = xx.ToString();
                DropdownRef.Items.Add(xx.ToString());
            }

            // Add blank/alternate first item.
            li.Value = FirstItemValue;
            li.Text = FirstItemDesc;
            DropdownRef.Items.Insert(0, li);
        }

        /// <summary>
        /// Returns a random number between 1 and 999999999 using a seed value
        /// derived from the current time.
        /// </summary>
        /// <returns>A random number between 1 and 999999999.</returns>
        public static int RandomNumber()
        {
            return RandomNumber(999999999);
        }

        /// <summary>
        /// Returns a random number between 1 and <paramref name="MaxNumber"/>
        /// using a seed value derived from the current time.
        /// </summary>
        /// <param name="MaxNumber">The maximum number that can be returned.</param>
        /// <returns>A random number between 1 and <paramref name="MaxNumber"/>.</returns>
        public static int RandomNumber(int MaxNumber)
        {
            return RandomNumber(MaxNumber, 1, 0);
        }

        /// <summary>
        /// Returns a random number between <paramref name="MinNumber"/> and <paramref name="MaxNumber"/>
        /// using a seed value provided by <paramref name="RandomSeed"/>.
        /// </summary>
        /// <param name="MaxNumber">The maximum number that can be returned.</param>
        /// <param name="MinNumber">The minimum number that can be returned.</param>
        /// <param name="RandomSeed">The random seed number to use.</param>
        /// <returns>A random number between <paramref name="MinNumber"/> and <paramref name="MaxNumber"/>.</returns>
        public static int RandomNumber(int MaxNumber, int MinNumber, int RandomSeed)
        {
            //initialize random number generator
            if (RandomSeed == 0)
            {
                RandomSeed = System.DateTime.Now.Millisecond;
            }
            Random r = new Random(RandomSeed);

            //if passed incorrect arguments, swap them
            //can also throw exception or return 0

            if (MinNumber > MaxNumber)
            {
                int t = MinNumber;
                MinNumber = MaxNumber;
                MaxNumber = t;
            }

            return r.Next(MinNumber, MaxNumber);

        }

        public static string RandomString(int minLength, int maxLength)
        {
            return RandomString(minLength, maxLength, (int)DateTime.Today.Ticks, DEFAULTPWCHARS);
        }

        public static string RandomString(int minLength, int maxLength, int randomSeed, string allowedChars)
        {
            char[] chars = new char[maxLength];
            int setLength = allowedChars.Length;

            Random r = new Random(randomSeed);
            int length = r.Next(minLength, maxLength + 1);
            for (int i = 0; i < length; ++i)
            {
                chars[i] = allowedChars[r.Next(setLength)];
            }

            return new string(chars, 0, length);
        }


        /// <summary>
        /// Checks to see if records exist for <paramref name="tablename" /> 
        /// where <paramref name="keyfield" /> is equal to <paramref name="keyvalue" />.
        /// </summary>
        /// <param name="tablename">The SQL Server table to be queried.</param>
        /// <param name="keyfield">The table field name to be checked.</param>
        /// <param name="keyvalue">The key value to be searched for.</param>
        /// <returns>
        /// A boolean value indicating if any results were found.
        /// </returns>
        public static bool RecordsOnFile(string tablename, string keyfield, object keyvalue)
        {
            Business.BusinessObject oData = new Business.BusinessObject();
            int tally;
            string keystring;
            if (keyvalue.GetType() == typeof(int) || keyvalue.GetType() == typeof(decimal))
            {
                keystring = keyvalue.ToString();
            }
            else
            {
                keystring = "'" + keyvalue.ToString() + "'";
            }
            tally = oData.ExecuteScalar("SELECT COUNT(*) FROM " + tablename +
                " WHERE " + keyfield + " = " + keystring);
            return (tally > 0);
        }

        /// <summary>
        /// Builds a new string where the occurrences of <paramref name="strSearchText"/>
        /// within <paramref name="strText"/> have been replaced with <paramref name="strReplacement"/>.
        /// </summary>
        /// <param name="strText">The string to be searched and replaced.</param>
        /// <param name="strSearchText">The string to be replaced.</param>
        /// <param name="strReplacement">The string that will replace the searched text.</param>
        /// <returns>
        /// A new string where the occurrences of <paramref name="strSearchText"/>
        /// within <paramref name="strText"/> have been replaced with <paramref name="strReplacement"/>.
        /// </returns>
        public static string ReplaceText(string strText, string strSearchText, string strReplacement)
        {
            // Replace all occurences of a string occuring within another string.
            string strReplacedText = Regex.Replace(strText, strSearchText, strReplacement, RegexOptions.IgnoreCase);
            return strReplacedText;
        }


        public static string ResetPassword(string strUserName)
        {

            string strNewPassword = "";

            // First ensure that the user's membership info can be retrieved and the PasswordExpired flag in the Users record can be set.
            MembershipUser mu = Membership.GetUser(strUserName);
            if (mu != null & Utilities.SetPasswordExpired(true, strUserName))
            {
                // Once the user's password has been marked as expired. Change it.
                var _with1 = mu;
                if (_with1.IsLockedOut)
                {
                    _with1.UnlockUser();
                }
                string strOldPassword = _with1.GetPassword();

                strNewPassword = System.Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
                _with1.ChangePassword(strOldPassword, strNewPassword);
                Membership.UpdateUser(mu);
            }

            return strNewPassword;
        }

        public static void SetListBoxValues(ListBox listBox, object sessionValue, System.Collections.Generic.List<int> selectedValuesList)
        {
            if (sessionValue is string)
            {
                var strsessionValue = sessionValue as string;
                if (!string.IsNullOrWhiteSpace(strsessionValue))
                {
                    selectedValuesList = ConvertStringToIntList(strsessionValue);
                    listBox.SelectedValue = strsessionValue;
                }
            }
            else
            {
                selectedValuesList = (List<int>)sessionValue;
                if (selectedValuesList != null)
                {
                    foreach (var idx in selectedValuesList)
                    {
                        if (listBox.Items.Count != 0 && listBox.Items.Count >= idx)
                            listBox.Items[idx].Selected = true;
                    }
                }
            }
        }

        public static List<int> ConvertStringToIntList(string data)
        {
            return !string.IsNullOrWhiteSpace(data) ? data.Split(',').Select(int.Parse).ToList() : new List<int>();
        }

        public static bool SetPasswordExpired(bool bExpired, string strUserName = "")
        {
            if (string.IsNullOrEmpty(strUserName))
            {
                strUserName = HttpContext.Current.Session["username"].ToString();
            }
            bool bSuccess = false;
            BusinessObjects.UsersBO oUsers = new BusinessObjects.UsersBO();
            oUsers.GetRecordByID(strUserName, "username");
            if (oUsers.dt.Rows.Count > 0)
            {
                oUsers.Row["passwordexpired"] = bExpired;
                bSuccess = oUsers.SaveDataTable();
            }
            return bSuccess;
        }

        /// <summary>
        /// Returns a new string with all occurrences of " and ' converted to HTML entities.
        /// </summary>
        /// <param name="strInput">A string to be converted.</param>
        /// <returns>
        /// A new string with all occurrences of " and ' converted to HTML entities.
        /// </returns>
        public static string SanitizeText(string strInput)
        {
            return SanitizeText(strInput, false);
        }

        /// <summary>
        /// Returns a new string with all occurrences of " and ' converted to HTML entities.
        /// &lt; and &gt; are also conditionally converted depending on the value of the
        /// <paramref name="boolConvertBrackets"/> parameter.
        /// </summary>
        /// <param name="strInput">A string.</param>
        /// <param name="boolConvertBrackets">Indicates whether &lt; and &gt; should be converted.</param>
        /// <returns>
        /// A new string with all occurrences of " and ' converted to HTML entities.
        /// &lt; and &gt; are also conditionally converted depending on the value of the
        /// <paramref name="boolConvertBrackets"/> parameter.
        /// </returns>
		public static string SanitizeText(string strInput, bool boolConvertBrackets)
        {
            string strOutput = strInput;
            strOutput = strOutput.Replace("\"", "&quot;");
            strOutput = strOutput.Replace("'", "&#39;");
            if (boolConvertBrackets)
            {
                strOutput = strOutput.Replace("<", "&#060;");
                strOutput = strOutput.Replace(">", "&#062;");
            }

            return strOutput;
        }

        /// <summary>
        /// Determines if the provided <paramref name="Email"/> address is a valid email address.
        /// </summary>
        /// <param name="Email">A string to be validated as an email address.</param>
        /// <returns>
        /// true if the provided <paramref name="Email"/> is a valid email address, otherwise False.
        /// </returns>
		public static bool ValidEmailAddress(string Email)
        {
            //"\b[A-Z0-9._%-]+@[A-Z0-9._%-]+\.[A-Z]{2,4}\b"
            //"^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$"
            //"^([a-zA-Z0-9_\-\.']+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)/(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}/[0-9]{1,3})"
            Regex oRegex = new Regex("\\A^\\w[A-Z0-9._%-]+@[A-Z0-9._%-]+\\.[A-Z]{2,4}\\Z",
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
            int nValidEmails = oRegex.Matches(Email).Count;
            return (nValidEmails == 1);
        }

        /// <summary>
        /// Determines if the provided <paramref name="dateString"/> can be parsed as a DateTime.
        /// </summary>
        /// <remarks>
        /// Used to replace usages of VB IsDate function.
        /// </remarks>
        /// <param name="dateString">A string to be validated as a DateTime.</param>
        /// <returns>
        /// true if the provided <paramref name="dateString"/> can be parsed as a DateTime, otherwise false.
        /// </returns>
        public static bool IsDate(string dateString)
        {
            try
            {
                DateTime.Parse(dateString);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool TryParse(string strGuid, ref Guid guid)
        {
            try
            {
                guid = new Guid(strGuid);
                return true;
            }
            catch (Exception e)
            {
                guid = Guid.Empty;
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the provided object is a DBNull.
        /// </summary>
        /// <remarks>
        /// Used to replace usages of VB IsDBNull function.
        /// </remarks>
        /// <param name="o">Any object.</param>
        /// <returns>
        /// true if the provided object is equal to DBNull.Value, otherwise false.
        /// </returns>
        public static bool IsDBNull(object o)
        {
            return o == DBNull.Value;
        }

        public static void ExportToCSV(DataTable table, string filePath, bool includeHeaders)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                ExportToCSV(table, fs, includeHeaders);
            }
        }

        public static void ExportToCSV(DataTable table, Stream stream)
        {
            ExportToCSV(table, stream, true);
        }

        public static void ExportToCSV(DataTable table, Stream stream, bool includeHeaders)
        {
            using (StreamWriter sw = new StreamWriter(stream))
            {
                int colCount = 0;

                if (includeHeaders)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        if (colCount++ > 0) sw.Write(',');
                        sw.Write(PrepareCSVField(col.ColumnName));
                    }

                    sw.Write(sw.NewLine);
                }

                foreach (DataRow row in table.Rows)
                {
                    colCount = 0;
                    foreach (DataColumn col in table.Columns)
                    {
                        if (colCount++ > 0) sw.Write(',');
                        if (!row.IsNull(col))
                        {
                            if (col.DataType == typeof(string))
                            {
                                sw.Write(PrepareCSVField(row[col].ToString()));
                            }
                            //else if (col.DataType == typeof(DateTime))
                            //{
                            //}
                            else
                            {
                                sw.Write(row[col].ToString());
                            }
                        }
                    }

                    sw.Write(sw.NewLine);
                    sw.Flush();
                }
            }
        }

        public static void LogMessage(string strMessage)
        {
            BusinessObjects.LogBO log = new BusinessObjects.LogBO();
            log.AddEntry(strMessage);
        }

        public static string PrepareCSVField(string field)
        {
            bool needsQuotes = false;
            foreach (char character in field)
            {
                if (character == ',' || character == '"' ||
                    character == '\r' || character == '\n')
                {
                    needsQuotes = true;
                    break;
                }
            }

            field = field.Replace("\"", "\"\"");
            return needsQuotes ? "\"" + field + "\"" : field;
        }

        public static String TimeZoneName(DateTime dt, TimeZoneInfo ti)
        {
            String sName = ti.IsDaylightSavingTime(dt)
                ? ti.DaylightName : ti.StandardName;

            String sNewName = "";
            String[] sSplit = sName.Split(new char[] { ' ' });
            foreach (String s in sSplit)
            {
                if (s.Length >= 1) sNewName += s.Substring(0, 1);
            }

            return sNewName;
        }

        public static string GetVCalendar(DateTime startTime, DateTime? endTime, string id, string title, string description,
            string location, string address, string city, string state, string zip, string phone)
        {
            startTime = startTime.ToUniversalTime();
            if (endTime.HasValue)
            {
                endTime = endTime.Value.ToUniversalTime();
            }
            else
            {
                endTime = startTime;
            }

            DateTime addedOn = DateTime.Now;
            DateTime updatedOn = DateTime.Now;

            string strLocation =
                (string.IsNullOrEmpty(location) ? "" : location + "\n") +
                (string.IsNullOrEmpty(address) ? "" : address + "\n") +
                (string.IsNullOrEmpty(city) ? "" : city + ", ") +
                (string.IsNullOrEmpty(state) ? "" : state + "\n") +
                (string.IsNullOrEmpty(phone) ? "" : phone + "\n");


            StringBuilder sbText = new StringBuilder();

            sbText.AppendFormat("BEGIN:VCALENDAR{0}", System.Environment.NewLine);
            sbText.AppendFormat("VERSION:2.0{0}", System.Environment.NewLine);
            sbText.AppendLine("PRODID:-//PMOInABox/Appt//NONSGML v1.0//EN");

            sbText.AppendFormat("METHOD:PUBLISH{0}", System.Environment.NewLine);
            sbText.AppendFormat("BEGIN:VEVENT{0}", System.Environment.NewLine);
            sbText.AppendFormat("UID:{0}{1}", id, System.Environment.NewLine);
            sbText.AppendFormat("CREATED:{0}{1}", addedOn.ToString("yyyyMMdd\\THHmmss\\Z"), System.Environment.NewLine);
            sbText.AppendFormat("LAST-MODIFIED:{0}{1}", updatedOn.ToString("yyyyMMdd\\THHmmss\\Z"), System.Environment.NewLine);
            sbText.AppendFormat("DTSTAMP:{0}{1}", DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z"), System.Environment.NewLine);
            sbText.AppendFormat("DTSTART:{0}{1}", startTime.ToString("yyyyMMdd\\THHmmss\\Z"), System.Environment.NewLine);
            sbText.AppendFormat("DTEND:{0}{1}", endTime.Value.ToString("yyyyMMdd\\THHmmss\\Z"), System.Environment.NewLine);
            if (strLocation.Length > 0)
            {
                sbText.AppendFormat("LOCATION:{0}{1}", strLocation, System.Environment.NewLine);
            }
            sbText.AppendFormat("SUMMARY:{0}{1}", title, System.Environment.NewLine);
            sbText.AppendFormat("PRIORITY:0{0}", System.Environment.NewLine);
            sbText.AppendFormat("SEQUENCE:0{0}", System.Environment.NewLine);
            sbText.AppendFormat("STATUS:CONFIRMED{0}", System.Environment.NewLine);
            sbText.AppendFormat("TRANSP:TRANSPARENT{0}", System.Environment.NewLine);

            sbText.AppendFormat("DESCRIPTION:{0}{1}", Utilities.ReformatHTMLforVcalendar(description), System.Environment.NewLine);
            sbText.AppendFormat("END:VEVENT{0}", System.Environment.NewLine);
            sbText.AppendFormat("END:VCALENDAR{0}", System.Environment.NewLine);

            return sbText.ToString();
        }

        public static string ReformatHTMLforVcalendar(string strText)
        {
            strText = strText.Replace("\t", "");
            strText = strText.Replace("\r", "");
            strText = strText.Replace("\n", "");
            strText = strText.Replace("&quot;", "\"");
            strText = strText.Replace("&#39;", "'");
            strText = strText.Replace("&ldquo;", "\"");
            strText = strText.Replace("&rdquo;", "\"");
            strText = strText.Replace("&nbsp;", " ");
            strText = strText.Replace("</p>", "\\n");
            strText = strText.Replace("</div>", "\\n");
            strText = strText.Replace("<ol>", "\\n");
            strText = strText.Replace("</ol>", "\\n");
            strText = strText.Replace("<li>", "   - ");
            strText = strText.Replace("</li>", "\\n");
            strText = strText.Replace(",", "\\,");
            strText = ReplaceText(strText, "<.*?>", "");
            return strText;
        }

        /// <summary>
        /// A class to contain static control character values.
        /// </summary>
        /// <remarks>
        /// Used to replace usages of VB ControlChars.CrLf.
        /// </remarks>
        public class ControlChars
        {
            public const String CrLf = "\r\n";
        }

    }
}