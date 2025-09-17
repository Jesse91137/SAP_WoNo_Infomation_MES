using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SAP_WoNo_Infomation_MES
{
    class db
    {
        //本機測試
        private static readonly String connStr = "server=192.168.6.57;database=AMES_DB;uid=sa;pwd=A12345678;Connect Timeout = 10";
        //正式環境
        //private static readonly String connStr = "server=192.168.4.200;database=AMES_DB;uid=fa;pwd=fa;Connect Timeout = 10";
        public static int ExecueNonQuery(string sql, CommandType cmdType, params SqlParameter[] pms)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    //設置目前執行的是「存儲過程? 還是帶參數的sql 語句?」
                    cmd.CommandType = cmdType;
                    if (pms != null)
                    {
                        cmd.Parameters.AddRange(pms);
                    }

                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public static SqlDataReader ExecuteReader(string sql, CommandType cmdType, params SqlParameter[] pms)
        {
            SqlConnection con = new SqlConnection(connStr);
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = cmdType;
                if (pms != null)
                {
                    cmd.Parameters.AddRange(pms);
                }
                try
                {
                    con.Open();
                    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch
                {
                    con.Close();
                    con.Dispose();
                    throw;
                }
            }
        }
        public static DataTable ExecuteDataTable(string sql, CommandType cmdType, params SqlParameter[] pms)
        {
            DataTable dt = new DataTable();
            //use SqlDataAdapter ,it will establish Sql connection.So ,it no need to create Connection by yourself.
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connStr))
            {
                adapter.SelectCommand.CommandType = cmdType;
                if (pms != null)
                {
                    adapter.SelectCommand.Parameters.AddRange(pms);

                }
                adapter.Fill(dt);
                return dt;
            }
        }
        public static DataSet ExecuteDataSet(string sql, CommandType cmdType, params SqlParameter[] pms)
        {
            DataSet ds = new DataSet();
            //use SqlDataAdapter ,it will establish Sql connection.So ,it no need to create Connection by yourself.
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connStr))
            {
                adapter.SelectCommand.CommandType = cmdType;
                if (pms != null)
                {
                    adapter.SelectCommand.Parameters.AddRange(pms);

                }
                adapter.Fill(ds);
                return ds;
            }
        }
        public static DataSet ExecuteDataSetPmsList(string sql, CommandType cmdType, List<SqlParameter> pms)
        {
            DataSet ds = new DataSet();
            //use SqlDataAdapter ,it will establish Sql connection.So ,it no need to create Connection by yourself.
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connStr))
            {
                adapter.SelectCommand.CommandType = cmdType;
                if (pms != null)
                {
                    adapter.SelectCommand.Parameters.AddRange(pms.ToArray<SqlParameter>());//paralist.ToArray<SqlParameter>()

                }
                adapter.Fill(ds);
                return ds;
            }
        }
    }
}
