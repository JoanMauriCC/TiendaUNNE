using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>Medios de pago disponibles para cobrar una venta.</summary>
    public static class ServicioMedioPago
    {
        public static List<MedioPagoItem> ListarActivos()
        {
            const string sql = @"
SELECT id_medio_pago, nombre, es_efectivo, requiere_referencia
FROM   dbo.Medio_pago
WHERE  activo = 1
ORDER BY es_efectivo DESC, nombre;";

            var lista = new List<MedioPagoItem>();
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new MedioPagoItem
                    {
                        Id = (int)dr["id_medio_pago"],
                        Nombre = (string)dr["nombre"],
                        EsEfectivo = (bool)dr["es_efectivo"],
                        RequiereReferencia = (bool)dr["requiere_referencia"]
                    });
                }
            }
            return lista;
        }
    }
}
