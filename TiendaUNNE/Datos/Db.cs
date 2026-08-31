using System.Configuration;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Fábrica de conexiones a SQL Server. La cadena se lee de App.config (clave "TiendaUNNE").
    /// </summary>
    public static class Db
    {
        public static string ConnectionString
            => ConfigurationManager.ConnectionStrings["TiendaUNNE"].ConnectionString;

        /// <summary>Devuelve una conexión SIN abrir.</summary>
        public static SqlConnection CrearConexion() => new SqlConnection(ConnectionString);

        /// <summary>Devuelve una conexión ya abierta (el llamador la debe liberar con using).</summary>
        public static SqlConnection AbrirConexion()
        {
            var cn = new SqlConnection(ConnectionString);
            cn.Open();
            return cn;
        }
    }
}
