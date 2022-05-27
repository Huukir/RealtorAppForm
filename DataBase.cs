using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtorAppForm
{
    internal class DataBase
    {
        SqlConnection sqlConnection = new SqlConnection($@"Data Source=(LocalDB)\MSSQLLocalDB;
        User ID=client; Password=123123;
        AttachDbFilename={Directory.GetCurrentDirectory()}\DataBase\SpecEcoDom.mdf;
        Integrated Security=True;
        Connect Timeout=10");

        public void OpenConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
                sqlConnection.Open();
        }
        public void CloseConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
                sqlConnection.Close();
        }
        public SqlConnection GetConnection()
        {
            return sqlConnection;
        }
    }
}
