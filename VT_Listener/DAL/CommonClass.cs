
using System;
using System.Data;
//using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VT_Listener.DAL
{
    class CommonClass
    {
        public static string str_ConStr;
        static SqlConnection Sql_Con;

        public CommonClass()
        {
            str_ConStr = System.Configuration.ConfigurationSettings.AppSettings.Get("ConnectionString");
            Sql_Con = new SqlConnection(str_ConStr);
        }

        /// <summary>
        /// Creates a new connection for connected state
        /// </summary>
        /// <returns>SqlConnection(returns the connection object)</returns>        
        public SqlConnection GetConnection()
        {
            return (new SqlConnection(str_ConStr));
        }


        /// <summary>
        /// Returns a connection object for disconnected state
        /// </summary>
        /// <returns>SqlConnection(returns the connection object)</returns>
        public static SqlConnection GetDisConnection()
        {
            return (Sql_Con);
        }


        /// <summary>
        /// Disposes the connection objects
        /// </summary>
        /// <param name="parScnObj">Connection Object</param>        
        public static void CloseConnection(SqlConnection parScnObj)
        {
            parScnObj.Dispose();
        }


        /// <summary>
        /// Gets the DataSet for the Sql Query
        /// Gets Dataset from DataAccess class
        /// </summary>
        /// <arr_Param name="str_Sql">Sql</arr_Param>
        /// <returns>DataSet</returns>
        public DataSet GetDataSet(string str_Sql)
        {
            SqlConnection scn_Main = GetConnection();
            DataSet dse = null;
            try
            {
                //scn_Main.ConnectionTimeout = 600;
                SqlDataAdapter sda = new SqlDataAdapter(str_Sql, scn_Main);
                dse = new DataSet();
                sda.Fill(dse, "tbl");
            }
            catch (Exception ex)
            {

                Console.Write(ex.Message);
                dse = null;
            }
            return dse;
        }


        /// <summary>
        /// Executes the SqlCommand object
        /// and returns the result as a  dataset
        /// </summary>
        /// <param name="sco_CmdObj">Command Object</param>
        /// <returns>DataSet</returns>
        public DataSet GetDataSet(SqlCommand sco_CmdObj)
        {
            SqlConnection scn_Main = GetConnection();
            DataSet dse = null;

            try
            {
                sco_CmdObj.Connection = scn_Main;
                //scn_Main.ConnectionTimeout = 600;
                SqlDataAdapter sda = new SqlDataAdapter(sco_CmdObj);
                dse = new DataSet();
                sda.Fill(dse, "tbl");
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                dse = null;
            }
            return dse;
        }



        /// <summary>
        /// Returns a DataTable for any Query string Passed as parameter
        /// </summary>
        /// <param name="str_sql">Sql Query</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(string str_sql)
        {
            DataTable dtb_Table = new DataTable();
            try
            {
                SqlConnection scn_Con = GetConnection();
                //scn_Con.ConnectionTimeout = 6000;
                SqlDataAdapter sda = new SqlDataAdapter(str_sql, scn_Con);
                sda.Fill(dtb_Table);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return dtb_Table;
            }

            return dtb_Table;
        }

        /// <summary>
        /// Returns a DataTable for any Command Object Passed as parameter
        /// </summary>
        /// <param name="str_sql">Sql Query</param>
        /// <returns>DataTable</returns>

        public DataTable GetDataTable(SqlCommand sco_CmdObj)
        {
            SqlConnection scn_Main = GetConnection();
            DataTable dtb = new DataTable();

            try
            {
                sco_CmdObj.Connection = scn_Main;
                //scn_Main.ConnectionTimeout = 600;
                SqlDataAdapter sda = new SqlDataAdapter(sco_CmdObj);
                sda.Fill(dtb);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return dtb;
            }
            return dtb;
        }


        /// <summary>
        /// Executes the command object passed as parameter 
        /// If Executes returns true else false.
        /// </summary>
        /// <param name="sco_CmdObj">Command object</param>
        /// <returns>Boolean</returns>
        public bool ExecuteCmdObj(SqlCommand sco_CmdObj)
        {
            SqlConnection scn_Main = GetConnection();
            int in_NoRec = -1;
            bool bo_Status = false;

            try
            {
                sco_CmdObj.Connection = scn_Main;
                scn_Main.Open();
                in_NoRec = sco_CmdObj.ExecuteNonQuery();
                bo_Status = true;
                scn_Main.Close();
            }
            catch (Exception ex)
            {
                bo_Status = false;
               Console.Write(ex.Message);
            }
            return bo_Status;
        }


        /// <summary>
        /// Executes the command object in the passed ArrayList 
        /// </summary>
        /// <arr_Param name="arr">ArrayList</arr_Param>
        /// <returns>Boolean</returns>
        //public bool ExecuteCmdObjArray(ArrayList arr_Cmd)
        //{
        //    bool bo_Status = true;
        //    int in_NoRec = -2;

        //    SqlCommand sco_Cmd = new SqlCommand(); ;
        //    SqlTransaction stc_Trans;
        //    SqlConnection scn_Con = GetConnection();
        //    scn_Con.Open();
        //    stc_Trans = scn_Con.BeginTransaction();

        //    try
        //    {
        //        for (int i = 0; i < arr_Cmd.Count; i++)
        //        {
        //            sco_Cmd = (SqlCommand)arr_Cmd[i];
        //            sco_Cmd.Connection = scn_Con;
        //            sco_Cmd.Transaction = stc_Trans;

        //            in_NoRec = sco_Cmd.ExecuteNonQuery();

        //        }
        //        stc_Trans.Commit();

        //    }
        //    catch (Exception ex)
        //    {
        //        bo_Status = false;
        //        System.Web.HttpContext.Current.Response.Write(ex.Message);
        //    }
        //    sco_Cmd.Dispose();
        //    scn_Con.Close();
        //    return bo_Status;
        //}


        /// <summary>
        /// Updates the sql Query and return the number of records updated.
        /// </summary>
        /// <param name="str_Sql">Sql string</param>
        /// <returns>integer</returns>
        public int ExecuteUptQry(string str_Sql)
        {
            int in_Res = -2;

            try
            {
                SqlCommand sco_Scmd = null;
                SqlConnection scn_Main = GetConnection();
                sco_Scmd = new SqlCommand(str_Sql, scn_Main);
                scn_Main.Open();
                in_Res = sco_Scmd.ExecuteNonQuery();
                scn_Main.Close();

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                in_Res = -2;
            }
            return in_Res;
        }

    }
}
