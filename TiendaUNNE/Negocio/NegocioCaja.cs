using System;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas del turno de caja: cuándo se puede abrir, cuándo cerrar y cómo se
    /// calcula el arqueo. Por ahora el turno vive solo en memoria: no se guarda en la
    /// base de datos, así que al cerrar la aplicación la caja vuelve a estar cerrada.
    /// </summary>
    public static class NegocioCaja
    {
        public const decimal MontoMaximo = 10000000m;
        public const string FormatoImporte = "N2";

        private const string NombreCaja = "Caja 1";

        private static CajaSesion _sesionAbierta;
        private static decimal _efectivoCobrado;
        private static int _ultimoIdSesion;

        /// <summary>Sesión abierta, o null si la caja está cerrada.</summary>
        public static CajaSesion ObtenerSesionAbierta() => _sesionAbierta;

        public static CajaSesion AbrirCaja(decimal montoInicial, int idUsuario)
        {
            ValidarMonto(montoInicial, "El monto inicial");

            if (_sesionAbierta != null)
                throw new ReglaNegocioException("La caja ya está abierta.");

            _ultimoIdSesion++;
            _efectivoCobrado = 0;
            _sesionAbierta = new CajaSesion
            {
                IdCajaSesion = _ultimoIdSesion,
                NombreCaja = NombreCaja,
                IdUsuarioApertura = idUsuario,
                UsuarioApertura = SesionActual.HaySesion ? SesionActual.Usuario.NombreCompleto : "-",
                FechaApertura = DateTime.Now,
                MontoInicial = montoInicial
            };

            return _sesionAbierta;
        }

        /// <summary>
        /// Arma las cuentas del cierre: monto inicial + efectivo cobrado durante el
        /// turno es lo que debería haber, y la diferencia contra lo contado a mano.
        /// </summary>
        public static ArqueoCaja CalcularArqueo(CajaSesion sesion, decimal montoDeclarado)
        {
            if (sesion == null) throw new ArgumentNullException(nameof(sesion));

            return new ArqueoCaja
            {
                IdCajaSesion = sesion.IdCajaSesion,
                MontoInicial = sesion.MontoInicial,
                VentasEnEfectivo = _efectivoCobrado,
                MontoDeclarado = montoDeclarado
            };
        }

        public static ArqueoCaja CerrarCaja(CajaSesion sesion, decimal montoDeclarado,
                                            int idUsuario, string observaciones)
        {
            if (sesion == null || _sesionAbierta == null ||
                sesion.IdCajaSesion != _sesionAbierta.IdCajaSesion)
                throw new ReglaNegocioException("No hay ninguna caja abierta para cerrar.");

            ValidarMonto(montoDeclarado, "El efectivo contado");

            ArqueoCaja arqueo = CalcularArqueo(sesion, montoDeclarado);

            _sesionAbierta = null;
            _efectivoCobrado = 0;

            return arqueo;
        }

        /// <summary>Suma al turno el efectivo que quedó en el cajón por una venta.</summary>
        internal static void RegistrarEfectivoCobrado(decimal importe)
        {
            if (_sesionAbierta == null)
                throw new ReglaNegocioException("Hay que abrir la caja antes de vender.");

            _efectivoCobrado += importe;
        }

        private static void ValidarMonto(decimal monto, string etiqueta)
        {
            if (monto < 0)
                throw new ReglaNegocioException(etiqueta + " no puede ser negativo.");

            if (monto > MontoMaximo)
                throw new ReglaNegocioException(etiqueta + " supera el máximo permitido.");
        }
    }
}
