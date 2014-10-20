using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Text.RegularExpressions;
using System.Windows;
using MySql.Data.MySqlClient;

namespace CasitServer
{
    class DatabaseControl
    {
        #region sql
        #region databaseconnect
        static public string DataTable_tbAdministrator = "tbAdministrator";
        static public string DataTable_tbUserInformation = "tbUserInformation";
        static string dbsource = "server = 192.168.1.45;Network Library = DBMSSOCN;Persist Security Info = true;Initial Catalog = CasitMedDataBase;uid = CasitUser;pwd = chenming";
        //static string dbsource = "server = (local);Network Library = DBMSSOCN;Persist Security Info = true;Initial Catalog = CasitMedDataBase;uid = user1;pwd = 123456";
        #endregion
        
        #region administrator
        public DataSet GetAdminstratorInfo()
        {
            using (SqlConnection sqlcon = new SqlConnection(dbsource))
            {
                string sqltext = "select * from " + DataTable_tbAdministrator;
                using (SqlDataAdapter sqldata = new SqlDataAdapter(sqltext, sqlcon))
                {
                    DataSet ds = new DataSet();
                    ds.Clear();
                    sqldata.Fill(ds, DataTable_tbAdministrator);
                    return ds;
                }
            }
        }
        #endregion
        #region user
        public DataSet GetUserInfo()
        {
            using (SqlConnection sqlcon = new SqlConnection(dbsource))
            {
                string sqltext = "select * from " + DataTable_tbUserInformation;
                using (SqlDataAdapter sqldata = new SqlDataAdapter(sqltext, sqlcon))
                {
                    DataSet ds = new DataSet();
                    ds.Clear();
                    sqldata.Fill(ds, DataTable_tbUserInformation);
                    return ds;
                }
            }
        }
        #endregion
        #region publicfunction
        public string GetTableNameAdministrator()/*get table administrator*/
        {
            return DataTable_tbAdministrator;
        }
        public string GetTableNameUserInfomation()/*get table userinformation*/
        {
            return DataTable_tbUserInformation;
        }
        public string Login(string Uid, string Password, string TableName)///利用tablename中的uid和password登录
        {
            using (SqlConnection sqlcon = new SqlConnection(dbsource))
            {
                sqlcon.Open();
                string sqltext = "select * from " + TableName + " where idnumber='" + Uid + "'";
                try
                {
                    //*******************************读取数据库方式1（连接式）***************//
                    //using (SqlDataAdapter sqldata = new SqlDataAdapter(sqltext, sqlcon))
                    //{
                    //    
                    //    //DataSet ds = new DataSet();
                    //    //ds.Clear();                        
                    //    //sqldata.Fill(ds, TableName);
                    //    //if (Password == ds.Tables[0].Rows[0]["password"].ToString())
                    //    //{
                    //    //    return "Success";
                    //    //}
                    //    //else
                    //    //{
                    //    //    return "密码错误";
                    //    //}                    
                    //}
                    //****************读取数据库方式2（非连接式）**********************//
                    SqlCommand cmd = new SqlCommand(sqltext, sqlcon);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    rdr.Read();
                    if (TableName == DataTable_tbUserInformation)
                    {
                        if (Password == rdr[2].ToString())
                            return "Success";
                        else
                            return "key error";
                    }
                    else if (TableName == DataTable_tbAdministrator)
                    {
                        if (Password == rdr[1].ToString())
                            return "Success";
                        else
                            return "key error";
                    }
                    else
                        return "key error";
                }
                catch
                {
                    return "user error";
                }
            }
        }

        public Boolean CheckSameID(string Keyword, string ColumnName, string tablename)///查看在tablename表中是否存在属性columnname值为keyword的项
        {
            using (SqlConnection sqlcon = new SqlConnection(dbsource))
            {
                try
                {
                    string sqltext = "select * from " + tablename + " where " + ColumnName + "='" + Keyword + "'";
                    using (SqlDataAdapter sqldata = new SqlDataAdapter(sqltext, sqlcon))
                    {
                        DataSet ds = new DataSet();
                        ds.Clear();
                        sqldata.Fill(ds, tablename);
                        if (ds.Tables[0].Rows.Count < 1)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                catch (Exception exc)
                {
                    return false;
                }
            }
        }

        public bool AddUser(string[] ud)///在数据库中注册新用户
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(dbsource))
                {
                    //MessageBox.Show("开始连接");
                    sqlcon.Open();
                    //MessageBox.Show("打开数据库成功");
                    string sqltext = "insert into "+ DataTable_tbUserInformation + "(IDnumber,UserName,Password,Remark) values('" + ud[0] + "','" + ud[1] + "','" + ud[2] + "','" + ud[3] + "')";
                    SqlCommand sqlcmd = new SqlCommand(sqltext, sqlcon);
                    sqlcmd.ExecuteNonQuery();
                    //MessageBox.Show("执行成功");
                    return true;
                }
            }
            catch (Exception exc)
            {
                return false;
                //MessageBox.Show(e.Message);
            }
        }
        #endregion
        #endregion

        #region mysql read and write
        #region databaseconnect
        private DataSet dsall;
        private MySqlConnection conn;
        private MySqlDataAdapter mdap;
        static public string T_userInfo = "userInfo";
        static string mysqlConnect = "server=localhost;database = tee;uid=root;pwd = casit.3058519";
        #endregion
        #region user
        public void mysqlconnect()
        {
            conn = new MySqlConnection(mysqlConnect);
            conn.Open();
        }
        #endregion
        #region publicfun
        #endregion
        #endregion
    }
}
