using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using Support.Business;
using Support.Email;
using Support.Miscellaneous;

namespace BusinessObjects
{
    public class EmailAccountBO : BusinessObject
    {
        public EmailAccountBO()
        {
            this.TableName = "Emailaccount";
            this.PrimaryKey = "Emailaccountid";
            this.FieldList = "*";
            this.LoadTableStructure();
            this.RequiredFields.AddRange(
                new string[] {
                    "emailserver,Email Server",
                    "emailport,Email Port",
                    "emailaccount,Email Account",
                    "login,Login",
                    "password,Password"
                }
            );
        }
        public override void SetFieldDefaults(DataRow row)
        {
            Row["Emailaccountid"] = System.Guid.NewGuid();
            Row["active"] = true;
        }
    } // EmailAccountBO

    public class EmailtextBO : BusinessObject
	{
		public EmailtextBO()
		{
			this.TableName = "Emailtext";
			this.PrimaryKey = "Emailtextid";
			this.FieldList = "*";
			this.LoadTableStructure();
            this.RequiredFields.AddRange(
                new string[] {
                    "emailtype, Email Type",
                    "subjecttext, Subject Text",
                    "messagetext, Message Text"
                }
            );
		}

        public override void SetFieldDefaults(DataRow row)
		{
			row["emailtype"] = "";
			row["subjecttext"] = "";
			row["messagetext"] = "";
		}

		// Returns the Emailtext record matching the specified Emailtype
		public void GetRecordByEmailtype(string strEmailtype)
		{
			string Command =
                "SELECT " + this.FieldList + " FROM " + this.TableName +
                " WHERE emailtype='" + strEmailtype + "'";

            dt = this.GetDataTable(Command);
		}
	} // EmailtextBO

	public class FileCaptionBO : BusinessObject
	{
		public FileCaptionBO()
		{
			this.TableName = "FileCaption";
			this.PrimaryKey = "Captionid";
			this.FieldList = "*";
			this.LoadTableStructure();
            this.RequiredFields.AddRange(
                new string[] {
                    "Filename, File Name",
                    "Caption, Caption"
                }
            );
		}

        public override void SetFieldDefaults(DataRow Row)
		{
		}

		/// <summary>
		/// Returns a DataTable containing the record with the specified caption matching the supplied filename.
		/// </summary>
		/// <param name="Filename">Filename</param>
		/// <returns>DataSet</returns>
		public void GetRecordByFilename(string FileName)
		{
			string Command =
                "SELECT " + this.FieldList + " FROM " + this.TableName +
                " WHERE filename='" + FileName.Replace("\\","/") + "'";
            dt = this.GetDataTable(Command);
		}

        public void DeleteRecordByFilename(string Filename)
		{
			this.GetRecordByFilename(Filename.Replace("\\","/"));
			if (dt.Rows.Count > 0)
            {
				for (int xx = 0; xx <= dt.Rows.Count - 1; xx++)
                {
					this.DeleteRow(true, xx);
				}
			}
		}

		public int DeleteRecordsByFolder(string Folder)
		{
			string strSQL =
                "delete from " + this.TableName +
                " where filename like '" + Utilities.AddBS(Folder.Replace("\\","/")) + "%'";

			int intCount = ExecuteSQL(strSQL);
			return intCount;
		}
    } // FileCaptionBO

	public class InterestBO : BusinessObject
	{
		public InterestBO()
		{
			this.TableName = "Interest";
			this.PrimaryKey = "Interestid";
			this.FieldList = "*";
			this.LoadTableStructure();
			this.RequiredFields.AddRange(
                new string[] {
                    "category, Category",
                    "interest, Interest"
                }
            );
		}

        public override void SetFieldDefaults(DataRow row)
		{
			row["category"] = "";
			row["interest"] = "";
		}

		// Returns the UserInterest records matching the specified Category
		public DataTable GetRecordsByCategory(string strCategory, string strInterestFilter, bool boolExactMatch)
		{
			string Command = null;
			if (string.IsNullOrEmpty(strInterestFilter))
            {
				Command =
                    "SELECT " + this.FieldList + " FROM " + this.TableName +
                    " WHERE category='" + strCategory + "'";
			}
            else if (boolExactMatch)
            {
				Command =
                    "SELECT " + this.FieldList + " FROM " + this.TableName +
                    " WHERE category='" + strCategory + "' and interest = '" + strInterestFilter + "'";
			}
            else
            {
				Command =
                    "SELECT " + this.FieldList + " FROM " + this.TableName +
                    " WHERE category='" + strCategory + "' and interest like '" + strInterestFilter + "%'";
			}

			DataTable dtResult = this.GetDataTable(Command + " order by interest");
			return dtResult;
		}

        public DataTable GetRecordsByCategory(string strCategory, string strInterestFilter)
        {
            return GetRecordsByCategory(strCategory, strInterestFilter, false);
        }

        public DataTable GetRecordsByCategory(string strCategory)
        {
            return GetRecordsByCategory(strCategory, "");
        }
	} // InterestBO

    public class IPRestrictionBO : BusinessObject
    {
        public IPRestrictionBO()
        {
            this.TableName = "IPRestriction";
            this.PrimaryKey = "IPRestrictionId";
            this.FieldList = "*";
            this.LoadTableStructure();
            this.RequiredFields.AddRange(
                new string[] {
                    "IPAddress,IP Address",
                    "Roles,Roles"
                }
            );
        }
        public override void SetFieldDefaults(DataRow row)
        {
            row["IPRestrictionId"] = System.Guid.NewGuid();
        }
    } // IPRestrictionBO

    public class LogBO : BusinessObject
    {
        public LogBO()
        {
            this.TableName = "Log";
            this.PrimaryKey = "Logid";
            this.FieldList = "*";
            this.LoadTableStructure();
        }

        public override void SetFieldDefaults(DataRow row)
        {
            row["Logid"] = System.Guid.NewGuid();
            //row["addedon"] = DateTime.Now;
        }

        public void AddEntry(string message)
        {
            AddNewRow();
            Row["message"] = message;
            Row["datetime"] = DateTime.Now;
            SaveDataTable();
        }
    } // LogBO

    // LoginHistory Business Object
    public class LoginHistoryBO : BusinessObject
	{
		public LoginHistoryBO()
		{
            this.TableName = "LoginHistory";
            this.PrimaryKey = "LoginHistoryid";
            this.FieldList = "*";
            this.LoadTableStructure();
        }

        public override void SetFieldDefaults(DataRow row)
		{
            row["logindate"] = DateTime.Now;
            row["ip"] = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            row["useragent"] = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];
		}

        public void SendLoginFailureNotification()
        {
            // On login failure, check for hacking attempt and send email notification based on
            // the email notification interval

            bool bSendemail = false;
            string strSQL = null;
            BusinessObject oData = new BusinessObject();

            // Retrieve timeframe for which the number of failed login attempts is gathered.
            int intHours = 1;
            string strHours = WebConfigurationManager.AppSettings["loginfailuretimeframe"];
            int.TryParse(strHours, out intHours);
            string strThresholdTime = DateTime.Now.AddHours(-1 * intHours).ToString();

            string strRecipients = WebConfigurationManager.AppSettings["loginfailurenotificationlist"];
            if (!String.IsNullOrEmpty(strRecipients))
            {
                // Retrieve threshold limit of failed attempts before notification is sent
                int intFailedAttemptThreshold = 10;
                string strFailedAttemptThreshold = WebConfigurationManager.AppSettings["loginfailureattempts"];
                int.TryParse(strFailedAttemptThreshold, out intFailedAttemptThreshold);

                // Check for suspected hacking attempts
                strSQL = "select count(loginhistoryid) as tally" +
                    " from loginhistory" +
                    " where loginfailed = 1" +
                    " and logindate >= '" + strThresholdTime + "'";
                DataTable dt = oData.GetDataTable(strSQL);
                //HttpContext.Current.Trace.Warn("Failure SQL=" + strSQL);

                if ((int)dt.Rows[0]["tally"] >= intFailedAttemptThreshold)
                {
                    // Threshold reached, send email
                    // Retrieve the frequency (in hours) at which notification emails are sent.
                    //HttpContext.Current.Trace.Warn("Login failure threshold reached. Sending email");
                    int intFrequency = 15;
                    string strFrequency = WebConfigurationManager.AppSettings["loginfailurenotificationfrequency"];
                    int.TryParse(strFrequency, out intFrequency);
                    string strEmailThreshholdTime = DateTime.Now.AddMinutes(-1 * intFrequency).ToString();

                    strSQL = "select * from loginnotification where datenotified >= '" + strEmailThreshholdTime + "'";
                    DataTable dtFailures = oData.GetDataTable(strSQL);
                    //HttpContext.Current.Trace.Warn("Notification SQL=" + strSQL);
                    if (dtFailures.Rows.Count == 0)
                    {
                        // Notification has not been sent within desired notification frequency, send now
                        bSendemail = true;
                        //HttpContext.Current.Trace.Warn("bSendemail=" + bSendemail.ToString());
                    }
                }
            }

            if (bSendemail)
            {
                // When notification criteria is met (above), send a notification email
                string strWebsiteName = WebConfigurationManager.AppSettings["websitename"];
                StringBuilder sbMessage = new StringBuilder();
                sbMessage.Append(
                    "<p>There has been a suspected hacking attempt on the " +
                    strWebsiteName + " web site.</p>\r\n" +
                    "<table border='1' cellspacing='0' cellpadding='3'>" +
                    "<tr><th>Date/Time</th><th>IP</th><th>Username</th><th>Password</th><th>Reason</th><th>Useragent</th>" +
                    "</tr>");
                strSQL = "select *" +
                    " from loginhistory" +
                    " where loginfailed = 1" +
                    " and logindate >= '" + strThresholdTime + "'";
                DataTable dt = oData.GetDataTable(strSQL);
                // HttpContext.Current.Trace.Warn("History SQL=" + strSQL);
                foreach (DataRow dr in dt.Rows)
                {
                    sbMessage.Append(
                        "<tr>" +
                        "<td>" + dr["logindate"].ToString() + "</td>" +
                        "<td>" + dr["ip"].ToString() + "</td>" +
                        "<td>" + dr["username"].ToString() + "</td>" +
                        "<td>" + dr["password"].ToString() + "</td>" +
                        "<td>" + dr["failurereason"].ToString() + "</td>" +
                        "<td>" + dr["useragent"].ToString() + "</td>" +
                        "</tr>");
                }
                sbMessage.Append("</table>");
                Email oEmail = new Email();
                string strTitle = "*** Suspected Hacking Attempt ***";
                HttpContext.Current.Trace.Warn("Recipients="+strRecipients);
                HttpContext.Current.Trace.Warn(
                oEmail.SendEmailMessage(strRecipients, strTitle, sbMessage.ToString())
                );

                // Log that notification email was sent
                LoginNotificationBO oLoginNotification = new LoginNotificationBO();
                oLoginNotification.AddNewRow();
                oLoginNotification.Row["datenotified"] = DateTime.Now;
                oLoginNotification.Row["failurecount"] = dt.Rows.Count;
                oLoginNotification.Row["lastloginhistoryid"] = dt.Rows[dt.Rows.Count - 1]["loginhistoryid"];
                oLoginNotification.SaveDataTable();

            }
        }
    } //LoginHistoryBO

    // LoginNotification Business Object
    public class LoginNotificationBO : BusinessObject
	{
		public LoginNotificationBO()
		{
            this.TableName = "LoginNotification";
            this.PrimaryKey = "LoginNotificationid";
            this.FieldList = "*";
            this.LoadTableStructure();
        }
    } // LoginNotificationBO

    public class LookupBO : BusinessObject
	{
		public LookupBO()
		{
			this.TableName = "Lookup";
			this.PrimaryKey = "Lookupid";
			this.FieldList = "*";
			this.LoadTableStructure();
            this.RequiredFields.AddRange(
                new string[] {
                    "Type, Lookup Type",
                    "Itemvalue, Lookup Value",
                    "Itemdescription, Description"
                }
            );
		}

        public override void SetFieldDefaults(DataRow row)
		{
			row["type"] = "";
			row["itemvalue"] = "";
			row["itemdescription"] = "";
			row["itemorder"] = 0;
            row["active"] = true;
        }
	} // LookupBO

    public class PagemasterBO : BusinessObject
	{
		public PagemasterBO()
		{
			this.TableName = "Pagemaster";
			this.PrimaryKey = "Pageid";
			this.FieldList = "*";
			this.LoadTableStructure();
            this.RequiredFields.AddRange(
                new string[] {
                    "Pagename, Page Name",
                    "Pagetitle, Page Title",
                    "Menutext, Menu Text"
                }
            );
		}

        public override void SetFieldDefaults(DataRow row)
		{
            row["sortorder"] = 0;
			row["active"] = true;
			row["remap"] = true;
			row["content"] = "";
			row["sidebar"] = "";
			row["pagetitle"] = "";
			row["menutext"] = "";
			row["showdocs"] = true;
			row["showvideos"] = true;
			row["showphotos"] = "thumbnails";
		}

        /// <summary>
		/// Returns a DataTable containing the record with the specified page name (example, "default.aspx")
		/// </summary>
		/// <param name="primarykey">Primary Key</param>
		/// <returns>DataSet</returns>
		public void GetRecordByPage(string PageName)
		{
			string Command =
                "SELECT " + this.FieldList + " FROM " + this.TableName +
                " WHERE pagename='" + PageName + "'";

			dt = this.GetDataTable(Command);
		}

        public override void CheckRulesHook(DataTable dt)
        {
            if (this.IsFilenameValid((string)dt.Rows[0]["pagename"]))
            {
                this.IsFilenameInUse((string)dt.Rows[0]["pagename"], (int)dt.Rows[0]["pageid"]);
            }
        }

        public bool IsFilenameValid(string Pagename)
        {
            bool IsValid = true;
            if (Pagename.Trim().Contains(" "))
            {
                this.AddBrokenRule("Page name must not contain blanks.");
                IsValid = false;
            }
            else if (!Pagename.Trim().EndsWith(".aspx"))
            {
                this.AddBrokenRule("Page name must end with '.aspx'.");
                IsValid = false;
            }

            return IsValid;
        }

        public bool IsFilenameInUse(string Pagename, int Pageid)
        {
            bool InUse = true;
            string Command = "";
            if ((Pageid == null))
            {
                Command = "SELECT * FROM pagemaster WHERE pagename='" + Pagename + "'";
            }
            else
            {
                Command = "SELECT * FROM pagemaster WHERE pagename='" + Pagename + "' and pageid <> " + Pageid;
            }

            Support.Data.SQLDataAccess SQLData = new Support.Data.SQLDataAccess();
            DataTable dtDupe = SQLData.GetDataTable(Command);
            if (dtDupe.Rows.Count > 0)
            {
                this.AddBrokenRule("Page name already exists.");
                InUse = true;
            }

            return InUse;
        }
	} // Pagemaster

    public class UserInterestBO : BusinessObject
	{
		public UserInterestBO()
		{
			this.TableName = "UserInterest";
			this.PrimaryKey = "UserInterestid";
			this.FieldList = "*";
			this.LoadTableStructure();
            this.RequiredFields.AddRange(
                new string[] {
                    "userid, User ID",
                    "category, Category",
                    "interest, Interest"
                }
            );
		}

		public override void SetFieldDefaults(DataRow row)
		{
			try
            {
				MembershipUser mu = Membership.GetUser();
				row["userid"] = mu.ProviderUserKey.ToString();
			}
            catch (Exception ex)
            {
				// Allows record to be created for non-logged in user.
			}

			row["category"] = "";
			row["interest"] = "";
			row["interestid"] = 0;
			row["notes"] = "";
		}

		// Returns the UserInterest records matching the specified Category
		public void GetRecordsByCategory(string strCategory)
		{
			string Command =
                "SELECT " + this.FieldList + " FROM " + this.TableName +
                " WHERE category='" + strCategory + "'";

			dt = this.GetDataTable(Command);
		}

		// Returns the UserInterest records matching the specified Category
		public void GetRecordsByUserid(int intUserid)
		{
			if (intUserid == 0)
            {
				intUserid = Utilities.GetUserID();
			}

            string Command =
                "SELECT " + this.FieldList + " FROM " + this.TableName +
                " WHERE userid=" + intUserid.ToString();

            dt = this.GetDataTable(Command);
		}

        public override void CheckRulesHook(DataTable dt)
        {
            this.IsDuplicate(dt);
        }

        public bool IsDuplicate(DataTable dt)
        {
            bool InUse = true;
            try
            {
                int intUserid = (int)dt.Rows[0]["userid"];
                string strCategory = (string)dt.Rows[0]["category"];
                string strInterest = (string)dt.Rows[0]["interest"];
                int intUserinterestid = (int)dt.Rows[0]["userinterestid"];

                string Command =
                    "SELECT * FROM userinterest" +
                    " WHERE category='" + strCategory + "' and interest='" + strInterest +
                    "' and userid = " + intUserid.ToString() + " and userinterestid <> " + intUserinterestid.ToString();

                Support.Data.SQLDataAccess SQLData = new Support.Data.SQLDataAccess();
                DataTable dtDupe = SQLData.GetDataTable(Command);
                if (dtDupe.Rows.Count > 0)
                {
                    this.AddBrokenRule("This Interest is already on file.");
                    InUse = true;
                }
            }
            catch (Exception ex)
            {
                this.AddBrokenRule("Error in IsDuplicate check of UserInterestBO: " + ex.Message);
            }

            return InUse;
        }
    } // UserInterestBO

    public class UsersBO : BusinessObject
	{
		public UsersBO()
		{
			this.TableName = "Users";
			this.PrimaryKey = "Userid";
			this.FieldList = "*";
			this.LoadTableStructure();
            this.RequiredFields.AddRange(
                new string[] {
                    "aspnet_userid, ASP.Net User ID",
                    "username, Username",
                    "firstname, First Name",
                    "lastname, Last Name",
                    "email, Email"
                }
            );
		}

        public override bool ChildrenCanBeDeleted(DataRow row)
        {
            bool bChildrenCanBeDeleted = true;
            BusinessObject oData = new BusinessObject();
            oData.LoadDataTable(string.Format(@"select * from qmcase where userid={0}", row["userid"]));
            if (oData.HasRows)
            {
                bChildrenCanBeDeleted = false;
                this.AddBrokenRule(string.Format(@"Unable to delete. User has {0} Cases on file.",oData.dt.Rows.Count));
            }
            return bChildrenCanBeDeleted;
        }

        public override void SetFieldDefaults(DataRow row)
		{
			row["authorizationid"] = Support.Miscellaneous.Utilities.RandomNumber();
			try
            {
				MembershipUser mu = Membership.GetUser();
				row["aspnet_userid"] = mu.ProviderUserKey.ToString();
			}
            catch (Exception ex)
            {
                // Allows record to be created for non-logged in user.
			}
		}

        // Returns the User record matching the specified Userid
		public void GetRecordByAspnet_Userid(string strAspnet_Userid)
		{
			if (string.IsNullOrEmpty(strAspnet_Userid))
            {
				MembershipUser mu = Membership.GetUser();
				strAspnet_Userid = mu.ProviderUserKey.ToString();
			}
			
            string Command =
                "SELECT " + this.FieldList + " FROM " + this.TableName +
                " WHERE aspnet_userid='" + strAspnet_Userid + "'";

            dt = this.GetDataTable(Command);
		}
    } // UsersBO
}