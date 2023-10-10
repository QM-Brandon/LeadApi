using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Q.TESTAPI2.Models;
using QuintessaMarketing.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Configuration;

public class CaseLeadRepository : ICaseLeadRepository
{
    private readonly string _connectionString;
    private readonly string _connectionStringTest;

    public CaseLeadRepository()
    {
        _connectionString = WebConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        _connectionStringTest = WebConfigurationManager.ConnectionStrings["MyConnectionTest"].ConnectionString;

    }

    public async Task<string> AddCaseLead(CaseLeadData data, Guid affiliateId, string api)
    {
        var caseid = string.Empty;
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@CampaignType", SqlDbType.VarChar) { Value = data.Source },
            new SqlParameter("@PotentialClientFullName", SqlDbType.VarChar) { Value = data.PotentialClientFullName },
            new SqlParameter("@PotentialClientFirstName", SqlDbType.VarChar) { Value = data.PotentialClientFirstName },
            new SqlParameter("@PotentialClientMiddleName", SqlDbType.VarChar) { Value = data.PotentialClientMiddleName },
            new SqlParameter("@PotentialClientLastName", SqlDbType.VarChar) { Value = data.PotentialClientLastName },
            new SqlParameter("@PotentialClientStreetAddress", SqlDbType.VarChar) { Value = data.PotentialClientStreetAddress },
            new SqlParameter("@PotentialClientCity", SqlDbType.VarChar) { Value = data.PotentialClientCity },
            new SqlParameter("@PotentialClientState", SqlDbType.VarChar) { Value = data.PotentialClientState },
            new SqlParameter("@PotentialClientZip", SqlDbType.VarChar) { Value = data.PotentialClientZip },
            new SqlParameter("@AccidentState", SqlDbType.VarChar) { Value = data.AccidentState },
            new SqlParameter("@PotentialClientPhoneNumber", SqlDbType.VarChar) { Value = data.PotentialClientPhoneNumber },
            new SqlParameter("@PotentialClientEmail", SqlDbType.VarChar) { Value = data.PotentialClientEmail },
            new SqlParameter("@CaseDescription", SqlDbType.VarChar) { Value = data.CaseDescription },
            new SqlParameter("@IncidentDate", SqlDbType.DateTime) { Value = data.ProcessedIncidentDate, IsNullable = true },
            new SqlParameter("@CaseType", SqlDbType.VarChar) { Value = data.CaseType },
            new SqlParameter("@AffiliateId", SqlDbType.UniqueIdentifier) { Value = affiliateId },
            new SqlParameter("@CaseIdOutput", SqlDbType.UniqueIdentifier,255, ParameterDirection.Output,false, 0, 0, null, DataRowVersion.Default, null),
            new SqlParameter("@LeadId", SqlDbType.VarChar) {Value = null}
        };

        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                var command = new SqlCommand("QMCase_AddLead", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 50
                };
                command.Parameters.AddRange(parameters.ToArray());
                connection.Open();
                await command.ExecuteNonQueryAsync();
                caseid = Convert.ToString(command.Parameters["@CaseIdOutput"].Value);
                string insertIntoLogs = "insert into qmLeadsApiLogs (Timestamp,Action, APIKey, caseid) values ("
                + "'" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "',"
                + "'SubmitLead',"
                + "'" + api + "',"
                + "'" + caseid + "')";

                var commandLog = new SqlCommand(insertIntoLogs, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 50
                };
                var result = commandLog.ExecuteNonQuery();
            }

            finally
            {
                connection.Close();
            }
        }

        return caseid;
    }


    public async Task<string> AddCaseLead(CaseLeadData data, Guid affiliateId, string api,Dictionary<string,string> extraData)
    {
        var caseid = string.Empty;
        var test = data.ProcessedIncidentDate;
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@CampaignType", SqlDbType.VarChar) { Value = data.Source },
            new SqlParameter("@PotentialClientFullName", SqlDbType.VarChar) { Value = data.PotentialClientFullName },
            new SqlParameter("@PotentialClientFirstName", SqlDbType.VarChar) { Value = data.PotentialClientFirstName },
            new SqlParameter("@PotentialClientMiddleName", SqlDbType.VarChar) { Value = data.PotentialClientMiddleName },
            new SqlParameter("@PotentialClientLastName", SqlDbType.VarChar) { Value = data.PotentialClientLastName },
            new SqlParameter("@PotentialClientStreetAddress", SqlDbType.VarChar) { Value = data.PotentialClientStreetAddress },
            new SqlParameter("@PotentialClientCity", SqlDbType.VarChar) { Value = data.PotentialClientCity },
            new SqlParameter("@PotentialClientState", SqlDbType.VarChar) { Value = data.PotentialClientState },
            new SqlParameter("@PotentialClientZip", SqlDbType.VarChar) { Value = data.PotentialClientZip },
            new SqlParameter("@AccidentState", SqlDbType.VarChar) { Value = data.AccidentState },
            new SqlParameter("@PotentialClientPhoneNumber", SqlDbType.VarChar) { Value = data.PotentialClientPhoneNumber },
            new SqlParameter("@PotentialClientEmail", SqlDbType.VarChar) { Value = data.PotentialClientEmail },
            new SqlParameter("@CaseDescription", SqlDbType.VarChar) { Value = data.CaseDescription },
            new SqlParameter("@IncidentDate", SqlDbType.DateTime) { Value = data.ProcessedIncidentDate, IsNullable = true },
            new SqlParameter("@CaseType", SqlDbType.VarChar) { Value = data.CaseType },
            new SqlParameter("@AffiliateId", SqlDbType.UniqueIdentifier) { Value = affiliateId },
            new SqlParameter("@CaseIdOutput", SqlDbType.UniqueIdentifier,255, ParameterDirection.Output,false, 0, 0, null, DataRowVersion.Default, null),
            new SqlParameter("@LeadId", SqlDbType.VarChar) {Value = null},
            new SqlParameter("@atFault", SqlDbType.VarChar) {Value = extraData["atFault"]},
            new SqlParameter("@work_vehicle", SqlDbType.VarChar) {Value = extraData["work_vehicle"]},
            new SqlParameter("@injured", SqlDbType.VarChar) {Value = extraData["injured"]},
            new SqlParameter("@medical", SqlDbType.VarChar) {Value = extraData["medical"]},
            new SqlParameter("@retainedAttorney", SqlDbType.VarChar) {Value = extraData["retainedAttorney"]},
            new SqlParameter("@c", SqlDbType.VarChar) {Value = extraData["c"]},
            new SqlParameter("@s1", SqlDbType.VarChar) {Value = extraData["s1"]},
            new SqlParameter("@s2", SqlDbType.VarChar) {Value = extraData["s2"]},
            new SqlParameter("@s3", SqlDbType.VarChar) {Value = extraData["s3"]}
        };

        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                var command = new SqlCommand("QMCase_AddLead_CID", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 50
                };
                command.Parameters.AddRange(parameters.ToArray());
                connection.Open();

                await command.ExecuteNonQueryAsync();
                caseid = Convert.ToString(command.Parameters["@CaseIdOutput"].Value);
                string insertIntoLogs = "insert into qmLeadsApiLogs (Timestamp,Action, APIKey, caseid) values ("
                + "'" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "',"
                + "'SubmitLeadCID',"
                + "'" + api + "',"
                + "'" + caseid + "')";

                var commandLog = new SqlCommand(insertIntoLogs, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 50
                };
                var result = commandLog.ExecuteNonQuery();
            }catch (Exception e)
            {

            }

            finally
            {
                connection.Close();
            }
        }

        return caseid;
    }
    public DataTable GetAffiliateWithApiKey(Guid apiKey)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                var sqlCommand = "Select TOP 1 * FROM qmAffiliate WHERE apikey = @API_Key";
                var command = new SqlCommand(sqlCommand, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 50
                };
                command.Parameters.Add(new SqlParameter() { ParameterName = "@API_Key", Value = apiKey, SqlDbType = SqlDbType.UniqueIdentifier });
                var dataTable = new DataTable();
                connection.Open();
                var dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(dataTable);
                dataAdapter.Dispose();
                return dataTable;
            }
            finally
            {
                connection.Close();
            }
        }
    }

    public async Task LogAPIInformationAsync(Guid logId, DateTime dateTime, string information)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                var sqlCommand = "INSERT INTO LOG (logid, datetime, message) VALUES (@logId, @dateTime, @message)";
                var command = new SqlCommand(sqlCommand, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 50
                };
                command.Parameters.Add(new SqlParameter() { ParameterName = "@logId", SqlDbType = SqlDbType.UniqueIdentifier, Value = logId });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@dateTime", SqlDbType = SqlDbType.DateTime, Value = dateTime });
                command.Parameters.Add(new SqlParameter() { ParameterName = "@message", SqlDbType = SqlDbType.VarChar, Value = information });
                connection.Open();
                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                connection.Close();
            }
        }
    }

    CaseLead ICaseLeadRepository.GetLead(string api,string caseid)
    {
        CaseLead toReturn = GetLead(caseid);
        string insertIntoLogs = "insert into qmLeadsApiLogs (Timestamp,Action, APIKey, caseid) values ("
                + "'" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "',"
                + "'GetLead',"
                + "'" + api + "',"
                + "'" + caseid + "')";
        using (var connection = new SqlConnection(_connectionString))
        {
            var command = new SqlCommand(insertIntoLogs, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 50
            };
            connection.Open();
            command = new SqlCommand(insertIntoLogs, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 50
            };
            var result = command.ExecuteNonQuery();
            connection.Close();
        }
        return toReturn;
    }

    private CaseLead GetLead(string caseid)
    {
        CaseLead lead = new CaseLead();
        using (var connection = new SqlConnection(_connectionString))
        {
            try
            {
                string sqlCommand = "select top 1 " +
                   "IncidentDate," +
    "CaseTypeId," +
    "CaseStatus," +
    "AttorneyId," +
    "CaseDescription," +
    "PassengerCount," +
    "PcName," +
    "PcPhone," +
    "PcEmail," +
    "PcContactName," +
    "PcContactPhone," +
    "PcBirthdate," +
    "PcSsn," +
    "PcAddress," +
    "PcCity," +
    "PcState," +
    "PcZipCode," +
    "PcInsuranceCompany," +
    "PcClaimNo," +
    "IncidentLocation," +
    "PcAtFault," +
    "PcInjured," +
    "PcPursuingTreatment," +
    "PcWillingForTreatment," +
    "PcEmployer," +
    "InvestigatingPd," +
    "InjuriesAndTreatment," +
    "DefendantName," +
    "DefendantClaimNo," +
    "PropertyDamageDescription," +
    "PropertyDamageAmount," +
    "PriorAttorneyName," +
    "PriorSettlement," +
    "EntryStatus," +
    "PcFirstName," +
    "PcMiddleName," +
    "PcLastName," +
    "AdverseInsurance," +
    "PoliceReportNo," +
    "PcVehicleInfo," +
    "PcVehicleLocation," +
    "PcHealthInsurance," +
    "PersonTicketed," +
    "PersonTicketedComments," +
    "PcEmployerAddress," +
    "PcEmployerCity," +
    "PcEmployerState," +
    "PcEmployerZipCode," +
    "DefendantVehicleInfo," +
    "AffiliateId," +
    "AccidentState," +
    "DriverLicense" +
                    " from qmcase where caseid='" + caseid + "'";//and affiliateid ='," + affiliateid.ToString() + "'";
                var command = new SqlCommand(sqlCommand, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 50
                };
                connection.Open();
                using (SqlDataReader sdr = command.ExecuteReader())
                {
                    
                    while (sdr.Read())
                    {
                        int passengercount;
                        double PropertyDamageAmount;
                        lead.IncidentDate = DateTime.Parse(sdr["IncidentDate"].ToString());
                        lead.CaseTypeId = sdr["CaseTypeId"].ToString();
                        lead.CaseStatus = sdr["CaseStatus"].ToString();
                        lead.AttorneyId = sdr["AttorneyId"].ToString();
                        lead.CaseDescription = sdr["CaseDescription"].ToString();
                        lead.PassengerCount = int.TryParse(sdr["PassengerCount"].ToString(), out passengercount) ? passengercount : 0;
                        lead.PcName = sdr["PcName"].ToString();
                        lead.PcPhone = sdr["PcPhone"].ToString();
                        lead.PcEmail = sdr["PcEmail"].ToString();
                        lead.PcContactName = sdr["PcContactName"].ToString();
                        lead.PcContactPhone = sdr["PcContactPhone"].ToString();
                        lead.PcBirthdate = sdr["PcBirthdate"].ToString();
                        lead.PcSsn = sdr["PcSsn"].ToString();
                        lead.PcAddress = sdr["PcAddress"].ToString();
                        lead.PcCity = sdr["PcCity"].ToString();
                        lead.PcState = sdr["PcState"].ToString();
                        lead.PcZipCode = sdr["PcZipCode"].ToString();
                        lead.PcInsuranceCompany = sdr["PcInsuranceCompany"].ToString();
                        lead.PcClaimNo = sdr["PcClaimNo"].ToString();
                        lead.IncidentLocation = sdr["IncidentLocation"].ToString();
                        lead.PcAtFault = sdr["PcAtFault"].ToString();
                        lead.PcInjured = sdr["PcInjured"].ToString();
                        lead.PcPursuingTreatment = sdr["PcPursuingTreatment"].ToString();
                        lead.PcWillingForTreatment = sdr["PcWillingForTreatment"].ToString();
                        lead.PcEmployer = sdr["PcEmployer"].ToString();
                        lead.InvestigatingPd = sdr["InvestigatingPd"].ToString();
                        lead.InjuriesAndTreatment = sdr["InjuriesAndTreatment"].ToString();
                        lead.DefendantName = sdr["DefendantName"].ToString();
                        lead.DefendantClaimNo = sdr["DefendantClaimNo"].ToString();
                        lead.PropertyDamageDescription = sdr["PropertyDamageDescription"].ToString();
                        lead.PropertyDamageAmount = Double.TryParse(sdr["PropertyDamageAmount"].ToString(), out PropertyDamageAmount) ? PropertyDamageAmount : 0;
                        lead.PriorAttorneyName = sdr["PriorAttorneyName"].ToString();
                        lead.PriorSettlement = sdr["PriorSettlement"].ToString();
                        lead.EntryStatus = sdr["EntryStatus"].ToString();
                        lead.PcFirstName = sdr["PcFirstName"].ToString();
                        lead.PcMiddleName = sdr["PcMiddleName"].ToString();
                        lead.PcLastName = sdr["PcLastName"].ToString();
                        lead.AdverseInsurance = sdr["AdverseInsurance"].ToString();
                        lead.PoliceReportNo = sdr["PoliceReportNo"].ToString();
                        lead.PcVehicleInfo = sdr["PcVehicleInfo"].ToString();
                        lead.PcVehicleLocation = sdr["PcVehicleLocation"].ToString();
                        lead.PcHealthInsurance = sdr["PcHealthInsurance"].ToString();
                        lead.PersonTicketed = sdr["PersonTicketed"].ToString();
                        lead.PersonTicketedComments = sdr["PersonTicketedComments"].ToString();
                        lead.PcEmployerAddress = sdr["PcEmployerAddress"].ToString();
                        lead.PcEmployerCity = sdr["PcEmployerCity"].ToString();
                        lead.PcEmployerState = sdr["PcEmployerState"].ToString();
                        lead.PcEmployerZipCode = sdr["PcEmployerZipCode"].ToString();
                        lead.DefendantVehicleInfo = sdr["DefendantVehicleInfo"].ToString();
                        lead.AffiliateId = sdr["AffiliateId"].ToString();
                        lead.AccidentState = sdr["AccidentState"].ToString();
                        lead.DriverLicense = sdr["DriverLicense"].ToString();
                    }

                }
            }
            finally
            {

                connection.Close();
            }
        }

        return lead;
    }

    public Tuple<int, string> UpdateCase(dynamic caseUpdate)
    {
        Tuple<int, string> validate = validateRows(caseUpdate);
        if (validate.Item1 != 1)
        {
            return validate;
        }
        Dictionary<string, object> caseUpdateDict = Dyn2Dict(caseUpdate);
        CaseLead oldCase = GetLead(caseUpdateDict["caseid"].ToString());
        using (var connection = new SqlConnection(_connectionString))
        {

            string updateType = "caseid";

           
            Dictionary<string, string> oldValues = new Dictionary<string, string>();
            Dictionary<string, string> newValues = new Dictionary<string, string>();
            string updateSqlStatement = "update qmcase set ";

            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new LowercaseContractResolver();
            var json = JsonConvert.SerializeObject(oldCase, Formatting.Indented, settings);
            var oldCaseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            foreach (KeyValuePair<string, object> kvp in caseUpdateDict)
            {
                //Console.WriteLine($"Key: {property.Key}, Value: {property.Value}");
                if (kvp.Key == "caseid" || kvp.Key == "api_key")
                {
                    continue;
                }
                if (
                    kvp.Key == "casestatus" ||
                    kvp.Key == "casedescription" ||
                    kvp.Key == "pcname" ||
                    kvp.Key == "pcphone" ||
                    kvp.Key == "pcemail" ||
                    kvp.Key == "pccontactname" ||
                    kvp.Key == "pccontactphone" ||
                    kvp.Key == "pccontactname2" ||
                    kvp.Key == "pccontactphone2" ||
                    kvp.Key == "pcbirthdate" ||
                    kvp.Key == "pcssn" ||
                    kvp.Key == "pcaddress" ||
                    kvp.Key == "pccity" ||
                    kvp.Key == "pcstate" ||
                    kvp.Key == "pczipcode" ||
                    kvp.Key == "pcinsurancecompany" ||
                    kvp.Key == "pcclaimno" ||
                    kvp.Key == "incidentlocation" ||
                    kvp.Key == "pcatfault" ||
                    kvp.Key == "pcinjured" ||
                    kvp.Key == "pcpursuingtreatment" ||
                    kvp.Key == "pcwillingfortreatment" ||
                    kvp.Key == "pcemployer" ||
                    kvp.Key == "investigatingpd" ||
                    kvp.Key == "injuriesandtreatment" ||
                    kvp.Key == "defendantname" ||
                    kvp.Key == "defendantclaimno" ||
                    kvp.Key == "propertydamagedescription" ||
                    kvp.Key == "priorattorneyname" ||
                    kvp.Key == "priorsettlement" ||
                    kvp.Key == "entrystatus" ||
                    kvp.Key == "pcfirstname" ||
                    kvp.Key == "pcmiddlename" ||
                    kvp.Key == "pclastname" ||
                    kvp.Key == "adverseinsurance" ||
                    kvp.Key == "policereportno" ||
                    kvp.Key == "pcvehicleinfo" ||
                    kvp.Key == "pcvehiclelocation" ||
                    kvp.Key == "attorneystatus" ||
                    kvp.Key == "pchealthinsurance" ||
                    kvp.Key == "personticketed" ||
                    kvp.Key == "personticketedcomments" ||
                    kvp.Key == "disengagereason" ||
                    kvp.Key == "disengagenotes" ||
                    kvp.Key == "defendantvehicleinfo" ||
                    kvp.Key == "livetransfer" ||
                    kvp.Key == "gclid" ||
                    kvp.Key == "notacasereason" ||
                    kvp.Key == "recaptime" ||
                    kvp.Key == "matchtime" ||
                    kvp.Key == "rejectleadreason" ||
                    kvp.Key == "transfername" ||
                    kvp.Key == "transfertime" ||
                    kvp.Key == "intakedatetime" ||
                    kvp.Key == "incidentdate" ||
                    //kvp.Key == "updatedon" || FIX THIS
                    kvp.Key == "disengagedatetime" ||
                    kvp.Key == "emaildatetime" ||
                    kvp.Key == "pendingdisengagedatetime" ||
                    kvp.Key == "overrideintakedatetime" ||
                    kvp.Key == "retaineddatetime" ||
                    kvp.Key == "rejectleaddate"
                    )
                {

                    updateSqlStatement += kvp.Key + " ='" + kvp.Value.ToString() + "', ";
                }
                else
                {
                    updateSqlStatement += kvp.Key + " =" + kvp.Value.ToString() + ", ";
                }
                oldValues.Add(kvp.Key, oldCaseDict[kvp.Key]);
                newValues.Add(kvp.Key, kvp.Value.ToString());
            }
            updateSqlStatement += "updatedon = '" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "'";

            updateSqlStatement += " where " + updateType + " = '" + caseUpdateDict["caseid"]+"'";
            

            var command = new SqlCommand(updateSqlStatement, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 50
            };
            connection.Open();
            int result =command.ExecuteNonQuery();
            if(result <= 0)
            {
                return new Tuple<int, string>(-1, "Row not updated");
            }
            string newValuesSql = "";
            foreach (var kvp in newValues)
            {
                newValuesSql += kvp.Key + ":" + kvp.Value + ",";
            }
            newValuesSql.TrimEnd();
            string oldValuesSql = "";
            foreach (var kvp in oldValues)
            {
                oldValuesSql += kvp.Key + ":" + kvp.Value + ",";
            }
            oldValuesSql.TrimEnd();
            string updateCaseId = caseUpdateDict.ContainsKey("caseid") ? caseUpdateDict["caseid"].ToString() : "";
            string insertIntoLogs = "insert into qmLeadsApiLogs (Timestamp,Action, [Previous Fields and Values],[New Fields and Values], APIKey, caseid) values ("
                + "'" + DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + "',"
                +"'Update',"
                +"'"+ oldValuesSql+"',"
                + "'" + newValuesSql + "',"
                +"'"+caseUpdateDict["api_key"]+"',"
                +"'"+ updateCaseId + "')";

            command = new SqlCommand(insertIntoLogs, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 50
            };
            result = command.ExecuteNonQuery();
            if (result <= 0)
            {
                return new Tuple<int, string>(0, "Log Not Entered");
            }
            connection.Close();
            return new Tuple<int, string>(1, "");
        }
    }

    static IEnumerable<KeyValuePair<string, object>> GetProperties(dynamic obj)
    {
        var propertyList = new List<KeyValuePair<string, object>>();

        // Get the type of the dynamic object
        var type = obj.GetType();

        // Retrieve the properties and their values using reflection
        foreach (var property in type.GetProperties())
        {
            var propertyName = property.Name;
            var propertyValue = property.GetValue(obj);

            propertyList.Add(new KeyValuePair<string, object>(propertyName, propertyValue));
        }

        return propertyList;
    }

    private Tuple<int, string> validateRows(dynamic dataObject)
    {
        List<string> validColumns = GetValidRows();
        Dictionary<string, object> temp = Dyn2Dict(dataObject);
        foreach (string property in temp.Keys)
        {
            if (!validColumns.Contains(property.ToLower()))
            {
                return new Tuple<int, string>(-1, property + " is not a valid field.");
            }
        }
        return new Tuple<int, string>(1, "");
    }

    public Dictionary<string, object> Dyn2Dict(dynamic dynObj)
    {
        var dictionary = new Dictionary<string, object>();
        foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynObj))
        {
            object obj = propertyDescriptor.GetValue(dynObj);
            dictionary.Add(propertyDescriptor.Name.ToLower(), obj); 
        }
        return dictionary;
    }

    public Case MapRowToCase(SqlDataReader reader)
    {
        Case caseObj = new Case();

        caseObj.CaseId = GetGuidValue(reader, "caseid");
        caseObj.IntakeDateTime = GetDateTimeValue(reader, "intakedatetime");
        caseObj.UserId = GetIntValue(reader, "userid");
        caseObj.IncidentDate = GetDateTimeValue(reader, "incidentdate");
        caseObj.CaseTypeId = GetGuidValue(reader, "casetypeid");
        caseObj.CaseStatus = GetString(reader, "casestatus");
        caseObj.AttorneyId = GetGuidValue(reader, "attorneyid");
        caseObj.CaseDescription = GetString(reader, "casedescription");
        caseObj.PassengerCount = GetIntValue(reader, "passengercount");
        caseObj.PcName = GetString(reader, "pcname");
        caseObj.PcPhone = GetString(reader, "pcphone");
        caseObj.PcEmail = GetString(reader, "pcemail");
        caseObj.PcContactName = GetString(reader, "pccontactname");
        caseObj.PcContactPhone = GetString(reader, "pccontactphone");
        caseObj.PcContactName2 = GetString(reader, "pccontactname2");
        caseObj.PcContactPhone2 = GetString(reader, "pccontactphone2");
        caseObj.PcBirthDate = GetString(reader, "pcbirthdate");
        caseObj.PcSsn = GetString(reader, "pcssn");
        caseObj.PcAddress = GetString(reader, "pcaddress");
        caseObj.PcCity = GetString(reader, "pccity");
        caseObj.PcState = GetString(reader, "pcstate");
        caseObj.PcZipCode = GetString(reader, "pczipcode");
        caseObj.PcInsuranceCompany = GetString(reader, "pcinsurancecompany");
        caseObj.PcClaimNo = GetString(reader, "pcclaimno");
        caseObj.IncidentLocation = GetString(reader, "incidentlocation");
        caseObj.PcAtFault = GetString(reader, "pcatfault");
        caseObj.PcInjured = GetString(reader, "pcinjured");
        caseObj.PcPursuingTreatment = GetString(reader, "pcpursuingtreatment");
        caseObj.PcWillingForTreatment = GetString(reader, "pcwillingfortreatment");
        

        return caseObj;
    }

    private Guid? GetGuidValue(SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return reader.IsDBNull(columnIndex) ? (Guid?)null : reader.GetGuid(columnIndex);
    }

    private DateTime? GetDateTimeValue(SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return reader.IsDBNull(columnIndex) ? (DateTime?)null : reader.GetDateTime(columnIndex);
    }

    private int? GetIntValue(SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return reader.IsDBNull(columnIndex) ? (int?)null : reader.GetInt32(columnIndex);
    }

    private string GetString(SqlDataReader reader, string columnName)
    {
        int columnIndex = reader.GetOrdinal(columnName);
        return reader.IsDBNull(columnIndex) ? "" : reader.GetString(columnIndex);
    }

    private List<string> GetValidRows()
    {
        List<string> caseInfoList = new List<string>();
        caseInfoList.Add("caseid");
        caseInfoList.Add("intakedatetime");
        caseInfoList.Add("userid");
        caseInfoList.Add("incidentdate");
        caseInfoList.Add("casetypeid");
        caseInfoList.Add("casestatus");
        caseInfoList.Add("attorneyid");
        caseInfoList.Add("casedescription");
        caseInfoList.Add("passengercount");
        caseInfoList.Add("pcname");
        caseInfoList.Add("pcphone");
        caseInfoList.Add("pcemail");
        caseInfoList.Add("pccontactname");
        caseInfoList.Add("pccontactphone");
        caseInfoList.Add("pccontactname2");
        caseInfoList.Add("pccontactphone2");
        caseInfoList.Add("pcbirthdate");
        caseInfoList.Add("pcssn");
        caseInfoList.Add("pcaddress");
        caseInfoList.Add("pccity");
        caseInfoList.Add("pcstate");
        caseInfoList.Add("pczipcode");
        caseInfoList.Add("pcinsurancecompany");
        caseInfoList.Add("pcclaimno");
        caseInfoList.Add("incidentlocation");
        caseInfoList.Add("pcatfault");
        caseInfoList.Add("pcinjured");
        caseInfoList.Add("pcpursuingtreatment");
        caseInfoList.Add("pcwillingfortreatment");
        caseInfoList.Add("pcemployer");
        caseInfoList.Add("investigatingpd");
        caseInfoList.Add("injuriesandtreatment");
        caseInfoList.Add("defendantname");
        caseInfoList.Add("defendantclaimno");
        caseInfoList.Add("propertydamagedescription");
        caseInfoList.Add("propertydamageamount");
        caseInfoList.Add("priorattorneyname");
        caseInfoList.Add("priorsettlement");
        caseInfoList.Add("addedby");
        caseInfoList.Add("addedon");
        caseInfoList.Add("updatedby");
        caseInfoList.Add("updatedon");
        caseInfoList.Add("campaignid");
        caseInfoList.Add("entrystatus");
        caseInfoList.Add("pcfirstname");
        caseInfoList.Add("pcmiddlename");
        caseInfoList.Add("pclastname");
        caseInfoList.Add("adverseinsurance");
        caseInfoList.Add("policereportno");
        caseInfoList.Add("pcvehicleinfo");
        caseInfoList.Add("pcvehiclelocation");
        caseInfoList.Add("attorneystatus");
        caseInfoList.Add("pchealthinsurance");
        caseInfoList.Add("calcdate");
        caseInfoList.Add("personticketed");
        caseInfoList.Add("personticketedcomments");
        caseInfoList.Add("attorneyfee");
        caseInfoList.Add("pcemployeraddress");
        caseInfoList.Add("pcemployercity");
        caseInfoList.Add("pcemployerstate");
        caseInfoList.Add("pcemployerzipcode");
        caseInfoList.Add("disengagereason");
        caseInfoList.Add("disengagenotes");
        caseInfoList.Add("disengagedatetime");
        caseInfoList.Add("defendantvehicleinfo");
        caseInfoList.Add("emaildatetime");
        caseInfoList.Add("overrideattorneyfee");
        caseInfoList.Add("affiliateid");
        caseInfoList.Add("pendingdisengagedatetime");
        caseInfoList.Add("accidentstate");
        caseInfoList.Add("overrideintakedatetime");
        caseInfoList.Add("resolvebillingnote");
        caseInfoList.Add("driverlicense");
        caseInfoList.Add("propertydamagepics");
        caseInfoList.Add("premiumcase");
        caseInfoList.Add("attcaseid");
        caseInfoList.Add("disputed");
        caseInfoList.Add("livetransfer");
        caseInfoList.Add("gclid");
        caseInfoList.Add("converted");
        caseInfoList.Add("notacaseReason");
        caseInfoList.Add("recaptime");
        caseInfoList.Add("matchtime");
        caseInfoList.Add("retainedtimestamp");
        caseInfoList.Add("rejectlead");
        caseInfoList.Add("rejectleadreason");
        caseInfoList.Add("rejectleaddate");
        caseInfoList.Add("rejectleadby");
        caseInfoList.Add("transfername");
        caseInfoList.Add("transfertime");
        caseInfoList.Add("api_key");
        return caseInfoList;
    }

    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}

