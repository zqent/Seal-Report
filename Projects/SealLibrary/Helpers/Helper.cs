﻿//
// Copyright (c) Seal Report, Eric Pfirsch (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. http://www.apache.org/licenses/LICENSE-2.0..
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using Seal.Model;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Web;
using System.Windows.Forms.DataVisualization.Charting;
using RazorEngine;
using RazorEngine.Templating;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Xml.Serialization;
using System.DirectoryServices.Protocols;
using System.Xml.Linq;
using System.ServiceModel.Syndication;

namespace Seal.Helpers
{
    public class Helper
    {
        public static string GetEnumDescription(Type type, Object value)
        {
            FieldInfo fi = type.GetField(Enum.GetName(type, value));
            DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
            if (dna != null) return dna.Description;
            else return value.ToString();
        }

        static public void CopyProperties(object src, object dest)
        {
            foreach (PropertyDescriptor item in TypeDescriptor.GetProperties(src))
            {
                item.SetValue(dest, item.GetValue(src));
            }
        }

        static public string QuoteDouble(string input)
        {
            if (input == null) input = "";
            return string.Format("\"{0}\"", input.Replace("\"", "\"\""));
        }

        static public string QuoteSingle(string input)
        {
            if (input == null) input = "";
            return string.Format("'{0}'", input.Replace("'", "''"));
        }


        static public string AddIfNotEmpty(string prefix, string input, string suffix)
        {
            if (!string.IsNullOrEmpty(input)) return prefix + input + suffix;
            return "";
        }

        static public string AddNotEmpty(string separator, string input)
        {
            if (!string.IsNullOrEmpty(input)) return separator + input;
            return "";
        }

        static public string AddNotEmpty2(string input, string separator)
        {
            if (!string.IsNullOrEmpty(input)) return input + separator;
            return "";
        }

        static public void AddValue(ref string input, string separator, string value)
        {
            if (!string.IsNullOrEmpty(input)) input += separator + value;
            else input = value;
        }

        static public void AddValue(ref StringBuilder input, string separator, string value)
        {
            if (input.Length > 0) input.Append(separator + value);
            else input = new StringBuilder(value);
        }

        static public string ConcatCellValues(ResultCell[] cells, string separator)
        {
            string result = "";
            foreach (var cell in cells) Helper.AddValue(ref result, separator, cell.ValueNoHTML);
            return result;
        }

        static public string IfNullOrEmpty(string value, string defaultValue)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;
            return value;
        }

        static public string ToHtml(string value)
        {
            if (value != null) return HttpUtility.HtmlEncode(value).Replace("\r\n", "<br>").Replace("\n", "<br>");
            return "";
        }

        static public string ToJS(bool value)
        {
            return value.ToString().ToLower();
        }

        static public string ToJS(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return HttpUtility.JavaScriptStringEncode(value);
        }


        static public string ToMomentJSFormat(string datetimeFormat)
        {
            return datetimeFormat.Replace("y", "Y").Replace("d", "D").Replace("tt", "A").Replace("z", "Z");
        }

        static public string RemoveHTMLTags(string value)
        {
            return Regex.Replace(value, @"<[^>]+>|&nbsp;", "").Trim();
        }

        static public string GetUniqueName(string name, List<string> entities)
        {
            string result;
            int i = 1;
            while (true)
            {
                result = name;
                if (i != 1) result += i.ToString();
                if (!entities.Contains(result)) break;
                i++;
            }
            return result;
        }

        static public string DBNameToDisplayName(string name)
        {
            string result = name;
            result = string.Join(" ", Regex.Split(result, @"([A-Z][a-z]+)"));
            result = result.Replace('_', ' ').Trim();
            result = result.Replace("  ", " ");
            if (result.Length > 0) result = result.Substring(0, 1).ToUpper() + result.Substring(1);
            return result.Trim();
        }

        static public ColumnType DatabaseToNetTypeConverter(object dbValue)
        {
            ColumnType result = ColumnType.Text;
            try
            {
                OleDbType columnType = (OleDbType)Convert.ToInt32(dbValue);
                result = Helper.NetTypeConverter(Helper.OleDbToNetTypeConverter(columnType));
                if (columnType == OleDbType.WChar || columnType == OleDbType.VarWChar || columnType == OleDbType.LongVarWChar) result = ColumnType.UnicodeText;
            }
            catch { }
            return result;
        }

        static public ColumnType ODBCToNetTypeConverter(string odbcType)
        {
            ColumnType result = ColumnType.Text;
            try
            {
                result = Helper.OdbcTypeConverter(odbcType);
            }
            catch { }
            return result;
        }

        static public ColumnType OdbcTypeConverter(string dbType)
        {
            string t = dbType.ToLower();

            if (t == "char" || t == "varchar" || t == "varchar2" || t == "text" || t == "uniqueidentifier") return ColumnType.Text;
            if (t == "nchar" || t == "ntext" || t == "nvarchar") return ColumnType.UnicodeText;
            if (t == "date" || t == "datetime" || t == "smalldatetime" || t == "timestamp") return ColumnType.DateTime;
            return ColumnType.Numeric;
        }

        static public Type OleDbToNetTypeConverter(OleDbType oleDbTypeNumber)
        {
            switch ((int)oleDbTypeNumber)
            {
                case 0: return typeof(Nullable);
                case 2: return typeof(Int16);
                case 3: return typeof(Int32);
                case 4: return typeof(Single);
                case 5: return typeof(Double);
                case 6: return typeof(Decimal);
                case 7: return typeof(DateTime);
                case 8: return typeof(String);
                case 9: return typeof(Object);
                case 10: return typeof(Exception);
                case 11: return typeof(Boolean);
                case 12: return typeof(Object);
                case 13: return typeof(Object);
                case 14: return typeof(Decimal);
                case 16: return typeof(SByte);
                case 17: return typeof(Byte);
                case 18: return typeof(UInt16);
                case 19: return typeof(UInt32);
                case 20: return typeof(Int64);
                case 21: return typeof(UInt64);
                case 64: return typeof(DateTime);
                case 72: return typeof(Guid);
                case 128: return typeof(Byte[]);
                case 129: return typeof(String);
                case 130: return typeof(String);
                case 131: return typeof(Decimal);
                case 133: return typeof(DateTime);
                case 134: return typeof(TimeSpan);
                case 135: return typeof(DateTime);
                case 138: return typeof(Object);
                case 139: return typeof(Decimal);
                case 200: return typeof(String);
                case 201: return typeof(String);
                case 202: return typeof(String);
                case 203: return typeof(String);
                case 204: return typeof(Byte[]);
                case 205: return typeof(Byte[]);
            }
            throw (new Exception("DataType Not Supported"));
        }

        static public ColumnType NetTypeConverter(Type netType)
        {
            if (netType == typeof(string)) return ColumnType.Text;
            if (netType == typeof(DateTime)) return ColumnType.DateTime;
            return ColumnType.Numeric;
        }

        public static List<String> GetSystemDriverList()
        {
            List<string> names = new List<string>();
            // get system dsn's
            Microsoft.Win32.RegistryKey reg = (Microsoft.Win32.Registry.LocalMachine).OpenSubKey("Software");
            if (reg != null)
            {
                reg = reg.OpenSubKey("ODBC");
                if (reg != null)
                {
                    reg = reg.OpenSubKey("ODBCINST.INI");
                    if (reg != null)
                    {

                        reg = reg.OpenSubKey("ODBC Drivers");
                        if (reg != null)
                        {
                            // Get all DSN entries defined in DSN_LOC_IN_REGISTRY.
                            foreach (string sName in reg.GetValueNames())
                            {
                                names.Add(sName);
                            }
                        }
                        try
                        {
                            reg.Close();
                        }
                        catch { /* ignore this exception if we couldn't close */ }
                    }
                }
            }

            return names;
        }
        static public string ConvertNumericStandardFormat(NumericStandardFormat format)
        {
            if (format == NumericStandardFormat.Numeric0) return "N0";
            else if (format == NumericStandardFormat.Numeric1) return "N1";
            else if (format == NumericStandardFormat.Numeric2) return "N2";
            else if (format == NumericStandardFormat.Numeric3) return "N3";
            else if (format == NumericStandardFormat.Numeric4) return "N4";
            else if (format == NumericStandardFormat.Percentage0) return "P0";
            else if (format == NumericStandardFormat.Percentage1) return "P1";
            else if (format == NumericStandardFormat.Percentage2) return "P2";
            else if (format == NumericStandardFormat.Currency0) return "C0";
            else if (format == NumericStandardFormat.Currency2) return "C2";
            else if (format == NumericStandardFormat.Decimal) return "D";
            else if (format == NumericStandardFormat.Decimal0) return "D0";
            else if (format == NumericStandardFormat.Decimal1) return "D1";
            else if (format == NumericStandardFormat.Decimal2) return "D2";
            else if (format == NumericStandardFormat.Decimal3) return "D3";
            else if (format == NumericStandardFormat.Decimal4) return "D4";
            else if (format == NumericStandardFormat.Exponential) return "E";
            else if (format == NumericStandardFormat.Exponential2) return "E2";
            else if (format == NumericStandardFormat.Fixedpoint) return "F";
            else if (format == NumericStandardFormat.Fixedpoint2) return "F2";
            else if (format == NumericStandardFormat.General) return "G";
            else if (format == NumericStandardFormat.General2) return "G2";
            else if (format == NumericStandardFormat.General5) return "G5";
            else if (format == NumericStandardFormat.Hexadecimal) return "H";
            else if (format == NumericStandardFormat.Hexadecimal8) return "H8";

            return "0";
        }

        static public string ConvertDateTimeStandardFormat(DateTimeStandardFormat format)
        {
            if (format == DateTimeStandardFormat.ShortDate) return "d";
            else if (format == DateTimeStandardFormat.LongDate) return "D";
            else if (format == DateTimeStandardFormat.ShortTime) return "t";
            else if (format == DateTimeStandardFormat.LongTime) return "T";
            else if (format == DateTimeStandardFormat.ShortDateTime) return "g";
            else if (format == DateTimeStandardFormat.LongDateTime) return "G";
            else if (format == DateTimeStandardFormat.FullShortDateTime) return "f";
            else if (format == DateTimeStandardFormat.FullLongDateTime) return "F";
            return "0";
        }

        public static bool CanDragAndDrop(DragEventArgs e)
        {
            return (e.Data.GetDataPresent(typeof(TreeNode)) && ((TreeNode)e.Data.GetData(typeof(TreeNode))).Tag is MetaColumn) || e.Data.GetDataPresent(typeof(Button));
        }

        static public string GetExceptionMessage(TemplateCompilationException ex)
        {
            var result = new StringBuilder("");
            foreach (var err in ex.CompilerErrors)
            {
                result.AppendFormat("{0}\r\nLine {1} Column {2} Error Number {3}\r\n", err.ErrorText, err.Line, err.Column, err.ErrorNumber);
            }
            return result.ToString();
        }

        static public Series CloneSeries(Series o)
        {
            PropertyInfo[] properties = typeof(Series).GetProperties();
            Series p = new Series();
            foreach (PropertyInfo pi in properties)
            {
                if (pi.CanWrite && pi.Name != "Item")
                {
                    pi.SetValue(p, pi.GetValue(o, null), null);
                }
            }

            return p;
        }

        static public string GetOleDbConnectionString(string input, string userName, string password)
        {
            string result = input;
            if (input != null && !input.Contains("User ID=") && !string.IsNullOrEmpty(userName)) result += string.Format(";User ID={0}", userName);
            if (input != null && !input.Contains("Password=") && !string.IsNullOrEmpty(password)) result += string.Format(";Password={0}", password);
            return result;
        }
        

        static public void ExecutePrePostSQL(DbConnection connection, string sql, object model, bool ignoreErrors)
        {
            try
            {
                if (!string.IsNullOrEmpty(sql))
                {
                    string finalSql = RazorHelper.CompileExecute(sql, model);
                    if (!string.IsNullOrEmpty(sql))
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = finalSql;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                if (!ignoreErrors) throw;
            }
        }

        public static GridItemCollection GetAllGridEntries(PropertyGrid grid)
        {
            object view = grid.GetType().GetField("gridView", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(grid);
            return (GridItemCollection)view.GetType().InvokeMember("GetAllGridEntries", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, view, null);
        }

        public static GridItem GetGridEntry(PropertyGrid grid, string label)
        {
            var entries = Helper.GetAllGridEntries(grid);
            if (entries != null)
            {
                foreach (GridItem item in entries)
                {
                    string label2 = item.Label.Replace("\t", "").ToLower();
                    if (label2 == label) return item;
                }
            }
            return null;
        }

        public static string FormatMessage(string message)
        {
            return string.Format("[{0} {1}] {2} ", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), message);
        }

        public static string CleanFileName(string s)
        {
            foreach (char character in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(character.ToString(), string.Empty);
            }

            foreach (char character in Path.GetInvalidPathChars())
            {
                s = s.Replace(character.ToString(), string.Empty);
            }

            return s;
        }

        private static void WriteLogEntry(string source, EventLogEntryType type, string message, params object[] args)
        {
            string msg = message;
            try
            {
                if (args.Length != 0) msg = string.Format(message, args);
            }
            catch { }
            try
            {
                EventLog.WriteEntry(source, msg, type);
            }
            catch { }
        }


        private static string GetIPAddress(HttpRequestBase request)
        {
            string ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }
            return request.ServerVariables["REMOTE_ADDR"];
        }

        private static string GetContextDetail(HttpRequestBase request, SecurityUser user)
        {
            var result = new StringBuilder("\r\n");
            if (user != null) result.AppendFormat("User: '{0}'; Groups: '{1}'; Windows User: '{2}'\r\n", user.Name, user.SecurityGroupsDisplay, Environment.UserName);
            if (request != null)
            {
                result.AppendFormat("URL: '{0}'\r\n", request.Url.OriginalString);
                if (request.RequestContext != null && request.RequestContext.HttpContext != null) result.AppendFormat("Session: '{0}'\r\n", request.RequestContext.HttpContext.Session.SessionID);                
                result.AppendFormat("IP: '{0}'\r\n", GetIPAddress(request));
                if (request.Form.Count > 0) foreach (string key in request.Form.Keys) result.AppendFormat("{0}={1}\r\n", key, request.Form[key]);
                if (request.QueryString.Count > 0) foreach (string key in request.QueryString.Keys) result.AppendFormat("{0}={1}\r\n", key, request.QueryString[key]);
            }
            return result.ToString();
        }

        public static void WriteWebException(Exception ex, HttpRequestBase request, SecurityUser user)
        {
            var currentEx = ex;
            var message = new StringBuilder("Unexpected error:\r\n");
            while (currentEx != null)
            {
                message.AppendFormat("\r\n{0}\r\n({1})\r\n", currentEx.Message, currentEx.StackTrace);
                currentEx = currentEx.InnerException;
            }
            message.Append(GetContextDetail(request, user));
            WriteLogEntry("Seal Web Server", EventLogEntryType.Error, message.ToString());
        }

        public static void WriteLogEntryWeb(EventLogEntryType type, string message, params object[] args)
        {
            WriteLogEntry("Seal Web Server", type, message, args);
        }

        public static void WriteLogEntryWeb(EventLogEntryType type, HttpRequestBase request, SecurityUser user, string message, params object[] args)
        {
            WriteLogEntry("Seal Web Server", type, message + GetContextDetail(request, user), args);
        }

        public static void WriteLogEntryWebDebug(HttpRequestBase request, SecurityUser user, string message)
        {
            WriteLogEntry("Seal Web Server", EventLogEntryType.Information, message + GetContextDetail(request, user));
        }

        public static void WriteLogEntryScheduler(EventLogEntryType type, string message, params object[] args)
        {
            var message2 = new StringBuilder("Unexpected error:\r\n");
            WriteLogEntry("Seal Task Scheduler", type, message, args);
        }

        public static bool IsMachineAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't determine user account status: " + ex.Message);
                return false;
            }
            return true;
        }


        public static bool IsValidOS()
        {
            int major = Environment.OSVersion.Version.Major;
            return (major >= 6);
        }

        static bool _checkTaskSchedulerOSDone = false;
        public static bool CheckTaskSchedulerOS()
        {
            if (!IsValidOS() && !_checkTaskSchedulerOSDone)
            {
                if (MessageBox.Show("The Task Scheduler works only with Windows Vista, 7, 2008 or above...\r\nDo you want to continue ?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    return false;
                }
                _checkTaskSchedulerOSDone = true;
            }
            return true;
        }

        static bool _checkWebServerOSDone = false;
        public static bool CheckWebServerOS()
        {
            if (!IsValidOS() && !_checkWebServerOSDone)
            {
                if (MessageBox.Show("The Web Server works only with Windows Vista, 7, 2008 or above...\r\nDo you want to continue ?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    return false;
                }
                _checkWebServerOSDone = true;
            }
            return true;
        }

        static bool _checkOleDBDone = false;
        public static bool CheckOLEDBOS()
        {
            if (!IsValidOS() && !_checkOleDBDone)
            {
                if (MessageBox.Show("The OLEDB Data Link Editor works only with Windows Vista, 7, 2008 or above...\r\nDo you want to continue ?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    return false;
                }
                _checkOleDBDone = true;
                return false;
            }
            return true;
        }

        public static void DisplayDataTable(DataTable table)
        {
            foreach (System.Data.DataRow row in table.Rows)
            {
                foreach (System.Data.DataColumn col in table.Columns)
                {
                    Debug.WriteLine("{0} = {1}", col.ColumnName, row[col]);
                }
                Debug.WriteLine("============================");
            }
        }


        public static DataTable GetDataTable(DbConnection connection, string sql)
        {
            DataTable result = new DataTable();
            DbDataAdapter adapter = null;
            if (connection is OdbcConnection)
            {
                OdbcCommand command = ((OdbcConnection)connection).CreateCommand();
                command.CommandTimeout = 0;
                command.CommandText = sql;
                adapter = new OdbcDataAdapter(command);
            }
            else
            {
                OleDbCommand command = ((OleDbConnection)connection).CreateCommand();
                command.CommandText = sql;
                adapter = new OleDbDataAdapter(command);
            }
            adapter.Fill(result);
            return result;
        }

        public static bool FindReplacePattern(string source, ref int index, string pattern, string replace, StringBuilder result)
        {
            if (index + pattern.Length <= source.Length && source.Substring(index, pattern.Length) == pattern)
            {
                result.Append(replace);
                index += pattern.Length - 1;
                return true;
            }
            return false;

        }

        public static Object Clone(Object source)
        {
            XmlSerializer serializer = new XmlSerializer(source.GetType());
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, source);
            ms.Position = 0;
            return serializer.Deserialize(ms);
        }

        public static DbConnection DbConnectionFromConnectionString(string connectionString)
        {
            DbConnection connection = null;
            OleDbConnectionStringBuilder builder = new System.Data.OleDb.OleDbConnectionStringBuilder(connectionString);
            string provider = builder["Provider"].ToString();
            if (provider.StartsWith("MSDASQL"))
            {
                //Provider=MSDASQL.1;Persist Security Info=False;Extended Properties="DSN=mysql2;SERVER=localhost;UID=root;DATABASE=sakila;PORT=3306";Initial Catalog=sakila
                //Provider=MSDASQL.1;Persist Security Info=True;Data Source=mysql;Initial Catalog=sakila
                //Provider=MSDASQL.1;Persist Security Info=False;Extended Properties="DSN=brCRM;DBQ=C:\tem\adb.mdb;DriverId=25;FIL=MS Access;MaxBufferSize=2048;PageTimeout=5;UID=admin;"

                //Extract the real ODBC connection string...to be able to use the OdbcConnection
                string odbcConnectionString = "";
                if (builder.ContainsKey("Extended Properties")) odbcConnectionString = builder["Extended Properties"].ToString();
                else if (builder.ContainsKey("Data Source") && !string.IsNullOrEmpty(builder["Data Source"].ToString())) odbcConnectionString = "DSN=" + builder["Data Source"].ToString();
                if (odbcConnectionString != "" && builder.ContainsKey("Initial Catalog")) odbcConnectionString += ";DATABASE=" + builder["Initial Catalog"].ToString();
                if (odbcConnectionString != "" && builder.ContainsKey("User ID")) odbcConnectionString += ";UID=" + builder["User ID"].ToString();
                if (odbcConnectionString != "" && builder.ContainsKey("Password")) odbcConnectionString += ";PWD=" + builder["Password"].ToString();

                connection = new OdbcConnection(odbcConnectionString);
            }
            else
            {
                connection = new OleDbConnection(connectionString);
            }
            return connection;
        }

        public static int CalculateHash(string str)
        {
            int res = 0;
            for (int i = 0; i < str.Length; i++)
            {
                res += (i * str[i]) % int.MaxValue;
            }
            return res;
        }


        public static string HtmlMakeImageSrcData(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] filebytes = new byte[fs.Length];
            fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));
            var ext = Path.GetExtension(path);
            string type;
            if (ext == ".ico") type = "x-icon";
            else type = ext.Replace(".", "");
            return "data:image/" + type + ";base64," + Convert.ToBase64String(filebytes, Base64FormattingOptions.None);
        }

    }
}
