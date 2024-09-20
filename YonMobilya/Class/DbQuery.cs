﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace YonMobilya.Class
{
    public class DbQuery
    {
        public static string ConnectionString = "Server=192.168.4.24;Database=VDB_YON01;User Id=sa;Password=MagicUser2023!;";
        public static string ConnectionString2 = "Server=192.168.4.24;Database=MDE_GENEL;User Id=sa;Password=MagicUser2023!;";
        public static DataTable Query(string Sorgu, string Connection)
        {
            DataTable dt = new DataTable();
            SqlConnection conn = new SqlConnection(Connection);
            SqlDataAdapter da = new SqlDataAdapter(Sorgu, conn);
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                return dt;
            }
            else { return null; }
        }
        public static DataTable Select(string spName, Dictionary<string, string> parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.AddWithValue(item.Key, item.Value);
                    }

                    SqlDataAdapter adap = new SqlDataAdapter(cmd);
                    adap.Fill(dt);
                }
            }
            return dt;
        }

        public static void Insert(string spName, Dictionary<string, string> parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.AddWithValue(item.Key, item.Value);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static string Insert2(string spName, Dictionary<string, string> param)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString2))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    cmd.CommandTimeout = 0;
                    if (param != null)
                    {
                        foreach (var item in param)
                        {
                            if (item.Key == "@ReturnDesc")
                            {
                                cmd.Parameters.Add("@ReturnDesc", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }
                    }
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.ExecuteNonQuery();

                    return (string)cmd.Parameters["@ReturnDesc"].Value;
                }
            }

        }
        public static void insertquery(string query, string Connection)
        {
            using (SqlConnection conn = new SqlConnection(Connection))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 0;
                    if (query != null)
                        cmd.ExecuteNonQuery();
                }
            }
        }
        public static string GetValue(string query)
        {
            using (SqlConnection sql = new SqlConnection(ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, sql);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt != null)
                {
                    return dt.Rows[0][0].ToString();
                }
                else
                {
                    return null;
                }
            }
        }
    }
}