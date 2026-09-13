using System;
using System.Data;
using System.Data.SqlClient;

namespace TiendaUNNE
{
    /// <summary>
    /// Acceso a datos de los turnos de caja: apertura, cierre y el efectivo que
    /// movió la sesión. Decidir si se puede abrir o cerrar es de NegocioCaja.
    /// </summary>
    public static class ServicioCaja
    {
        /// <summary>Primera caja activa. El sistema trabaja con una sola caja.</summary>
        public static CajaItem ObtenerCajaPredeterminada()
        {
            const string sql = "SELECT TOP 1 id_caja, nombre FROM dbo.Caja WHERE activo = 1 ORDER BY id_caja;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (!dr.Read())
                    return null;

                return new CajaItem
                {
                    Id = (int)dr["id_caja"],
                    Nombre = (string)dr["nombre"]
                };
            }
        }

        /// <summary>Sesión ABIERTA de esa caja, o null si está cerrada.</summary>
        public static CajaSesion ObtenerSesionAbierta(int idCaja)
        {
            const string sql = @"
SELECT  s.id_caja_sesion, s.id_caja, s.id_usuario_apertura,
        s.fecha_apertura, s.monto_inicial,
        c.nombre                        AS caja_nombre,
        (p.apellido + N', ' + p.nombre) AS usuario_apertura
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
                        NombreCaja = (string)dr["caja_nombre"],
                        IdUsuarioApertura = (int)dr["id_usuario_apertura"],
                        UsuarioApertura = (string)dr["usuario_apertura"],
                        FechaApertura = (DateTime)dr["fecha_apertura"],
                        MontoInicial = (decimal)dr["monto_inicial"]
                    };
                }
            }
        }

        public static int AbrirSesion(int idCaja, int idUsuario, decimal montoInicial)
        {
            const string sql = @"
INSERT INTO dbo.Caja_sesion (id_caja, id_usuario_apertura, monto_inicial)
VALUES (@id_caja, @id_usuario, @monto);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id_caja", SqlDbType.Int).Value = idCaja;
                cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@monto", SqlDbType.Decimal).Value = montoInicial;
                return (int)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Efectivo que entró y salió del cajón durante la sesión. Se calcula sobre
        /// Movimiento_caja (no sobre las ventas) para que cualquier movimiento manual
        /// que se agregue en el futuro entre solo en la cuenta.
        /// </summary>
        public static decimal TotalEfectivoDeLaSesion(int idCajaSesion)
        {
            const string sql = @"
SELECT ISNULL(SUM(CASE m.tipo WHEN 'INGRESO' THEN m.monto ELSE -m.monto END), 0)
FROM        dbo.Movimiento_caja m
INNER JOIN  dbo.Medio_pago     mp ON mp.id_medio_pago = m.id_medio_pago
WHERE m.id_caja_sesion = @id AND mp.es_efectivo = 1;";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCajaSesion;
                return (decimal)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Cierra la sesión. Devuelve la cantidad de filas afectadas: 0 significa que
        /// ya estaba cerrada (interpretarlo es cosa de Negocio).
        /// </summary>
        public static int CerrarSesion(int idCajaSesion, int idUsuarioCierre,
                                       decimal montoSistema, decimal montoDeclarado,
                                       decimal diferencia, string observaciones)
        {
            const string sql = @"
UPDATE dbo.Caja_sesion
SET estado                = 'CERRADA',
    id_usuario_cierre     = @id_usuario,
    fecha_cierre          = SYSDATETIME(),
    monto_final_sistema   = @sistema,
    monto_final_declarado = @declarado,
    diferencia            = @diferencia,
    observaciones         = @observaciones
WHERE id_caja_sesion = @id AND estado = 'ABIERTA';";

            using (var cn = Db.AbrirConexion())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = idUsuarioCierre;
                cmd.Parameters.Add("@sistema", SqlDbType.Decimal).Value = montoSistema;
                cmd.Parameters.Add("@declarado", SqlDbType.Decimal).Value = montoDeclarado;
                cmd.Parameters.Add("@diferencia", SqlDbType.Decimal).Value = diferencia;
                cmd.Parameters.Add("@observaciones", SqlDbType.NVarChar, 300).Value =
                    (object)observaciones ?? DBNull.Value;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCajaSesion;

                return cmd.ExecuteNonQuery();
            }
        }
    }
}
