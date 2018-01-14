using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;

namespace SQLHelperClass
{
    public class SqlHelper
    {
        private static readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ServiceTrackConnectionString"].ConnectionString;

        #region public method
        public DataTable ExecuteDataTable(string storedProcedure, Dictionary<string, object> parametersCollection)
        {
            DataTable ret = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = storedProcedure;
                    cmd.Parameters.Clear();
                    foreach (var item in parametersCollection)
                    {
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = item.Key,
                            SqlDbType = ConvertToDbType(item.Value.GetType()),
                            Value = item.Value
                        });
                    }
                    con.Open();
                    new SqlDataAdapter(cmd).Fill(ret);
                }
            }

            return ret;
        }
        public DataTable ExecuteDataTable(string storedProcedure, SqlParameter[] para)
        {
            DataTable ret = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = storedProcedure;

                    cmd.Parameters.Clear();
                    foreach (var item in para)
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = item.ParameterName,
                            SqlDbType = item.SqlDbType,
                            Value = item.Value
                        });

                    con.Open();
                    new SqlDataAdapter(cmd).Fill(ret);
                }
            }

            return ret;
        }
        public DataTable ExecuteDataTable(string storedProcedure)
        {
            DataTable ret = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = storedProcedure;
                    con.Open();
                    new SqlDataAdapter(cmd).Fill(ret);
                }
            }

            return ret;
        }
        public bool ExecuteNonQuery(string storedProcedure, SqlParameter[] para)
        {
            bool ret = false;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = storedProcedure;

                        cmd.Parameters.Clear();
                        foreach (var item in para)
                            cmd.Parameters.Add(new SqlParameter()
                            {
                                ParameterName = item.ParameterName,
                                SqlDbType = item.SqlDbType,
                                Value = item.Value
                            });

                        con.Open();
                        if (cmd.ExecuteNonQuery() <= 0) ret = false;
                        else ret = true;
                    }
                }
            }
            catch (Exception)
            {
                ret = false;
            }

            return ret;
        }
        public object ExecuteScalarObject(string storedProcedure, SqlParameter[] para)
        {
            object ret;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = storedProcedure;

                    cmd.Parameters.Clear();
                    foreach (var item in para)
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = item.ParameterName,
                            SqlDbType = item.SqlDbType,
                            Value = item.Value
                        });

                    con.Open();
                    ret = cmd.ExecuteScalar();
                }
            }
            return ret;
        }
        public DataTable ExecuteSqlDataTable(string sql)
        {
            DataTable ret = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("", con))
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = sql;

                    con.Open();
                    new SqlDataAdapter(cmd).Fill(ret);
                }
            }

            return ret;
        }
        public DataSet ExcuteDataSet(string storedProcedure, SqlParameter[] para)
        {
            DataSet ret = new DataSet();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("", con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = storedProcedure;

                    cmd.Parameters.Clear();
                    foreach (var item in para)
                        cmd.Parameters.Add(new SqlParameter()
                        {
                            ParameterName = item.ParameterName,
                            SqlDbType = item.SqlDbType,
                            Value = item.Value
                        });

                    con.Open();
                    new SqlDataAdapter(cmd).Fill(ret);
                }
            }

            return ret;
        }
        public SqlDbType ConvertToDbType(Type objectType)
        {
            var typeMap = new Dictionary<Type, SqlDbType>();
            typeMap[typeof(string)] = SqlDbType.NVarChar;
            typeMap[typeof(char[])] = SqlDbType.NVarChar;
            typeMap[typeof(int)] = SqlDbType.Int;
            typeMap[typeof(Int32)] = SqlDbType.Int;
            typeMap[typeof(Int16)] = SqlDbType.SmallInt;
            typeMap[typeof(Int64)] = SqlDbType.BigInt;
            typeMap[typeof(Byte[])] = SqlDbType.VarBinary;
            typeMap[typeof(Boolean)] = SqlDbType.Bit;
            typeMap[typeof(DateTime)] = SqlDbType.DateTime2;
            typeMap[typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset;
            typeMap[typeof(Decimal)] = SqlDbType.Decimal;
            typeMap[typeof(Double)] = SqlDbType.Float;
            typeMap[typeof(Decimal)] = SqlDbType.Money;
            typeMap[typeof(Byte)] = SqlDbType.TinyInt;
            typeMap[typeof(TimeSpan)] = SqlDbType.Time;

            return typeMap[(objectType)];
        }
        public List<T> ConverToList<T>(DataTable table)
        {
            var columnNames = table.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();
            var properties = typeof(T).GetProperties();
            return table.AsEnumerable().Select(row =>
            {
                var obj = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (pro != null && pro.CanWrite)
                    {
                        if (columnNames.Contains(pro.Name))
                        {
                            if (row[pro.Name] != System.DBNull.Value)
                            {
                                object propertyValue = System.Convert.ChangeType(row[pro.Name], pro.PropertyType);
                                pro.SetValue(obj, propertyValue, null);
                            }
                        }
                    }
                }
                return obj;
            }).ToList();
        }
        #endregion
    }
}