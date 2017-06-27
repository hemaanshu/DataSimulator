using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace DataSimulator
{
    public class DBUtil
    {
        public static DataTable GetDataTable(Type t)
        {
            var dt = new DataTable("MonitorData." + t.Name);
            dt.Locale = System.Globalization.CultureInfo.InvariantCulture;

            foreach (PropertyInfo info in t.GetProperties())
            {
                dt.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            return dt;
        }

        public static void FillData<T>(IList<T> data, DataTable dt)
        {
            dt.Rows.Clear();

            foreach (T t in data)
            {
                DataRow row = dt.NewRow();
                foreach (PropertyInfo info in typeof(T).GetProperties())
                {
                    if (info.GetValue(t, null) == null)
                        row[info.Name] = DBNull.Value;
                    else
                        row[info.Name] = info.GetValue(t, null);
                }

                dt.Rows.Add(row);
            }
        }

        public static void BulkInsert(DataTable dt)
        {
            try
            {
                SqlBulkCopy s = new SqlBulkCopy(Config.Instance.ConnectionString);

                // Copy the DataTable to SQL Server using SqlBulkCopy
                s.BulkCopyTimeout = 20;
                s.BatchSize = 5000;
                s.DestinationTableName = dt.TableName;

                foreach (DataColumn column in dt.Columns)
                {
                    s.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                Trace.Instance.WriteVerbose(">> Inserting Rows. Type: {0}, Count: {1}", dt.TableName, dt.Rows.Count);
                s.WriteToServer(dt);
                Trace.Instance.WriteVerbose("<< Inserted Rows. Type: {0}, Count: {1}", dt.TableName, dt.Rows.Count);
            }
            catch (Exception ex)
            {
                Trace.Instance.WriteError("Failed to bulk insert {0} data. Reason: {1}", dt.TableName, ex.Message);
            }
        }
    }
}
