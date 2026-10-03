using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a datos de Caja y Caja_sesion (los turnos de caja): conexión y SQL, nada más.
    /// Qué caja se usa, si se puede abrir o cerrar y cómo se calcula el arqueo lo decide
    /// NegocioCaja.
    /// </summary>
    public static class ServicioCaja
    {
        /// <summary>Id de la primera caja activa, o null si no hay ninguna cargada.</summary>
        public static int? ObtenerIdCajaActiva()
        {
            const string sql = "SELECT TOP 1 id_caja FROM dbo.Caja WHERE activo = 1 ORDER BY id_caja;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                object id = cmd.ExecuteScalar();
                return id == null ? (int?)null : (int)id;
            }
        }

        /// <summary>Turno abierto de esa caja, o null si la caja está cerrada.</summary>
        public static CajaSesion ObtenerSesionAbierta(int idCaja)
        {
            const string sql = @"
SELECT  s.id_caja_sesion,
        s.id_caja,
        c.nombre                          AS nombre_caja,
        s.id_usuario_apertura,
        (p.apellido + N', ' + p.nombre)   AS usuario_apertura,
        s.fecha_apertura,
        s.monto_inicial
FROM        dbo.Caja_sesion s
INNER JOIN  dbo.Caja    c ON c.id_caja    = s.id_caja
INNER JOIN  dbo.Usuario u ON u.id_usuario = s.id_usuario_apertura
INNER JOIN  dbo.Persona p ON p.id_persona = u.id_persona
WHERE s.id_caja = @id_caja AND s.estado = 'ABIERTA';";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id_caja", SqlDbType.Int).Value = idCaja;

                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return null;

                    return new CajaSesion
                    {
                        IdCajaSesion = (int)dr["id_caja_sesion"],
                        IdCaja = (int)dr["id_caja"],
                        NombreCaja = (string)dr["nombre_caja"],
                        IdUsuarioApertura = (int)dr["id_usuario_apertura"],
                        UsuarioApertura = (string)dr["usuario_apertura"],
                        FechaApertura = (DateTime)dr["fecha_apertura"],
                        MontoInicial = (decimal)dr["monto_inicial"]
                    };
                }
            }
        }

        /// <summary>
        /// Total cobrado en efectivo durante ese turno, según los pagos de sus ventas
        /// emitidas. Las ventas anuladas no suman.
        /// </summary>
        public static decimal ObtenerEfectivoCobrado(int idCajaSesion)
        {
            const string sql = @"
SELECT ISNULL(SUM(vp.importe), 0)
FROM        dbo.Venta_pago     vp
INNER JOIN  dbo.Venta_cabecera vc ON vc.id_venta      = vp.id_venta
INNER JOIN  dbo.Medio_pago     mp ON mp.id_medio_pago = vp.id_medio_pago
WHERE vc.id_caja_sesion = @id_caja_sesion
  AND vc.estado         = 'EMITIDA'
  AND mp.es_efectivo    = 1;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id_caja_sesion", SqlDbType.Int).Value = idCajaSesion;
                return (decimal)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Abre un turno y devuelve su id. El índice único filtrado UQ_CajaSesion_abierta
        /// impide dos turnos abiertos en la misma caja: si ya había uno, lanza
        /// DuplicadoException.
        /// </summary>
        public static int AbrirSesion(int idCaja, int idUsuario, decimal montoInicial)
        {
            const string sql = @"
INSERT INTO dbo.Caja_sesion (id_caja, id_usuario_apertura, monto_inicial)
VALUES (@id_caja, @id_usuario, @monto);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            try
            {
                using (var cn = Db.AbrirConexion())
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@id_caja", SqlDbType.Int).Value = idCaja;
                    cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = idUsuario;
                    AgregarImporte(cmd, "@monto", montoInicial);

                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex)
            {
                throw DuplicadoException.Traducir(ex);
            }
        }

        /// <summary>
        /// Cierra el turno con las cuentas que calculó NegocioCaja. Devuelve la cantidad
        /// de filas afectadas; 0 significa que ya estaba cerrado o que no existe.
        /// </summary>
        public static int CerrarSesion(int idCajaSesion, int idUsuarioCierre,
                                       decimal montoFinalSistema, decimal montoFinalDeclarado,
                                       decimal diferencia, string observaciones)
        {
            const string sql = @"
UPDATE dbo.Caja_sesion
SET estado                = 'CERRADA',
    fecha_cierre          = SYSDATETIME(),
    id_usuario_cierre     = @id_usuario,
    monto_final_sistema   = @sistema,
    monto_final_declarado = @declarado,
    diferencia            = @diferencia,
    observaciones         = @observaciones
WHERE id_caja_sesion = @id AND estado = 'ABIERTA';";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCajaSesion;
                cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = idUsuarioCierre;
                AgregarImporte(cmd, "@sistema", montoFinalSistema);
                AgregarImporte(cmd, "@declarado", montoFinalDeclarado);
                AgregarImporte(cmd, "@diferencia", diferencia);
                cmd.Parameters.Add("@observaciones", SqlDbType.NVarChar, 300).Value =
                    (object)observaciones ?? DBNull.Value;

                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Los importes de la caja son DECIMAL(12,2).</summary>
        private static void AgregarImporte(SqlCommand cmd, string nombre, decimal valor)
        {
            SqlParameter p = cmd.Parameters.Add(nombre, SqlDbType.Decimal);
            p.Precision = 12;
            p.Scale = 2;
            p.Value = valor;
        }
    }
}
