using System.Collections.Generic;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>Acceso a datos de la tabla Perfil.</summary>
    public static class ServicioPerfil
    {
        /// <summary>Perfiles activos, para poblar el ComboBox del editor de usuarios.</summary>
        public static List<PerfilItem> ListarActivos()
        {
            const string sql = @"
SELECT id_perfil, nombre
FROM   dbo.Perfil
WHERE  activo = 1
ORDER BY nombre;";

            var lista = new List<PerfilItem>();
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new PerfilItem
                    {
                        Id = (int)dr["id_perfil"],
                        Nombre = (string)dr["nombre"]
                    });
                }
            }
            return lista;
        }
    }
}
