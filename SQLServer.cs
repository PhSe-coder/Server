using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace 服务器
{
    class SQLServer
    {
        public static string StrManager = "Data Source=DESKTOP-KVKOGT6;Initial Catalog = User information; Integrated Security = True";//登录信息数据库连接字符串
        public static string StrDynamicStatistic = "Data Source = DESKTOP-KVKOGT6; Initial Catalog = Dynamic statistics; Integrated Security = True";
        //用于查询；其实是相当于提供一个可以传参的函数，到时候写一个sql语句，存在string里，传给这个函数，就会自动执行。
        public DataTable ExecuteQuery(string mode, string sqlStr)
        {
            SqlConnection con = new SqlConnection(@mode);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sqlStr;
            DataTable dt = new DataTable();
            SqlDataAdapter msda;
            msda = new SqlDataAdapter(cmd);
            msda.Fill(dt);
            con.Close();
            return dt;
        }
        public int ExecuteUpdate(string mode, string sqlStr)      //用于增删改;
        {
            int i = 0;
            SqlConnection con = new SqlConnection(@mode);
            SqlCommand cmd = new SqlCommand(sqlStr, con);
            try//异常处理
            {
                con.Open();
                i = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
            return i;
        }
    }
}
