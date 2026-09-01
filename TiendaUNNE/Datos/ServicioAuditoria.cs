using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a la tabla Auditoria (altas, bajas y modificaciones).
    /// <see cref="Registrar"/> puede enlistarse en una conexión/transacción existente
    /// para que el registro de auditoría sea atómico con la operación auditada.
    /// </summary>
    public static class ServicioAuditoria
    {
        public static void Registrar(
            string accion,                 // 'ALTA' | 'BAJA' | 'MODIFICACION'
            string tablaAfectada,
            int? idRegistroAfectado,
            string valorAnterior,
            string valorNuevo,
            int? idUsuario,
            SqlConnection cn = null,
            SqlTransaction tx = null)
        {
            const string sql = @"
INSERT INTO dbo.Auditoria
    (id_usuario, fecha_hora, accion, tabla_afectada, id_registro_afectado, valor_anterior, valor_nuevo)
VALUES
    (@id_usuario, SYSDATETIME(), @accion, @tabla, @id_reg, @val_ant, @val_nue);";

            bool conexionPropia = cn == null;
            if (conexionPropia) cn = Db.AbrirConexion();

            try
            {
                using (var cmd = new SqlCommand(sql, cn, tx))
                {
                    cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = (object)idUsuario ?? DBNull.Value;
                    cmd.Parameters.Add("@accion", SqlDbType.NVarChar, 20).Value = accion;
                    cmd.Parameters.Add("@tabla", SqlDbType.NVarChar, 100).Value = tablaAfectada;
                    cmd.Parameters.Add("@id_reg", SqlDbType.Int).Value = (object)idRegistroAfectado ?? DBNull.Value;
                    cmd.Parameters.Add("@val_ant", SqlDbType.NVarChar, -1).Value = (object)valorAnterior ?? DBNull.Value;
                    cmd.Parameters.Add("@val_nue", SqlDbType.NVarChar, -1).Value = (object)valorNuevo ?? DBNull.Value;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (conexionPropia) cn.Dispose();
            }
        }

        /// <summary>
        /// Listado para la pantalla de auditoría (solo lectura). Filtros opcionales:
        /// <paramref name="tablaAfectada"/> null/""/"Todas" = sin filtro de tabla;
        /// <paramref name="desde"/>/<paramref name="hasta"/> se aplican por día completo.
        /// </summary>
        public static DataTable Listar(string tablaAfectada, DateTime? desde, DateTime? hasta)
        {
            bool filtraTabla = !string.IsNullOrWhiteSpace(tablaAfectada) &&
                               !string.Equals(tablaAfectada, "Todas", StringComparison.OrdinalIgnoreCase);

            var sql = new StringBuilder(@"
SELECT  a.fecha_hora                        AS Fecha,
        (p.apellido + N', ' + p.nombre)     AS Usuario,
        a.accion                            AS Accion,
        a.tabla_afectada                    AS Tabla,
        a.valor_anterior                    AS ValorAnterior,
        a.valor_nuevo                       AS ValorNuevo
FROM        dbo.Auditoria a
LEFT JOIN   dbo.Usuario  u ON u.id_usuario = a.id_usuario
LEFT JOIN   dbo.Persona  p ON p.id_persona = u.id_persona
WHERE 1 = 1");

            if (filtraTabla) sql.Append(" AND a.tabla_afectada = @tabla");
            if (desde.HasValue) sql.Append(" AND a.fecha_hora >= @desde");
            if (hasta.HasValue) sql.Append(" AND a.fecha_hora < @hasta");
            sql.Append(" ORDER BY a.fecha_hora DESC;");

            var dt = new DataTable();
            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql.ToString(), cn))
            {
                if (filtraTabla)
                    cmd.Parameters.Add("@tabla", SqlDbType.NVarChar, 100).Value = tablaAfectada;
                if (desde.HasValue)
                    cmd.Parameters.Add("@desde", SqlDbType.DateTime2).Value = desde.Value.Date;
                if (hasta.HasValue)
                    cmd.Parameters.Add("@hasta", SqlDbType.DateTime2).Value = hasta.Value.Date.AddDays(1);

                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }
            return dt;
        }
    }
}
