using System;
using System.Data.SqlClient;

namespace VBWebService
{
    public class Conexion
    {
        public static SqlDataAdapter adaptador = new SqlDataAdapter();
        public static SqlCommand comando = new SqlCommand();
        public static SqlConnection conexion = new SqlConnection();

        public static void Conectar()
        {
            conexion.ConnectionString = "Server=localhost;Database=prueba;User id=sa;Password=calv.2009dd";
        }
    }
}
