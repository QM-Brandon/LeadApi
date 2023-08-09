using System;
using System.Data;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using BusinessObjects;
using Support.Miscellaneous;
using Support.Data;
using System.IO;
using System.Linq;

namespace Support.Email
{
    public class EmailMessageText
	{
		private DataTable _dt;
		public DataTable dt {
			get { return this._dt; }
			set { this._dt = value; }
		}

		private string _MessageText = "";
		public string MessageText {
			get { return this._MessageText; }
			set { this._MessageText = value; }
		}

		private string _SubjectText = "";
		public string SubjectText {
			get { return this._SubjectText; }
			set { this._SubjectText = value; }
		}

		private bool _MessageLoaded = false;
		public bool MessageLoaded {
			get { return this._MessageLoaded; }
			set { this._MessageLoaded = value; }
		}

		public bool LoadMessageText(string strMessageType, string strUser)
		{
			EmailtextBO oEmailtext = new EmailtextBO();
			oEmailtext.GetRecordByEmailtype(strMessageType);
			this.dt = oEmailtext.dt;
			this.MessageLoaded = (this.dt.Rows.Count > 0);
			if (MessageLoaded) {
				this.MessageText = AppUtils.RenderText((string)oEmailtext.dt.Rows[0]["messagetext"], strUser);
				this.SubjectText = (string)oEmailtext.dt.Rows[0]["subjecttext"];
			}
			return this.MessageLoaded;
		}

        public bool LoadMessageText(string strMessageType, DataRow data)
        {
            EmailtextBO oEmailtext = new EmailtextBO();
            oEmailtext.GetRecordByEmailtype(strMessageType);
            this.dt = oEmailtext.dt;
            this.MessageLoaded = (this.dt.Rows.Count > 0);
            if (MessageLoaded)
            {
                this.MessageText = AppUtils.RenderText((string)oEmailtext.dt.Rows[0]["messagetext"], data);
                this.SubjectText = (string)oEmailtext.dt.Rows[0]["subjecttext"];
            }
            return this.MessageLoaded;
        }

        public bool LoadMessageText(string strMessageType)
        {
            EmailtextBO oEmailtext = new EmailtextBO();
            oEmailtext.GetRecordByEmailtype(strMessageType);
            this.dt = oEmailtext.dt;
            this.MessageLoaded = (this.dt.Rows.Count > 0);
            if (MessageLoaded)
            {
                this.MessageText = (string)oEmailtext.dt.Rows[0]["messagetext"];
                this.SubjectText = (string)oEmailtext.dt.Rows[0]["subjecttext"];
            }
            return this.MessageLoaded;
        }
    }

    public class Email
	{
        private EmailAccountBO oEmailAccount = new EmailAccountBO();

        private bool GetEmailAccount()
        {
            bool bSuccess = false;
            oEmailAccount.LoadDataTable("select top 1 *" +
                " from emailaccount" +
                " where active = 1" +
                " order by lastused");
            if (oEmailAccount.dt.Rows.Count > 0)
            {
                DataReaderSql dr = new DataReaderSql();
                dr.SQLExecuteNonQuery(
                    "update emailaccount" +
                    " set lastused=SYSDATETIME()" +
                    " where emailaccountid='" + oEmailAccount.Row["emailaccountid"].ToString() + "'");
                bSuccess = true;
            }
            return bSuccess;
        }

        public string SendEmailMessage(
            string[] strRecipient, string strSubject, string strMessage, string strFileList,
            string strCC, string strBCC, string strReplyTo, bool boolSendAsHTML)
        {
            string strResult = "";
            string strSenderAddress = "";
            string strEmailAuthUser = "";
            string strEmailAuthPassword = "";
            string strMailserver = "";
            int intPort = 0;
            bool boolUseSSL = false;

            if (GetEmailAccount())
            {
                strSenderAddress = oEmailAccount.Row["emailaccount"].ToString();
                strEmailAuthUser = oEmailAccount.Row["login"].ToString();
                strEmailAuthPassword = oEmailAccount.Row["password"].ToString();
                strMailserver = oEmailAccount.Row["emailserver"].ToString();
                intPort = (int)oEmailAccount.Row["emailport"];
                boolUseSSL=(bool)oEmailAccount.Row["usessl"];
                //int.TryParse((string)oEmailAccount.Row["emailport"], out intPort);
                //bool.TryParse((string)oEmailAccount.Row["usessl"], out boolUseSSL);
                }
            else
            {
                strSenderAddress = WebConfigurationManager.AppSettings["emailaccount"];
                strEmailAuthUser = WebConfigurationManager.AppSettings["emailauthuser"];
                strEmailAuthPassword = WebConfigurationManager.AppSettings["emailauthpassword"];
                strMailserver = WebConfigurationManager.AppSettings["emailserver"];
                int.TryParse(WebConfigurationManager.AppSettings["emailport"], out intPort);
                bool.TryParse(WebConfigurationManager.AppSettings["emailssl"], out boolUseSSL);
            }

            strResult = SendEmailMessage(strRecipient, strSubject, strMessage, strFileList,
                strCC, strBCC, strReplyTo, boolSendAsHTML, strSenderAddress, strEmailAuthUser,
			strEmailAuthPassword, strMailserver, intPort, boolUseSSL);
			return strResult;
		}

		//This function takes string array parameters for multiple recipients and files
		public string SendEmailMessage(
            string[] strRecipient, string strSubject, string strMessage, string strFileList,
            string strCC, string strBCC, string strReplyTo, bool boolSendAsHTML,
            string strSenderAddress, string strEmailAuthUser,
            string strEmailAuthPassword, string strMailserver, int intPort, bool boolUseSSL)
		{
			string strResult = "";
			try {
				foreach (string strItem in strRecipient) {
					strResult = SendEmailMessage(strItem, strSubject, strMessage, strFileList,
                        strCC, strBCC, strReplyTo, boolSendAsHTML, strSenderAddress, strEmailAuthUser,
					    strEmailAuthPassword, strMailserver, intPort, boolUseSSL);

					if (strResult.Substring(0, 6) == "Error:") {
						break; // TODO: might not be correct. Was : Exit For
					}
				}
			} catch (Exception ex) {
				//Message Error
				strResult = "Error: " + ex.Message;
			}

            return strResult;
		}

        public string SendEmailMessage(
            string strRecipient, string strSubject, string strMessage)
        {
            return SendEmailMessage(
                strRecipient, strSubject, strMessage, "", "", "", "", true);
        }

        public string SendEmailMessage(
            string strRecipient, string strSubject, string strMessage, string strFileList,
            string strCC, string strBCC)
        {
            return SendEmailMessage(
                strRecipient, strSubject, strMessage, strFileList, strCC, strBCC, "", true);
        }

        public string SendEmailMessage(
            string strRecipient, string strSubject, string strMessage, string strFileList,
            string strCC, string strBCC, string strReplyTo, bool boolSendAsHTML)
		{
            string strResult = "";
            string strSenderAddress = "";
            string strEmailAuthUser = "";
            string strEmailAuthPassword = "";
            string strMailserver = "";
            int intPort = 0;
            bool boolUseSSL = false;

            if (GetEmailAccount())
            {
                strSenderAddress = oEmailAccount.Row["emailaccount"].ToString();
                strEmailAuthUser = oEmailAccount.Row["login"].ToString();
                strEmailAuthPassword = oEmailAccount.Row["password"].ToString();
                strMailserver = oEmailAccount.Row["emailserver"].ToString();
                intPort = (int)oEmailAccount.Row["emailport"];
                boolUseSSL = (bool)oEmailAccount.Row["usessl"];
            }
            else
            {
                strSenderAddress = WebConfigurationManager.AppSettings["emailaccount"];
                strEmailAuthUser = WebConfigurationManager.AppSettings["emailauthuser"];
                strEmailAuthPassword = WebConfigurationManager.AppSettings["emailauthpassword"];
                strMailserver = WebConfigurationManager.AppSettings["emailserver"];
                int.TryParse(WebConfigurationManager.AppSettings["emailport"], out intPort);
                bool.TryParse(WebConfigurationManager.AppSettings["emailssl"], out boolUseSSL);
            }

            strResult = SendEmailMessage(strRecipient, strSubject, strMessage, strFileList,
                strCC, strBCC, strReplyTo, boolSendAsHTML, strSenderAddress, strEmailAuthUser,
    			strEmailAuthPassword, strMailserver, intPort, boolUseSSL);
			return strResult;
		}

        //This function overrides the first function and accepts a single
        //string for the recipient and file attachement
        public string SendEmailMessage(
            string strRecipient, string strSubject, string strMessage, string strFileList,
            string strCC, string strBCC, string strReplyTo, bool boolSendAsHTML, string strSenderAddress, string strEmailAuthUser,
		    string strEmailAuthPassword, string strMailserver, int intPort, bool boolUseSSL)
		{
			string strResult = "";
			try {
                //MailMessage MailMsg = new MailMessage(new MailAddress(strSenderAddress.Trim()), new MailAddress(strRecipient));
                //MailMsg.From = (string.IsNullOrEmpty(strReplyTo) ? new MailAddress(strSenderAddress.Trim()) : new MailAddress(strReplyTo));
                //MailMsg.Sender = new MailAddress(strSenderAddress.Trim());

                MailMessage MailMsg = new MailMessage();
                MailMsg.From = new MailAddress(strSenderAddress.Trim());
                MailMsg.BodyEncoding = Encoding.Default;
                MailMsg.Subject = strSubject.Trim();
                var recipients = strRecipient.Split(new char[] { ',', ' ', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);


                bool bTestmode = false;
                bool.TryParse(WebConfigurationManager.AppSettings["testmode"], out bTestmode);
                if (bTestmode)
                {
                    var emailAddress = WebConfigurationManager.AppSettings["disengagednotificationemail"];
                    var nl = boolSendAsHTML ? "<br />" : System.Environment.NewLine;
                    MailMsg.To.Add(emailAddress);
                    MailMsg.Body = "This is an email from staging. It would normally have been sent to: " + nl
                        + recipients.Aggregate((acc, el) => acc + nl + el)
                        + nl + strMessage.Trim() + nl;
                }
                else
                {
                    foreach (var email in recipients)
                    {
                        MailMsg.To.Add(email);
                    }

                    MailMsg.Body = strMessage.Trim() + "\r\n";
                }

                MailMsg.Priority = MailPriority.Normal;
				if (!string.IsNullOrEmpty(strCC)) {
                    MailMsg.CC.Add(strCC);
				}
				if (!string.IsNullOrEmpty(strBCC)) {
                    MailMsg.Bcc.Add(strBCC);
				}
                MailMsg.IsBodyHtml = boolSendAsHTML;
				if (!string.IsNullOrEmpty(strReplyTo)) {
                    MailMsg.ReplyTo = new MailAddress(strReplyTo);
				}

				if (!string.IsNullOrEmpty(strFileList))
                {
					//Attach each file attachment
                    string[] strFiles = strFileList.Split(',');

					foreach (string strFile in strFiles)
                    {
                        if (!string.IsNullOrEmpty(strFile) && File.Exists(strFile))
                        {
							Attachment MsgAttach = new Attachment(strFile.Trim());
							MailMsg.Attachments.Add(MsgAttach);
						}
					}
				}

				//Smtpclient to send the mail message
				System.Net.NetworkCredential oCred = new System.Net.NetworkCredential();
				SmtpClient SmtpMail = new SmtpClient();
                SmtpMail.Host = strMailserver;
                SmtpMail.Port = intPort;
                SmtpMail.EnableSsl = boolUseSSL;
				if (!string.IsNullOrEmpty(strEmailAuthUser)) {
                    SmtpMail.Credentials = new System.Net.NetworkCredential(strEmailAuthUser, strEmailAuthPassword);
				}
                SmtpMail.Send(MailMsg);
				//Message Successful
				strResult = "Success";
			} catch (Exception ex) {
				//Message Error

				strResult = "Error: " + ex.Message;
			}
			return strResult;
		}

		public bool ValidEmailAddress(string Email)
		{
			//"\b[A-Z0-9._%-]+@[A-Z0-9._%-]+\.[A-Z]{2,4}\b"
			//"^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$"
			//"^([a-zA-Z0-9_\-\.']+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)/(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}/[0-9]{1,3})"
			Regex oRegex = new Regex("\\A^\\w[A-Z0-9._%-]+@[A-Z0-9._%-]+\\.[A-Z]{2,4}\\Z",
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
			int nValidEmails = oRegex.Matches(Email).Count;
			return (nValidEmails == 1);
		}
	}
}