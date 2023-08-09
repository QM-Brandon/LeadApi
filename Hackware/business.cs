using System;
using System.Data;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Support.Data;
using Support.Miscellaneous;
using System.Collections.Generic;
using System.Linq;

namespace Support.Business
{
    /// <summary>
    /// Business object base class
    /// </summary>
    public class BusinessObject
    {
        private SQLDataAccess SQLData = new SQLDataAccess();

        private DataReaderSql SQLReader = new DataReaderSql();

        public TxnConnection TxnConnection
        {
            set
            {
                SQLData.Connection = value;
                SQLReader.Connection = value;
            }
        }

        private DataTable _dt = null;

        public DataTable dt
        {
            get { return this._dt; }
            set { this._dt = value; }
        }

        public bool HasRows
        {
            get { return dt != null && dt.Rows.Count > 0; }
        }

        public DataRow Row
        {
            get { return HasRows ? dt.Rows[0] : null; }
        }

        private string _tableName = "table";
        public string TableName
        {
            get { return this._tableName; }
            set { this._tableName = value; }
        }

        private string _PrimaryKey;
        public string PrimaryKey
        {
            get { return this._PrimaryKey; }
            set { this._PrimaryKey = value; }
        }

        private string _PrimaryKeySP;
        public string PrimaryKeySP
        {
            get { return this._PrimaryKeySP; }
            set { this._PrimaryKeySP = value; }
        }

        private string _fieldList = "*";

        public string FieldList
        {
            get { return this._fieldList; }
            set { this._fieldList = value; }
        }

        private int _RowsUpdated = 0;
        public int RowsUpdated
        {
            get { return this._RowsUpdated; }
            set { this._RowsUpdated = value; }
        }

        /// <summary>
		/// Collection of broken rules
		/// </summary>
		private List<string> _BrokenRules = new List<string>();
        public List<string> BrokenRules
        {
            get { return _BrokenRules; }
        }

        /// <summary>
		/// Required fields collection
		/// </summary>
		private List<string> _RequiredFields = new List<string>();
        public List<string> RequiredFields
        {
            get { return this._RequiredFields; }
        }

        private string _BrokenRulesHeader =
            "<a name='Errors'><h4>Please correct the following:</h4></a>";
        public string BrokenRulesHeader
        {
            get { return this._BrokenRulesHeader; }
            set { this._BrokenRulesHeader = value; }
        }

        /// <summary>
		/// The number of broken rules
		/// </summary>
		public int BrokenRuleCount
        {
            get { return BrokenRules.Count; }
        }

        /// <summary>
        /// Returns true if there are any broken rules.
        /// </summary>
        public bool HasBrokenRules
        {
            get { return BrokenRules.Count > 0; }
        }

        /// <summary>
        /// Active record column name. (Expected Values = "Active", "IsActive", "IsDeleted" etc)
        /// </summary>
        public string ActiveRecordColumnName { get; set; }

        /// <summary>
        /// Column Value in order to apply filter in order to get active only records.
        /// </summary>
        public string ActiveRecordsColumnValue { get; set; }

        /// <summary>
        /// Only Soft Delete objects
        /// </summary>
        public bool AllowOnlySoftDelete { get; set; }

        /// <summary>
        /// BusinessObject constructor
        /// </summary>
        public BusinessObject()
        {
        }

        public void LoadTableStructure()
        {
            LoadTableStructure(null);
        }

        /// <summary>
		/// Returns a table structure and bind it to the table in the class instance
		/// </summary>
		public void LoadTableStructure(string CommandText)
        {
            if (string.IsNullOrEmpty(CommandText))
            {
                CommandText = "SELECT top 0 " + this.FieldList + " FROM " + this.TableName;
            }

            dt = this.GetDataTable(CommandText);
        }

        public void GetRecordByID(int keyValue)
        {
            GetRecordByID(keyValue.ToString(), null, null);
        }

        public void GetRecordByID(int keyValue, string strKeyField)
        {
            GetRecordByID(keyValue.ToString(), strKeyField, null);
        }

        public void GetRecordByID(Guid keyValue)
        {
            GetRecordByID(keyValue.ToString(), null, null);
        }

        public void GetRecordByID(Guid keyValue, string strKeyField)
        {
            GetRecordByID(keyValue.ToString(), strKeyField, null);
        }

        public void GetRecordByID(String strKeyValue)
        {
            GetRecordByID(strKeyValue, null, null);
        }

        public void GetRecordByID(string strKeyValue, string strKeyField)
        {
            GetRecordByID(strKeyValue, strKeyField, null);
        }

        /// <summary>
		/// Returns a DataTable containing the record(s) with the specified key
		/// </summary>
		/// <param name="strprimarykey">Key Value for lookup</param>
		/// <param name="strkeyfield">Key Field for lookup (optional)</param>
		public void GetRecordByID(string strKeyValue, string strKeyField, string strSortBy)
        {
            if (string.IsNullOrEmpty(strKeyField))
            {
                strKeyField = this.PrimaryKey;
            }

            string FieldType = dt.Columns[strKeyField].DataType.ToString();
            string Command = "";
            switch (FieldType)
            {
                case "System.String":
                case "System.Guid":
                    Command = "SELECT " + this.FieldList + " FROM " + this.TableName + " WHERE " + strKeyField + "='" + strKeyValue + "'";
                    break;
                case "System.Int16":
                case "System.Int32":
                case "System.Decimal":
                    Command = "SELECT " + this.FieldList + " FROM " + this.TableName + " WHERE " + strKeyField + "=" + strKeyValue;
                    break;
            }

            if (!string.IsNullOrEmpty(strSortBy))
            {
                Command += " order by " + strSortBy;
            }

            dt = this.GetDataTable(Command);
        }

        /// <summary>
        /// Delete the records matching the specified primary key
        /// </summary>
        /// <param name="primarykey">Primary Key</param>
        /// <returns>Integer</returns>
        // Delete records with Integer key
        public bool DeleteRecordByID(int intID, string strKeyField)
        {
            bool boolSuccess = false;
            try
            {
                if (string.IsNullOrEmpty(strKeyField))
                {
                    strKeyField = this.PrimaryKey;
                }
                if (this.AllowOnlySoftDelete)
                {
                    string Command = "update " + this.TableName + " set " + (!string.IsNullOrEmpty(this.ActiveRecordColumnName) ? this.ActiveRecordColumnName : "active") + " = '0' where " + strKeyField + "=" + intID;

                    boolSuccess = (this.ExecuteSQL(Command) > -1);
                }
                else
                {
                    string Command = "delete from " + this.TableName + " where " + strKeyField + "=" + intID;
                    boolSuccess = (this.ExecuteSQL(Command) > -1);
                }


                if (!boolSuccess)
                {
                    this.AddBrokenRule("Unable to delete record from " + this.TableName + " for " + strKeyField + "=" + intID);
                }
            }
            catch (Exception ex)
            {
                boolSuccess = false;
                this.AddBrokenRule("Unable to delete record. Error: " + ex.Message);
            }

            return boolSuccess;
        }

        // Delete records with String key
        public bool DeleteRecordByID(string strID, string strKeyField, bool softDelete = false)
        {
            bool boolSuccess = false;
            try
            {
                if (string.IsNullOrEmpty(strKeyField))
                {
                    strKeyField = this.PrimaryKey;
                }
                if (softDelete || this.AllowOnlySoftDelete)
                {
                    string Command = "Update " + this.TableName + " set active=0 where " + strKeyField + "='" + strID + "'";

                    boolSuccess = (this.ExecuteSQL(Command) > -1);
                }
                else
                {
                    string Command = "delete from " + this.TableName + " where " + strKeyField + "='" + strID + "'";

                    boolSuccess = (this.ExecuteSQL(Command) > -1);

                }

                if (!boolSuccess)
                {
                    this.AddBrokenRule("Unable to delete record from " + this.TableName + " for " + strKeyField + "='" + strID + "'");
                }
            }
            catch (Exception ex)
            {
                boolSuccess = false;
                this.AddBrokenRule(ex.Message);
            }

            return boolSuccess;
        }


        /// <summary>
        /// Adds a blank row to the object's DataTable.
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <returns>New row number</returns>
        public int AddNewRow(bool GeneratePrimaryKey)
        {
            DataRow NewRow = dt.NewRow();
            if (GeneratePrimaryKey)
            {
                NewRow[this.PrimaryKey] = this.GetNewPrimaryKey();
            }
            else
            {
                string FieldType = dt.Columns[this.PrimaryKey].DataType.ToString();
                switch (FieldType)
                {
                    case "System.String":
                        //NewRow(Me.PrimaryKey) = ""
                        NewRow[this.PrimaryKey] = (dt.Rows.Count * -1).ToString();
                        // Attempt to assign a temporary unique value to the PK.
                        break;
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Decimal":
                        //NewRow(Me.PrimaryKey) = 0
                        NewRow[this.PrimaryKey] = (dt.Rows.Count * -1);
                        // Attempt to assign a temporary unique value to the PK.
                        break;
                }
            }

            this.SetFieldDefaults(NewRow);
            dt.Rows.Add(NewRow);

            return dt.Rows.Count - 1;
        }

        public int AddNewRow()
        {
            return AddNewRow(false);
        }

        /// <summary>
        /// Delete the selected row from the object's DataTable.
        /// </summary>
        /// <returns>New row number</returns>
        //Public Overloads Function DeleteRow(Optional ByVal CommitDeletion As Boolean = False) As Boolean
        //    Return DeleteRow(0, CommitDeletion)
        //End Function
        public bool DeleteRow(bool CommitDeletion, int Rowno, bool DeleteAllRows = false)
        {
            bool bSuccess = true;
            try
            {
                if (this.AllowOnlySoftDelete)
                {
                    if (dt.Columns.Contains("active"))
                    {
                        if (DeleteAllRows)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                row["active"] = false;
                            }

                        }
                        else
                        {
                            dt.Rows[Rowno]["active"] = false;
                        }

                        if (CommitDeletion)
                        {
                            bSuccess = SaveDataTable(true);
                        }
                        if (DeleteAllRows)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                row.Delete();
                            }
                        }
                        else
                        {
                            dt.Rows[Rowno].Delete();
                        }

                    }

                }
                else if (this.ChildrenCanBeDeleted(dt.Rows[Rowno]))
                {
                    if (this.DeleteChildren(dt.Rows[Rowno]))
                    {
                        if (DeleteAllRows)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                row.Delete();
                            }
                        }
                        else
                        {
                            dt.Rows[Rowno].Delete();
                        }

                        if (CommitDeletion)
                        {
                            bSuccess = SaveDataTable(true);
                        }
                    }
                    else
                    {
                        bSuccess = false;
                    }
                }
                else
                {
                    bSuccess = false;
                }
            }
            catch (Exception ex)
            {
                bSuccess = false;
                this.AddBrokenRule(ex.Message);
            }

            return bSuccess;
        }

        public bool DeleteRow(bool CommitDeletion, bool deleteAllRows = false)
        {
            return DeleteRow(CommitDeletion, 0, deleteAllRows);
        }

        public virtual bool ChildrenCanBeDeleted(DataRow row)
        {
            bool bSuccess = true;
            return bSuccess;
        }

        public virtual bool DeleteChildren(DataRow row)
        {
            bool bSuccess = true;
            return bSuccess;
        }

        /// <summary>
        /// Execute the selected query.
        /// </summary>
        /// <returns>Return results of query</returns>
        public int ExecuteSQL(string SQLCode)
        {
            int intResult = -1;
            try
            {
                intResult = SQLReader.SQLExecuteNonQuery(SQLCode);
            }
            catch (Exception ex)
            {
                intResult = -1;
                this.AddBrokenRule(ex.Message);
            }

            return intResult;
        }

        /// <summary>
        /// Execute the selected query.
        /// </summary>
        /// <returns>Return results of query</returns>
        public int ExecuteScalar(string SQLCode)
        {
            int intResult = -1;
            try
            {
                intResult = Convert.ToInt32(SQLReader.SQLExecuteScalar(SQLCode));
                if (intResult < 0)
                {
                    this.AddBrokenRule("Unable to execute SQL code.<br>SQL code=" + SQLCode);
                }
            }
            catch (Exception ex)
            {
                intResult = -1;
                this.AddBrokenRule(ex.Message);
            }

            return intResult;
        }

        /// <summary>
        /// Execute the selected query.
        /// </summary>
        /// <returns>Return results of query</returns>
        public string ExecuteScalarString(string SQLCode)
        {
            string result = null;
            try
            {
                result = SQLReader.SQLExecuteScalarString(SQLCode);
                if (result == null)
                {
                    this.AddBrokenRule("Unable to execute SQL code.<br>SQL code=" + SQLCode);
                }
            }
            catch (Exception ex)
            {
                result = null;
                this.AddBrokenRule(ex.Message);
            }

            return result;
        }

        public int InsertAndReturnKey()
        {
            return InsertAndReturnKey(false);
        }

        public int InsertAndReturnKey(bool BypassValidation)
        {
            // This function adds a new record and retrieves the PK for the new record.

            int intNewKey = -1;
            bool BusinessRulesPassed = true;
            if (!BypassValidation)
            {
                //ClearBrokenRules() 'Clear any broken rules
                this.CheckRequiredFields(dt);
                // Check for any required fields
                this.CheckRulesHook(dt);
                // Check the business rules
                BusinessRulesPassed = this.BrokenRuleCount == 0;
            }

            if (BusinessRulesPassed)
            {
                // Update the DataTable
                try
                {
                    StringBuilder sbFields = new StringBuilder();
                    StringBuilder sbValues = new StringBuilder();
                    string strField = null;
                    string strPrefix = "";

                    foreach (DataColumn col in dt.Columns)
                    {
                        strField = col.ColumnName;
                        // Only insert if not primary key field.
                        if (!strField.Equals(PrimaryKey, StringComparison.InvariantCultureIgnoreCase) &&
                            !Utilities.IsDBNull(dt.Rows[0][strField]))
                        {
                            sbFields.Append(strPrefix + strField);
                            sbValues.Append(strPrefix + "'" + dt.Rows[0][strField].ToString().Replace("'", "''") + "'");
                            if (string.IsNullOrEmpty(strPrefix))
                            {
                                strPrefix = ",";
                            }
                        }
                    }

                    string strSQLCode = "insert into " + TableName + " (" + sbFields.ToString() + ")" +
                        " values (" + sbValues.ToString() + ");\r\n" +
                        " select scope_identity()";

                    intNewKey = this.ExecuteScalar(strSQLCode);
                }
                catch (Exception ex)
                {
                    this.AddBrokenRule("Unable to insert new record: " + ex.Message);
                    intNewKey = -1;
                }
            }
            else
            {
                // Indicate one or more rules is broken
                intNewKey = -1;
            }

            return intNewKey;
        }

        // Check for records on file for selected Query
        public bool RecordsOnFile(string strSQLCode)
        {
            bool boolRecordsOnFile = false;
            try
            {
                Support.Business.BusinessObject oData = new Support.Business.BusinessObject();
                DataTable dt = oData.GetDataTable(strSQLCode);
                boolRecordsOnFile = (dt.Rows.Count > 0);
            }
            catch (Exception ex)
            {
                boolRecordsOnFile = true;
            }

            return boolRecordsOnFile;
        }

        /// <summary>
        /// Function overridden in BusinessObjects used to set field defaults for new records
        /// </summary>
        public virtual void SetFieldDefaults(DataRow Row)
        {
        }

        /// <summary>
        /// Returns the next Location number to be used for a new, blank Location record
        /// </summary>
        /// <returns>String</returns>
        public string GetNewPrimaryKey()
        {
            if (string.IsNullOrEmpty(this.PrimaryKeySP))
            {
                return "";
            }
            else
            {
                Support.Business.BusinessObject oData = new Support.Business.BusinessObject();
                DataTable dt = oData.GetDataTable("exec " + this.PrimaryKeySP);
                string newnum = (string)dt.Rows[0][0];
                return (newnum);
            }
        }


        /// <summary>
        /// Returns a DataTable containing data returned by executing the specified command
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <returns></returns>
        public DataTable GetDataTable(string Command)
        {
            return GetDataTable(Command, true);
        }

        /// <summary>
        /// Returns a DataTable containing data returned by executing the specified command
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <param name="supressException">Determines whether this function returns an empty datatable or throws an exception upon exception.</param>
        /// <returns></returns>
        public DataTable GetDataTable(string Command, bool supressException)
        {
            if (string.IsNullOrEmpty(Command))
            {
                Command = "SELECT " + this.FieldList + " FROM " + TableName;

                if (!string.IsNullOrEmpty(ActiveRecordsColumnValue) && !string.IsNullOrEmpty(ActiveRecordColumnName))
                {
                    Command += " WHERE " + ActiveRecordColumnName + " = '" + ActiveRecordsColumnValue + "'";
                }
            }

            return SQLData.GetDataTable(Command, supressException);
        }

        /// <summary>
        /// Loads a DataTable into the current object containing data returned by executing the specified command
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <returns></returns>
        public void LoadDataTable(string Command)
        {
            if (string.IsNullOrEmpty(Command))
            {
                Command = "SELECT " + this.FieldList + " FROM " + TableName;

                if (!string.IsNullOrEmpty(ActiveRecordsColumnValue) && !string.IsNullOrEmpty(ActiveRecordColumnName))
                {
                    Command += " WHERE " + ActiveRecordColumnName + " = '" + ActiveRecordsColumnValue + "'";
                }
            }

            dt = SQLData.GetDataTable(Command);

            return;
        }

        public bool SaveDataTable()
        {
            return SaveDataTable(false);
        }

        /// <summary>
        /// Saves the object's DataTable
        /// </summary>
        /// <param name="BypassValidation">Bypasses the validation routine</param>
        /// <returns>Number of rows affected. Returns -1 if business rules failed</returns>
        public virtual bool SaveDataTable(bool BypassValidation)
        {
            //As Integer
            bool BusinessRulesPassed = true;
            if (!BypassValidation)
            {
                //ClearBrokenRules() 'Clear any broken rules
                this.CheckRequiredFields(dt);
                // Check for any required fields
                this.CheckRulesHook(dt);
                // Check the business rules
                BusinessRulesPassed = this.BrokenRuleCount == 0;
            }

            if (BusinessRulesPassed)
            {
                // Update the date/time of last update
                bool bAddLogged = dt.Columns.Contains("AddedOn");
                bool bUpdateLogged = dt.Columns.Contains("UpdatedOn");
                DateTime dtCurrent = DateTime.Now;
                int intUserid = Support.Miscellaneous.Utilities.GetUserID();
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr.RowState != DataRowState.Deleted)
                    {
                        if (bAddLogged && dr.RowState == DataRowState.Added)
                        {
                            dr["AddedOn"] = dtCurrent;
                            dr["AddedBy"] = intUserid;
                        }

                        if (bUpdateLogged && (
                            dr.RowState == DataRowState.Added ||
                            dr.RowState == DataRowState.Modified))
                        {
                            dr["UpdatedOn"] = dtCurrent;
                            dr["UpdatedBy"] = intUserid;
                        }
                    }
                }

                // Update the DataTable
                try
                {
                    this.RowsUpdated = SQLData.SaveDataTable(dt);
                }
                catch (Exception ex)
                {
                    this.AddBrokenRule("Unable to save record: " + ex.Message +
                        (ex.InnerException != null ? " -> " + ex.InnerException.Message : ""));

                    this.RowsUpdated = -1;
                }
            }
            else
            {
                // Indicate one or more rules is broken
                this.RowsUpdated = -1;
            }

            return (this.RowsUpdated >= 0);
        }

        /// <summary>
        /// Sets the selected item for a Checkbox instance
        /// </summary>
        /// <param name="CheckboxRef">CheckboxRef</param>
        /// <param name="ColumnName">String</param>
        /// <returns>Nothing</returns>
        public void LoadFormVar(CheckBox CheckboxRef, string ColumnName, int RowNo)
        {
            try
            {
                DataRow dr = dt.Rows[RowNo];
                if (Utilities.IsDBNull(dr[ColumnName]))
                {
                    CheckboxRef.Checked = false;
                }
                else
                {
                    CheckboxRef.Checked = (bool)dr[ColumnName];
                }
            }
            catch (Exception ex)
            {
                CheckboxRef.Checked = false;
            }

            SetClass(CheckboxRef, false);
        }

        public void LoadFormVar(CheckBox CheckboxRef, string ColumnName)
        {
            LoadFormVar(CheckboxRef, ColumnName, 0);
        }

        /// <summary>
        /// Loads a value from a Datarow column into a hidden form var
        /// </summary>
        /// <param name="HtmlInputHidden">Inputhidden</param>
        /// <param name="dr">DataRow</param>
        /// <param name="ColumnName">String</param>
        /// <returns>Nothing</returns>
        public void LoadFormVar(HtmlInputHidden Inputhidden, string ColumnName, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            string FieldType = dr[ColumnName].GetType().ToString();
            switch (FieldType)
            {
                case "System.String":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Decimal":
                    Inputhidden.Value = dr[ColumnName].ToString().Trim();
                    break;
                case "System.DateTime":
                    Inputhidden.Value = ((DateTime)dr[ColumnName]).ToString("MM/dd/yyyy");
                    break;
                default:
                    Inputhidden.Value = "";
                    break;
            }
        }

        public void LoadFormVar(HtmlInputHidden Inputhidden, string ColumnName)
        {
            LoadFormVar(Inputhidden, ColumnName, 0);
        }

        /// <summary>
        /// Loads a value from a Datarow column into a hidden form var
        /// </summary>
        /// <param name="HiddenField">Hiddenfield</param>
        /// <param name="dr">DataRow</param>
        /// <param name="ColumnName">String</param>
        /// <returns>Nothing</returns>
        public void LoadFormVar(HiddenField Hiddenfield, string ColumnName, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            string FieldType = dr[ColumnName].GetType().ToString();
            switch (FieldType)
            {
                case "System.String":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Decimal":
                    Hiddenfield.Value = dr[ColumnName].ToString().Trim();
                    break;
                case "System.DateTime":
                    Hiddenfield.Value = ((DateTime)dr[ColumnName]).ToString("MM/dd/yyyy");
                    break;
                default:
                    Hiddenfield.Value = "";
                    break;
            }
        }

        public void LoadFormVar(HiddenField Hiddenfield, string ColumnName)
        {
            LoadFormVar(Hiddenfield, ColumnName, 0);
        }

        /// <summary>
		/// Loads a value from a Datarow column into a Textbox form var
		/// </summary>
		/// <param name="Textbox">Textbox</param>
		/// <param name="dr">DataRow</param>
		/// <param name="ColumnName">String</param>
		/// <returns>Nothing</returns>
		public void LoadFormVar(ref string VarRef, string ColumnName, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            string FieldType = dr[ColumnName].GetType().ToString();
            switch (FieldType)
            {
                case "System.String":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Decimal":
                case "System.Double":
                    VarRef = dr[ColumnName].ToString().Trim();
                    break;
                case "System.DateTime":
                    VarRef = ((DateTime)dr[ColumnName]).ToShortDateString();
                    break;
                default:
                    VarRef = "";
                    break;
            }
        }

        public void LoadFormVar(ref string VarRef, string ColumnName)
        {
            LoadFormVar(ref VarRef, ColumnName, 0);
        }

        /// <summary>
        /// Loads a value from a Datarow column into a Textbox form var
        /// </summary>
        /// <param name="Textbox">Textbox</param>
        /// <param name="dr">DataRow</param>
        /// <param name="ColumnName">String</param>
        /// <returns>Nothing</returns>
        public void LoadFormVar(TextBox Textbox, string ColumnName, int RowNo)
        {
            LoadFormVar(Textbox, ColumnName, RowNo, null);
        }

        public void LoadFormVar(TextBox Textbox, string ColumnName)
        {
            LoadFormVar(Textbox, ColumnName, 0, null);
        }

        public void LoadFormVar(TextBox Textbox, string ColumnName, string format)
        {
            LoadFormVar(Textbox, ColumnName, 0, format);
        }

        public void LoadFormVar(TextBox Textbox, string ColumnName, int RowNo, string format)
        {
            DataRow dr = dt.Rows[RowNo];
            string FieldType = dr[ColumnName].GetType().ToString();
            switch (FieldType)
            {
                case "System.String":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Decimal":
                case "System.Double":
                    Textbox.Text = dr[ColumnName].ToString().Trim();
                    break;
                case "System.DateTime":
                    Textbox.Text = ((DateTime)dr[ColumnName]).ToString(format != null ? format : "MM/dd/yyyy");
                    break;
                default:
                    Textbox.Text = "";
                    break;
            }

            SetClass(Textbox, false);
        }

        /// <summary>
        /// Loads a value from a Datarow column into a Label form var
        /// </summary>
        /// <param name="Label">Label</param>
        /// <param name="dr">DataRow</param>
        /// <param name="ColumnName">String</param>
        /// <returns>Nothing</returns>
        public void LoadFormVar(Label Label, string ColumnName, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            string FieldType = dr[ColumnName].GetType().ToString();
            switch (FieldType)
            {
                case "System.String":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Decimal":
                case "System.Double":
                    Label.Text = dr[ColumnName].ToString().Trim();
                    break;
                case "System.DateTime":
                    Label.Text = ((DateTime)dr[ColumnName]).ToShortDateString();
                    break;
                default:
                    Label.Text = "";
                    break;
            }
        }

        /// <summary>
		/// Sets the selected item for a Dropdown List instance
		/// </summary>
		/// <param name="DropDownRef">DropDownList</param>
		/// <param name="dr">DataRow</param>
		/// <param name="ColumnName">String</param>
		/// <returns>Nothing</returns>
		public void LoadFormVar(DropDownList DropDownRef, string ColumnName, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            try
            {
                DropDownRef.SelectedValue = dr[ColumnName].ToString();
            }
            catch (Exception ex)
            {
                DropDownRef.SelectedIndex = 0;
            }

            SetClass(DropDownRef, false);
        }

        public void LoadFormVar(DropDownList DropDownRef, string ColumnName)
        {
            LoadFormVar(DropDownRef, ColumnName, 0);
        }

        /// <summary>
		/// Sets the selected item for a RadioButtonList instance
		/// </summary>
		/// <param name="rbl">A RadioButtonList instance</param>
		/// <param name="ColumnName">The column name used to set the value of the RadioButtonList</param>
		/// <param name="RowNo">Row number</param>
		/// <returns>Nothing</returns>
        public void LoadFormVar(RadioButtonList rbl, string ColumnName, int RowNo = 0)
        {
            DataRow dr = dt.Rows[RowNo];
            try
            {
                rbl.SelectedValue = dr[ColumnName].ToString();
            }
            catch (Exception ex)
            {
                rbl.SelectedIndex = 0;
            }

            SetClass(rbl, false);
        }

        /// <summary>
		/// Sets the selected item for a CheckBoxList instance
		/// </summary>
		/// <param name="rbl">A CheckBoxList instance</param>
		/// <param name="ColumnName">The column name used to set the value of the CheckBoxList</param>
		/// <param name="RowNo">Row number</param>
		/// <returns>Nothing</returns>
        public void LoadFormVar(CheckBoxList cbl, string ColumnName, int RowNo = 0)
        {
            DataRow dr = dt.Rows[RowNo];
            string value = dr[ColumnName] as string ?? "";
            string[] values = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                cbl.Items.Cast<ListItem>().ToList().ForEach(i =>
                {
                    if (values.Contains(i.Value)) i.Selected = true;
                });
            }
            catch (Exception ex)
            {
                cbl.SelectedIndex = -1;
            }

            SetClass(cbl, false);
        }

        /// <summary>
		/// Sets the selected item for a group of 3 Radio Buttons
		/// </summary>
		/// <param name="Rb1">RadioButton</param>
		/// <param name="Rb2">RadioButton</param>
		/// <param name="Rb3">RadioButton</param>
		/// <param name="dr">DataRow</param>
		/// <param name="ColumnName">String</param>
		/// <returns>Nothing</returns>
		public void LoadFormVar(RadioButton Rb1, RadioButton Rb2, string ColumnName, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            try
            {
                if ((int)dr[ColumnName] == 1)
                {
                    Rb1.Checked = true;
                    Rb2.Checked = false;
                }
                else if ((int)dr[ColumnName] == 2)
                {
                    Rb1.Checked = false;
                    Rb2.Checked = true;
                }
                else
                {
                    Rb1.Checked = false;
                    Rb2.Checked = false;
                }
            }
            catch (Exception ex)
            {
                Rb1.Checked = false;
                Rb2.Checked = false;
            }
        }

        /// <summary>
        /// Sets the selected item for a group of 3 Radio Buttons
        /// </summary>
        /// <param name="Rb1">RadioButton</param>
        /// <param name="Rb2">RadioButton</param>
        /// <param name="Rb3">RadioButton</param>
        /// <param name="dr">DataRow</param>
        /// <param name="ColumnName">String</param>
        /// <returns>Nothing</returns>
        public void LoadFormVar(RadioButton Rb1, RadioButton Rb2, RadioButton Rb3, string ColumnName, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            try
            {
                if ((int)dr[ColumnName] == 1)
                {
                    Rb1.Checked = true;
                    Rb2.Checked = false;
                    Rb3.Checked = false;
                }
                else if ((int)dr[ColumnName] == 2)
                {
                    Rb1.Checked = false;
                    Rb2.Checked = true;
                    Rb3.Checked = false;
                }
                else if ((int)dr[ColumnName] == 3)
                {
                    Rb1.Checked = false;
                    Rb2.Checked = false;
                    Rb3.Checked = true;
                }
                else
                {
                    Rb1.Checked = false;
                    Rb2.Checked = false;
                    Rb3.Checked = false;
                }
            }
            catch (Exception ex)
            {
                Rb1.Checked = false;
                Rb2.Checked = false;
                Rb3.Checked = false;
            }
        }

        /// <summary>
        /// Builds and save a date value into a Datarow column
        /// </summary>
        /// <param name="dr">Datarow</param>
        /// <param name="ColumnName">String</param>
        /// <param name="Dateref">Textbox</param>
        /// <param name="Hourref">Dropdownlist</param>
        /// <param name="Minuteref">Dropdownlist</param>
        /// <param name="Ampmref">Dropdownlist</param>
        /// <param name="Required">Boolean</param>
        /// <param name="ColumnDesc">String</param>
        /// <returns>Nothing</returns>
        //public bool SaveDateFormVar(
        //    string ColumnName, TextBox Dateref, DropDownList Hourref, DropDownList Minuteref, DropDownList Ampmref,
        //    string ColumnDesc, bool Required, int RowNo)
        //{
        //    if (string.IsNullOrEmpty(ColumnDesc)) ColumnDesc = "";

        //    bool ErrorFound = false;
        //    DataRow dr = dt.Rows[RowNo];
        //    try
        //    {
        //        if (!Required && string.IsNullOrEmpty(Dateref.Text.Trim()) && string.IsNullOrEmpty(Hourref.SelectedValue.Trim()) &&
        //            string.IsNullOrEmpty(Minuteref.SelectedValue.Trim()) && string.IsNullOrEmpty(Ampmref.SelectedValue.Trim()))
        //        {
        //            // Not required and date is blank
        //            dr[ColumnName] = DBNull.Value;
        //            //CDate(Nothing)
        //        }
        //        else
        //        {
        //            // Validate date portion
        //            if (string.IsNullOrEmpty(Dateref.Text.Trim()))
        //            {
        //                if (Required)
        //                {
        //                    ErrorFound = true;
        //                    this.AddBrokenRule(ColumnDesc + " Date is a required field");
        //                }
        //            }
        //            else if (!Utilities.IsDate(Dateref.Text.Trim()))
        //            {
        //                if (Required)
        //                {
        //                    ErrorFound = true;
        //                    this.AddBrokenRule(ColumnDesc + " Date is invalid");
        //                }
        //            }

        //            // Validate hour
        //            if (string.IsNullOrEmpty(Hourref.SelectedValue.Trim()))
        //            {
        //                if (Required)
        //                {
        //                    ErrorFound = true;
        //                    this.AddBrokenRule(ColumnDesc + " Hour is a required field");
        //                }
        //            }

        //            // Validate minute
        //            if (string.IsNullOrEmpty(Minuteref.SelectedValue.Trim()))
        //            {
        //                if (Required)
        //                {
        //                    ErrorFound = true;
        //                    this.AddBrokenRule(ColumnDesc + " Minute is a required field");
        //                }
        //            }

        //            // Validate AMPM
        //            if (string.IsNullOrEmpty(Ampmref.SelectedValue.Trim()))
        //            {
        //                if (Required)
        //                {
        //                    ErrorFound = true;
        //                    this.AddBrokenRule(ColumnDesc + " am/pm is a required field");
        //                }
        //            }

        //            if (!ErrorFound)
        //            {
        //                DateTime myDatetime = System.DateTime.Parse(Dateref.Text + " " + Hourref.SelectedValue + ":" +
        //                    Minuteref.SelectedValue + " " + Ampmref.SelectedValue);

        //                dr[ColumnName] = myDatetime;
        //            }
        //        }
        //    }
        //    catch (Exception ex) {
        //        this.AddBrokenRule(ColumnDesc + " is invalid");
        //        ErrorFound = true;
        //    }

        //    SetClass(Dateref, ErrorFound);
        //    SetClass(Hourref, ErrorFound);
        //    SetClass(Minuteref, ErrorFound);
        //    SetClass(Ampmref, ErrorFound);

        //    return !ErrorFound;
        //}

        /// <summary>
        /// Builds and save a date value into a Datarow column
        /// </summary>
        /// <param name="dr">Datarow</param>
        /// <param name="ColumnName">String</param>
        /// <param name="Dateref">Textbox</param>
        /// <param name="Hourref">Dropdownlist</param>
        /// <param name="Minuteref">Dropdownlist</param>
        /// <param name="Ampmref">Dropdownlist</param>
        /// <param name="Required">Boolean</param>
        /// <param name="ColumnDesc">String</param>
        /// <returns>Nothing</returns>
        public bool SaveDateFormVar(
            string ColumnName, TextBox Dateref, TextBox Timeref, string ColumnDesc, bool DateRequired, bool TimeRequired, int RowNo)
        {
            if (string.IsNullOrEmpty(ColumnDesc)) ColumnDesc = "";

            bool DateErrorFound = false;
            bool TimeErrorFound = false;
            string strDate = Dateref.Text.Trim();
            string strTime = Timeref.Text.Trim();
            DataRow dr = dt.Rows[RowNo];
            try
            {
                if (!(DateRequired | TimeRequired) && string.IsNullOrEmpty((strDate + strTime).Trim()))
                {
                    // Not required and date and time is blank
                    dr[ColumnName] = DBNull.Value;
                }
                else
                {
                    // Validate date portion
                    if (string.IsNullOrEmpty(strDate))
                    {
                        if ((DateRequired | TimeRequired))
                        {
                            DateErrorFound = true;
                            this.AddBrokenRule(ColumnDesc + " Date is a required field");
                        }
                    }
                    else if (!Utilities.IsDate(strDate))
                    {
                        DateErrorFound = true;
                        this.AddBrokenRule(ColumnDesc + " Date entered is invalid");
                    }

                    // Validate time portion
                    if (string.IsNullOrEmpty(strTime))
                    {
                        if (TimeRequired)
                        {
                            TimeErrorFound = true;
                            this.AddBrokenRule(ColumnDesc + " Time is a required field");
                        }
                    }
                    else
                    {
                        if (!Utilities.IsDate(strTime))
                        {
                            TimeErrorFound = true;
                            this.AddBrokenRule(ColumnDesc + " Time is invalid");
                        }
                    }
                    if (!DateErrorFound & !TimeErrorFound)
                    {
                        if (string.IsNullOrEmpty(strTime))
                        {
                            dr[ColumnName] = DateTime.Parse(strDate + " 12:01 AM");
                        }
                        else
                        {
                            dr[ColumnName] = DateTime.Parse(strDate + " " + strTime);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.AddBrokenRule(ColumnDesc + " is invalid");
                DateErrorFound = true;
                TimeErrorFound = true;
            }

            SetClass(Dateref, DateErrorFound);
            SetClass(Timeref, TimeErrorFound);

            return !(DateErrorFound & TimeErrorFound);
        }

        /// <summary>
        /// Saves a value from a Checkbox form var into a Datarow column
        /// </summary>
        /// <param name="CheckboxRef">CheckBox</param>
        /// <param name="ColumnName">String</param>
        /// <param name="Required">Boolean</param>
        /// <returns>Nothing</returns>
        public void SaveFormVar(CheckBox CheckboxRef, string ColumnName, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            dr.SaveFormVar(CheckboxRef, ColumnName, this.BrokenRules);

            //bool ErrorFound = false;
            //try
            //{
            //    dr[ColumnName] = CheckboxRef.Checked;
            //}
            //catch (Exception ex)
            //{
            //    this.AddBrokenRule("Unexpected error with " + CheckboxRef.ID.ToString());
            //    ErrorFound = true;
            //}

            //SetClass(CheckboxRef, ErrorFound);
        }

        public void SaveFormVar(CheckBox CheckboxRef, string ColumnName)
        {
            SaveFormVar(CheckboxRef, ColumnName, 0);
        }

        /// <summary>
        /// Saves a value from a Text String into a Datarow column
        /// </summary>
        /// <param name="TextStr">String</param>
        /// <param name="dr">DataRow</param>
        /// <param name="ColumnName">String</param>
        /// <param name="Required">Boolean</param>
        /// <returns>Nothing</returns>
        public void SaveFormVar(string TextStr, string ColumnName, string ColumnDesc, bool Required, int RowNo)
        {
            if (string.IsNullOrEmpty(ColumnDesc)) ColumnDesc = "";

            DataRow dr = dt.Rows[RowNo];
            bool ErrorFound = false;
            string FieldType = dt.Columns[ColumnName].DataType.ToString();
            string CSSClass = "";
            bool IsEmpty = false;
            try
            {
                switch (FieldType)
                {
                    case "System.String":
                    case "System.Guid":
                        dr[ColumnName] = TextStr;
                        IsEmpty = string.IsNullOrEmpty(dr[ColumnName].ToString().Trim());
                        break;

                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        //if (string.IsNullOrEmpty(TextStr))
                        //                  {
                        //	dr[ColumnName] = 0;
                        //}
                        //                  else
                        //                  {
                        //	dr[ColumnName] = Convert.ToInt32(TextStr);
                        //}

                        //IsEmpty = (int)dr[ColumnName] == 0;
                        if (string.IsNullOrEmpty(TextStr))
                        {
                            dr[ColumnName] = DBNull.Value;
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToInt32(TextStr);
                            IsEmpty = false;
                        }
                        break;

                    case "System.TimeSpan":
                        if (string.IsNullOrEmpty(TextStr))
                        {
                            dr[ColumnName] = DBNull.Value;
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = TimeSpan.Parse(TextStr);
                            IsEmpty = false;
                        }
                        break;

                    case "System.DateTime":
                        if (string.IsNullOrEmpty(TextStr))
                        {
                            dr[ColumnName] = DBNull.Value;
                            //CDate("01/01/1900")
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToDateTime(TextStr);
                            IsEmpty = false;
                        }
                        break;

                    case "System.Decimal":
                        //dr[ColumnName] = Convert.ToString(TextStr);
                        //IsEmpty = Convert.ToDecimal(dr[ColumnName]) == 0;
                        if (string.IsNullOrEmpty(TextStr))
                        {
                            dr[ColumnName] = DBNull.Value;
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToString(TextStr);
                            IsEmpty = false;
                        }
                        break;

                    case "System.Double":
                        //dr[ColumnName] = Convert.ToString(TextStr);
                        //IsEmpty = Convert.ToDouble(dr[ColumnName]) == 0;
                        if (string.IsNullOrEmpty(TextStr))
                        {
                            dr[ColumnName] = DBNull.Value;
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToString(TextStr);
                            IsEmpty = false;
                        }
                        break;

                    default:
                        this.AddBrokenRule(ColumnName + ", Unhandled field type: " + FieldType);
                        ErrorFound = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                this.AddBrokenRule(ColumnDesc.ToString() + " is invalid: " + ex.Message);
                ErrorFound = true;
            }

            if (Required & IsEmpty)
            {
                this.AddBrokenRule(ColumnDesc.ToString() + " is a required field");
                ErrorFound = true;
            }
            //SetClass(TextStr, ErrorFound)
        }

        public void SaveFormVar(string TextStr, string ColumnName, string ColumnDesc)
        {
            SaveFormVar(TextStr, ColumnName, ColumnDesc, false, 0);
        }

        public void SaveFormVar(string TextStr, string ColumnName, string ColumnDesc, bool Required)
        {
            SaveFormVar(TextStr, ColumnName, ColumnDesc, Required, 0);
        }

        /// <summary>
        /// Saves a value from a hidden form var into a Datarow column
        /// </summary>
        /// <param name="HiddenField">Hiddenfield</param>
        /// <param name="dr">DataRow</param>
        /// <param name="ColumnName">String</param>
        /// <param name="Required">Boolean</param>
        /// <returns>Nothing</returns>
        public void SaveFormVar(HtmlInputHidden Inputhidden, string ColumnName, string ColumnDesc, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            bool ErrorFound = false;
            string FieldType = dt.Columns[ColumnName].DataType.ToString();
            bool IsEmpty = false;
            try
            {
                switch (FieldType)
                {
                    case "System.Guid":
                        if (string.IsNullOrEmpty(Inputhidden.Value.Trim()))
                        {
                            dr[ColumnName] = DBNull.Value;
                        }
                        else
                        {
                            dr[ColumnName] = Inputhidden.Value;
                        }

                        IsEmpty = string.IsNullOrEmpty(dr[ColumnName].ToString().Trim());
                        break;

                    case "System.String":
                        dr[ColumnName] = Inputhidden.Value;
                        IsEmpty = string.IsNullOrEmpty(dr[ColumnName].ToString().Trim());
                        break;

                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        if (string.IsNullOrEmpty(Inputhidden.Value))
                        {
                            dr[ColumnName] = 0;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToInt32(Inputhidden.Value);
                        }
                        IsEmpty = (int)dr[ColumnName] == 0;
                        break;

                    case "System.Decimal":
                        if (string.IsNullOrEmpty(Inputhidden.Value))
                        {
                            dr[ColumnName] = 0;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToDecimal(Inputhidden.Value);
                        }

                        IsEmpty = (decimal)dr[ColumnName] == 0;
                        break;

                    case "System.DateTime":
                        if (string.IsNullOrEmpty(Inputhidden.Value))
                        {
                            dr[ColumnName] = DBNull.Value;
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToDateTime(Inputhidden.Value);
                            IsEmpty = false;
                        }
                        break;

                    default:
                        this.AddBrokenRule(ColumnName + ", Unhandled field type: " + FieldType);
                        ErrorFound = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                this.AddBrokenRule(ColumnDesc.ToString() + " is invalid");
                ErrorFound = true;
            }

        }

        public void SaveFormVar(HtmlInputHidden Inputhidden, string ColumnName, string ColumnDesc)
        {
            SaveFormVar(Inputhidden, ColumnName, ColumnDesc, 0);
        }

        /// <summary>
        /// Saves a value from a hidden form var into a Datarow column
        /// </summary>
        /// <param name="HiddenField">Hiddenfield</param>
        /// <param name="dr">DataRow</param>
        /// <param name="ColumnName">String</param>
        /// <param name="Required">Boolean</param>
        /// <returns>Nothing</returns>
        public void SaveFormVar(HiddenField Hiddenfield, string ColumnName, string ColumnDesc, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            bool ErrorFound = false;
            string FieldType = dt.Columns[ColumnName].DataType.ToString();
            bool IsEmpty = false;
            try
            {
                switch (FieldType)
                {
                    case "System.Guid":
                        if (string.IsNullOrEmpty(Hiddenfield.Value.Trim()))
                        {
                            dr[ColumnName] = DBNull.Value;
                        }
                        else
                        {
                            dr[ColumnName] = Hiddenfield.Value;
                        }

                        IsEmpty = string.IsNullOrEmpty(dr[ColumnName].ToString().Trim());
                        break;

                    case "System.String":
                        dr[ColumnName] = Hiddenfield.Value;
                        IsEmpty = string.IsNullOrEmpty(dr[ColumnName].ToString().Trim());
                        break;

                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        if (string.IsNullOrEmpty(Hiddenfield.Value))
                        {
                            dr[ColumnName] = 0;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToInt32(Hiddenfield.Value);
                        }
                        IsEmpty = (int)dr[ColumnName] == 0;
                        break;

                    case "System.Decimal":
                        if (string.IsNullOrEmpty(Hiddenfield.Value))
                        {
                            dr[ColumnName] = 0;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToDecimal(Hiddenfield.Value);
                        }

                        IsEmpty = (decimal)dr[ColumnName] == 0;
                        break;

                    case "System.DateTime":
                        if (string.IsNullOrEmpty(Hiddenfield.Value))
                        {
                            dr[ColumnName] = DBNull.Value;
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToDateTime(Hiddenfield.Value);
                            IsEmpty = false;
                        }
                        break;

                    default:
                        this.AddBrokenRule(ColumnName + ", Unhandled field type: " + FieldType);
                        ErrorFound = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                this.AddBrokenRule(ColumnDesc.ToString() + " is invalid");
                ErrorFound = true;
            }

        }

        public void SaveFormVar(HiddenField Hiddenfield, string ColumnName, string ColumnDesc)
        {
            SaveFormVar(Hiddenfield, ColumnName, ColumnDesc, 0);
        }

        /// <summary>
		/// Saves a value from a Textbox form var into a Datarow column
		/// </summary>
		/// <param name="Textbox">Textbox</param>
		/// <param name="dr">DataRow</param>
		/// <param name="ColumnName">String</param>
		/// <param name="Required">Boolean</param>
		/// <returns>Nothing</returns>
		public void SaveFormVar(TextBox Textbox, string ColumnName, string ColumnDesc, bool Required, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            dr.SaveFormVar(Textbox, ColumnName, ColumnDesc, Required, this.BrokenRules);
        }

        public void SaveFormVar(TextBox Textbox, string ColumnName, string ColumnDesc)
        {
            SaveFormVar(Textbox, ColumnName, ColumnDesc, false);
        }

        public void SaveFormVar(TextBox Textbox, string ColumnName, string ColumnDesc, bool Required)
        {
            SaveFormVar(Textbox, ColumnName, ColumnDesc, Required, 0);
        }

        /// <summary>
        /// Saves a value from a DropdownList form var into a Datarow column
        /// </summary>
        /// <param name="DropDownRef">DropDownList</param>
        /// <param name="dr">DataRow</param>
        /// <param name="ColumnName">String</param>
        /// <param name="Required">Boolean</param>
        /// <returns>Nothing</returns>
        public void SaveFormVar(DropDownList DropDownRef, string ColumnName, string ColumnDesc, bool Required, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            dr.SaveFormVar(DropDownRef, ColumnName, ColumnDesc, Required, this.BrokenRules);
        }

        public void SaveFormVar(DropDownList DropDownRef, string ColumnName, string ColumnDesc)
        {
            SaveFormVar(DropDownRef, ColumnName, ColumnDesc, false);
        }

        public void SaveFormVar(DropDownList DropDownRef, string ColumnName, string ColumnDesc, bool Required)
        {
            SaveFormVar(DropDownRef, ColumnName, ColumnDesc, Required, 0);
        }

        public void SaveFormVar(RadioButtonList rbl, string ColumnName, string ColumnDesc, bool Required, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            dr.SaveFormVar(rbl, ColumnName, ColumnDesc, Required, this.BrokenRules);
        }

        public void SaveFormVar(RadioButtonList rbl, string ColumnName, string ColumnDesc, bool Required)
        {
            SaveFormVar(rbl, ColumnName, ColumnDesc, Required, 0);
        }

        public void SaveFormVar(CheckBoxList cbl, string ColumnName, string ColumnDesc, bool Required, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            dr.SaveFormVar(cbl, ColumnName, ColumnDesc, Required, this.BrokenRules);
        }

        public void SaveFormVar(CheckBoxList cbl, string ColumnName, string ColumnDesc, bool Required)
        {
            SaveFormVar(cbl, ColumnName, ColumnDesc, Required, 0);
        }

        /// <summary>
		/// Saves the selected value from a group of 3 Radio Buttons into a Datarow column
		/// </summary>
		/// <param name="Rb1">RadioButton</param>
		/// <param name="Rb2">RadioButton</param>
		/// <param name="dr">DataRow</param>
		/// <param name="ColumnName">String</param>
		/// <param name="Required">Boolean</param>
		/// <returns>Nothing</returns>
		public void SaveFormVar(RadioButton Rb1, RadioButton Rb2, string ColumnName, string ColumnDesc, bool Required, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            bool ErrorFound = false;
            bool IsEmpty = false;
            string FieldType = dt.Columns[ColumnName].DataType.ToString();

            try
            {
                if (Rb1.Checked)
                {
                    dr[ColumnName] = 1;
                }
                else if (Rb2.Checked)
                {
                    dr[ColumnName] = 2;
                }
                else
                {
                    dr[ColumnName] = 0;
                }

                IsEmpty = Convert.ToInt32(dr[ColumnName]) == 0;
            }
            catch (Exception ex)
            {
                this.AddBrokenRule("Unexpected error with " + ColumnDesc.ToString());
                ErrorFound = true;
            }

            if (Required && IsEmpty)
            {
                this.AddBrokenRule(ColumnDesc.ToString() + " is a required field");
                ErrorFound = true;
            }

            SetClass(Rb1, ErrorFound);
            SetClass(Rb2, ErrorFound);
        }

        /// <summary>
        /// Saves the selected value from a group of 3 Radio Buttons into a Datarow column
        /// </summary>
        /// <param name="Rb1">RadioButton</param>
        /// <param name="Rb2">RadioButton</param>
        /// <param name="Rb3">RadioButton</param>
        /// <param name="dr">DataRow</param>
        /// <param name="ColumnName">String</param>
        /// <param name="Required">Boolean</param>
        /// <returns>Nothing</returns>
        public void SaveFormVar(RadioButton Rb1, RadioButton Rb2, RadioButton Rb3, string ColumnName, string ColumnDesc, bool Required, int RowNo)
        {
            DataRow dr = dt.Rows[RowNo];
            bool ErrorFound = false;
            bool IsEmpty = false;
            string FieldType = dt.Columns[ColumnName].DataType.ToString();

            try
            {
                if (Rb1.Checked)
                {
                    dr[ColumnName] = 1;
                }
                else if (Rb2.Checked)
                {
                    dr[ColumnName] = 2;
                }
                else if (Rb3.Checked)
                {
                    dr[ColumnName] = 3;
                }
                else
                {
                    dr[ColumnName] = 0;
                }

                IsEmpty = Convert.ToInt32(dr[ColumnName]) == 0;
            }
            catch (Exception ex)
            {
                this.AddBrokenRule("Unexpected error with " + ColumnDesc.ToString());
                ErrorFound = true;
            }

            if (Required && IsEmpty)
            {
                this.AddBrokenRule(ColumnDesc.ToString() + " is a required field");
                ErrorFound = true;
            }

            SetClass(Rb1, ErrorFound);
            SetClass(Rb2, ErrorFound);
            SetClass(Rb3, ErrorFound);
        }

        public void SetClass(System.Web.UI.WebControls.WebControl oObject, bool lFlagAsError)
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
		/// Add the specified broken rule to the broken rule collection if it's not already there
		/// </summary>
		/// <param name="BrokenRule">The broken rule to be added to the collection</param>
		public void AddBrokenRule(string brokenRule)
        {
            /// Only add the broken rule if it's not already there
            if (!BrokenRules.Contains(brokenRule))
            {
                BrokenRules.Add(brokenRule);
            }
        }

        /// <summary>
        /// Checks if all required fields are entered for the current DataTable referenced by dt.
        /// </summary>
        /// <returns>True if all fields are entered, otherwise False</returns>
        public bool CheckRequiredFields()
        {
            return CheckRequiredFields(this.dt);
        }

        /// <summary>
		/// Checks if all required fields are entered for the provided DataTable
		/// </summary>
		/// <param name="dt">DataTable</param>
		/// <returns>True if all fields are entered, otherwise False</returns>
		public bool CheckRequiredFields(DataTable dt)
        {
            if (this.RequiredFields == null || this.RequiredFields.Count == 0) return true;

            // Get the first DataRow
            foreach (DataRow dr in dt.Rows)
            {
                if (!(dr.RowState == DataRowState.Deleted))
                {
                    string Field = null;
                    foreach (string Field_loopVariable in this.RequiredFields)
                    {
                        Field = Field_loopVariable;
                        string FieldName = "";
                        string FieldDesc = "";

                        int commapos = Field.IndexOf(",");
                        if (commapos > 0)
                        {
                            FieldName = Field.Substring(0, commapos).Trim();
                            FieldDesc = Field.Substring((commapos + 1)).Trim();
                        }
                        else
                        {
                            FieldName = Field;
                            FieldDesc = Field;
                        }

                        bool IsEmpty = false;
                        string FieldType = dr[FieldName].GetType().ToString();

                        switch (FieldType)
                        {
                            case "System.String":
                                IsEmpty = string.IsNullOrEmpty(dr[FieldName].ToString().Trim());
                                break;
                            case "System.Int16":
                            case "System.Int32":
                            case "System.Int64":
                                //IsEmpty = Convert.ToInt32(dr[FieldName]) == 0;
                                IsEmpty = string.IsNullOrEmpty(dr[FieldName].ToString());
                                break;
                            case "System.DateTime":
                                IsEmpty = string.IsNullOrEmpty(dr[FieldName].ToString());
                                break;
                            case "System.Decimal":
                                //IsEmpty = Convert.ToDecimal(dr[FieldName]) == 0;
                                IsEmpty = string.IsNullOrEmpty(dr[FieldName].ToString());
                                break;
                            case "System.Double":
                                //IsEmpty = Convert.ToDouble(dr[FieldName]) == 0;
                                IsEmpty = string.IsNullOrEmpty(dr[FieldName].ToString());
                                break;
                            case "System.Guid":
                                Guid? fieldValue = dr[FieldName] as Guid?;
                                IsEmpty = !fieldValue.HasValue || fieldValue.Value == Guid.Empty;
                                break;
                            case "System.DBNull":
                                IsEmpty = true;
                                break;

                            default:
                                IsEmpty = false;
                                break;
                        }

                        if (IsEmpty)
                        {
                            this.AddBrokenRule(FieldDesc.ToString() + " is a required field");
                        }
                    }
                }
            }

            return this.BrokenRuleCount == 0;
        }

        /// <summary>
        /// Validate the data for the DataTable currently referenced by dt.
        /// </summary>
        public void CheckRulesHook()
        {
            CheckRulesHook(this.dt);
        }

        /// <summary>
		/// Provides a hook method into which developers can place
		/// application-specific code to check rules
		/// </summary>
		/// <returns>Logical true if no broken rules, otherwise, false</returns>
		public virtual void CheckRulesHook(DataTable dt)
        {
        }

        /// <summary>
		/// Clears the broken rules collection
		/// </summary>
		public void ClearBrokenRules()
        {
            BrokenRules.Clear();
        }

        /// <summary>
        /// Removes the specified broken rule from the collection
        /// </summary>
        /// <param name="?">Broken rule string to be cleared</param>
        public void ClearRule(string brokenRule)
        {
            BrokenRules.Remove(brokenRule);
        }

        /// <summary>
        /// Returns the nth broken rule specified by the index number
        /// </summary>
        /// <param name="index">Index number of broken rule to be returned</param>
        /// <returns>The nth broken rule</returns>
        public string GetBrokenRule(int index)
        {
            return BrokenRules[index];
        }

        /// <summary>
		/// Returns the list of broken rule messages in an unordered list
		/// </summary>
		/// <returns>The entire broken rule message list</returns>
		public string GetBrokenRuleList(bool SuppressHeader)
        {
            StringBuilder BrokenRuleList = new StringBuilder();
            if (this.BrokenRuleCount > 0)
            {
                if (!SuppressHeader)
                {
                    BrokenRuleList.Append(BrokenRulesHeader);
                }

                BrokenRuleList.Append("<ul>\r\n");
                int xx = 0;
                for (xx = 0; xx <= this.BrokenRuleCount - 1; xx++)
                {
                    BrokenRuleList.Append("<li>" + GetBrokenRule(xx).ToString() + "</li>\r\n");
                }

                BrokenRuleList.Append("</ul>\r\n");
            }

            return BrokenRuleList.ToString();
        }

        public string GetBrokenRuleList()
        {
            return GetBrokenRuleList(false);
        }

        public bool IsFieldRequired(string fieldName)
        {
            return RequiredFields.FirstOrDefault(
                x => x.ToLower().StartsWith(fieldName.ToLower())) != null;
        }
    }

    public static class FieldDataExtensions
    {
        public static void SaveFormVar(this DataRow dr, TextBox textBox,
            string ColumnName, string ColumnDesc = null, bool Required = false, List<string> brokenRules = null)
        {
            if (string.IsNullOrEmpty(ColumnDesc)) ColumnDesc = ColumnName;

            bool ErrorFound = false;
            dr.SetColumnError(ColumnName, null);

            string FieldType = dr.Table.Columns[ColumnName].DataType.ToString();
            bool IsEmpty = false;
            try
            {
                switch (FieldType)
                {
                    case "System.Guid":
                        if (string.IsNullOrEmpty(textBox.Text.Trim()))
                        {
                            dr[ColumnName] = DBNull.Value;
                        }
                        else
                        {
                            dr[ColumnName] = textBox.Text;
                        }

                        IsEmpty = string.IsNullOrEmpty(dr[ColumnName].ToString().Trim());
                        break;

                    case "System.String":
                        dr[ColumnName] = textBox.Text;
                        IsEmpty = string.IsNullOrEmpty(dr[ColumnName].ToString().Trim());
                        break;

                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        //if (string.IsNullOrEmpty(textBox.Text))
                        //{
                        //    dr[ColumnName] = 0;
                        //}
                        //else
                        //{
                        //    dr[ColumnName] = Convert.ToInt32(textBox.Text);
                        //}
                        //IsEmpty = (int)dr[ColumnName] == 0;
                        //break;

                        if (string.IsNullOrEmpty(textBox.Text))
                        {
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToInt32(textBox.Text);
                        }
                        break;

                    case "System.Decimal":
                        //if (string.IsNullOrEmpty(textBox.Text))
                        //{
                        //    dr[ColumnName] = 0;
                        //}
                        //else
                        //{
                        //    dr[ColumnName] = Convert.ToDecimal(textBox.Text);
                        //}

                        //IsEmpty = (decimal)dr[ColumnName] == 0;
                        //break;

                        if (string.IsNullOrEmpty(textBox.Text))
                        {
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToDecimal(textBox.Text);
                        }
                        break;

                    case "System.Double":
                        //if (string.IsNullOrEmpty(textBox.Text))
                        //{
                        //    dr[ColumnName] = 0.0;
                        //}
                        //else
                        //{
                        //    dr[ColumnName] = Convert.ToDouble(textBox.Text);
                        //}

                        //IsEmpty = (double)dr[ColumnName] == 0.0;
                        //break;

                        if (string.IsNullOrEmpty(textBox.Text))
                        {
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToDouble(textBox.Text);
                        }
                        break;

                    case "System.DateTime":
                        if (string.IsNullOrEmpty(textBox.Text))
                        {
                            dr[ColumnName] = DBNull.Value;
                            IsEmpty = true;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToDateTime(textBox.Text);
                            // Prohibit dates more than 8 months into the future
                            if (((DateTime)dr[ColumnName] - DateTime.Now).Days >= (8 * 30))
                            {
                                if (brokenRules != null) brokenRules.Add(ColumnName + ", Date is greater than 8 months in the future.");
                                ErrorFound = true;
                            }
                            IsEmpty = false;
                        }
                        break;

                    default:
                        string error = ColumnName + ", Unhandled field type: " + FieldType;
                        if (brokenRules != null) brokenRules.Add(error);
                        dr.SetColumnError(ColumnName, error);
                        ErrorFound = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                string error = ColumnDesc.ToString() + " is invalid: " + ex.Message;
                if (brokenRules != null) brokenRules.Add(error);
                dr.SetColumnError(ColumnName, error);
                ErrorFound = true;
            }

            if (Required && IsEmpty)
            {
                string error = ColumnDesc.ToString() + " is a required field";
                if (brokenRules != null) brokenRules.Add(error);
                dr.SetColumnError(ColumnName, error);
                ErrorFound = true;
            }

            textBox.SetErrorClass(ErrorFound);
        }

        public static void SaveFormVar(this DataRow dr, DropDownList DropDownRef,
            string ColumnName, string ColumnDesc, bool Required, List<string> brokenRules = null)
        {
            bool ErrorFound = false;
            bool IsEmpty = false;
            string FieldType = dr.Table.Columns[ColumnName].DataType.ToString();

            try
            {
                switch (FieldType)
                {
                    case "System.String":
                        dr[ColumnName] = DropDownRef.SelectedValue;
                        IsEmpty = string.IsNullOrEmpty(dr[ColumnName].ToString().Trim());
                        break;

                    case "System.Guid":
                        Guid fieldValue = Guid.Empty;
                        if (Guid.TryParse(DropDownRef.SelectedValue.Trim(), out fieldValue))
                        {
                            dr[ColumnName] = fieldValue;
                        }
                        else
                        {
                            dr[ColumnName] = DBNull.Value;
                        }

                        IsEmpty = fieldValue == Guid.Empty;

                        //if (string.IsNullOrEmpty(DropDownRef.SelectedValue.Trim()))
                        //{
                        //    dr[ColumnName] = DBNull.Value;
                        //}
                        //else
                        //{
                        //    dr[ColumnName] = DropDownRef.SelectedValue;
                        //}
                        //IsEmpty = string.IsNullOrEmpty(dr[ColumnName].ToString().Trim());
                        break;

                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        if (string.IsNullOrEmpty(DropDownRef.SelectedValue.Trim()))
                        {
                            dr[ColumnName] = 0;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToInt32(DropDownRef.SelectedValue.Trim());
                        }
                        IsEmpty = (int)dr[ColumnName] == 0;
                        break;

                    case "System.Decimal":
                        if (string.IsNullOrEmpty(DropDownRef.SelectedValue.Trim()))
                        {
                            dr[ColumnName] = 0;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToDecimal(DropDownRef.SelectedValue);
                        }
                        IsEmpty = (decimal)dr[ColumnName] == 0;
                        break;
                    case "System.Double":
                        if (string.IsNullOrEmpty(DropDownRef.SelectedValue.Trim()))
                        {
                            dr[ColumnName] = 0.0;
                        }
                        else
                        {
                            dr[ColumnName] = Convert.ToDouble(DropDownRef.SelectedValue);
                        }
                        IsEmpty = (double)dr[ColumnName] == 0.0;
                        break;
                }
            }
            catch (Exception ex)
            {
                if (brokenRules != null) brokenRules.Add("Unexpected error with " + ColumnDesc.ToString() + ", type=" + FieldType + "<br />Error: " + ex.Message);
                ErrorFound = true;
            }
            if (Required && IsEmpty)
            {
                if (brokenRules != null) brokenRules.Add(ColumnDesc.ToString() + " is a required field");
                ErrorFound = true;
            }

            DropDownRef.SetErrorClass(ErrorFound);
        }

        public static void SaveFormVar(this DataRow dr, CheckBox CheckboxRef, string ColumnName, List<string> brokenRules = null)
        {
            bool ErrorFound = false;
            try
            {
                dr[ColumnName] = CheckboxRef.Checked;
            }
            catch (Exception ex)
            {
                if (brokenRules != null) brokenRules.Add("Unexpected error with " + CheckboxRef.ID.ToString());
                ErrorFound = true;
            }

            CheckboxRef.SetErrorClass(ErrorFound);
        }

        public static void SaveFormVar(this DataRow dr, RadioButtonList rbl,
            string columnName, string columnLabel = null, bool required = false, List<string> brokenRules = null)
        {
            if (string.IsNullOrEmpty(columnLabel)) columnLabel = columnName;

            bool ErrorFound = false;
            bool IsEmpty = false;
            string FieldType = dr.Table.Columns[columnName].DataType.ToString();

            try
            {
                switch (FieldType)
                {
                    case "System.String":
                        dr[columnName] = rbl.SelectedValue;
                        IsEmpty = string.IsNullOrEmpty(dr[columnName].ToString().Trim());
                        break;

                    case "System.Guid":
                        if (string.IsNullOrEmpty(rbl.SelectedValue.Trim()))
                        {
                            dr[columnName] = DBNull.Value;
                        }
                        else
                        {
                            dr[columnName] = rbl.SelectedValue;
                        }
                        IsEmpty = string.IsNullOrEmpty(dr[columnName].ToString().Trim());
                        break;

                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        if (string.IsNullOrEmpty(rbl.SelectedValue.Trim()))
                        {
                            dr[columnName] = 0;
                        }
                        else
                        {
                            dr[columnName] = Convert.ToInt32(rbl.SelectedValue.Trim());
                        }
                        IsEmpty = (int)dr[columnName] == 0;
                        break;

                    case "System.Decimal":
                        if (string.IsNullOrEmpty(rbl.SelectedValue.Trim()))
                        {
                            dr[columnName] = 0;
                        }
                        else
                        {
                            dr[columnName] = Convert.ToDecimal(rbl.SelectedValue);
                        }
                        IsEmpty = (decimal)dr[columnName] == 0;
                        break;
                    case "System.Double":
                        if (string.IsNullOrEmpty(rbl.SelectedValue.Trim()))
                        {
                            dr[columnName] = 0.0;
                        }
                        else
                        {
                            dr[columnName] = Convert.ToDouble(rbl.SelectedValue);
                        }
                        IsEmpty = (double)dr[columnName] == 0.0;
                        break;
                }
            }
            catch (Exception ex)
            {
                if (brokenRules != null) brokenRules.Add("Unexpected error with " + columnLabel + ", type=" + FieldType + "<br />Error: " + ex.Message);
                ErrorFound = true;
            }
            if (required && IsEmpty)
            {
                if (brokenRules != null) brokenRules.Add(columnLabel + " is a required field");
                ErrorFound = true;
            }

            rbl.SetErrorClass(ErrorFound);
        }

        public static void SaveFormVar(this DataRow dr, CheckBoxList cbl,
            string columnName, string columnLabel = null, bool required = false, List<string> brokenRules = null)
        {
            if (string.IsNullOrEmpty(columnLabel)) columnLabel = columnName;

            bool ErrorFound = false;
            bool IsEmpty = false;
            string FieldType = dr.Table.Columns[columnName].DataType.ToString();

            string value = null;
            if (cbl.SelectedIndex > -1)
            {
                value = string.Join(",",
                    cbl.Items.Cast<ListItem>().Where(li => li.Selected).Select(li => li.Value));
            }

            try
            {
                switch (FieldType)
                {
                    case "System.String":
                        dr[columnName] = value ?? (object)DBNull.Value;
                        IsEmpty = string.IsNullOrEmpty(value);
                        break;

                    case "System.Guid":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                    case "System.Decimal":
                    case "System.Double":
                        if (brokenRules != null) brokenRules.Add(string.Format("Invalid field type ({0}) for {1}.", FieldType, columnLabel));
                        ErrorFound = true;
                        IsEmpty = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                if (brokenRules != null) brokenRules.Add("Unexpected error with " + columnLabel + ", type=" + FieldType + "<br />Error: " + ex.Message);
                ErrorFound = true;
            }
            if (required && IsEmpty)
            {
                if (brokenRules != null) brokenRules.Add(columnLabel + " is a required field");
                ErrorFound = true;
            }

            cbl.SetErrorClass(ErrorFound);
        }

        public static void SaveFormVar(this DataRow dr, RadioButton[] radioButtons,
            string ColumnName, string ColumnDesc, bool Required, List<string> brokenRules = null)
        {
            bool ErrorFound = false;
            bool IsEmpty = false;
            string FieldType = dr.Table.Columns[ColumnName].DataType.ToString();

            try
            {
                if (radioButtons != null)
                {
                    foreach (var rb in radioButtons)
                    {
                        if (rb.Checked)
                        {
                            dr[ColumnName] = rb.Text;
                            break;
                        }
                    }
                }

                IsEmpty = dr.IsNull(ColumnName);
            }
            catch (Exception ex)
            {
                if (brokenRules != null) brokenRules.Add("Unexpected error with " + ColumnDesc.ToString());
                ErrorFound = true;
            }

            if (Required && IsEmpty)
            {
                if (brokenRules != null) brokenRules.Add(ColumnDesc.ToString() + " is a required field");
                ErrorFound = true;
            }

            foreach (var rb in radioButtons) rb.SetErrorClass(ErrorFound);
        }

        public static void SetErrorClass(this WebControl oObject, bool lFlagAsError)
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
    }
}