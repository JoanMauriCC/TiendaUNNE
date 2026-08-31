using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Inserta filas en la tabla Auditoria (altas, bajas y modificaciones).
    /// Puede enlistarse en una conexión/transacción existente para que el registro
    /// de auditoría sea atómico con la operación auditada.
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
    }
}
