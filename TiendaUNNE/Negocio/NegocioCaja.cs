using System;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas del turno de caja: cuándo se puede abrir, cuándo cerrar y cómo se
    /// calcula el arqueo (lo que debería haber en el cajón contra lo que hay).
    /// </summary>
    public static class NegocioCaja
    {
        public const decimal MontoMaximo = 10000000m;

        /// <summary>Formato con el que se muestran los importes de la caja.</summary>
        public const string FormatoImporte = "N2";

        /// <summary>Sesión abierta de la caja, o null si está cerrada.</summary>
        public static CajaSesion ObtenerSesionAbierta()
        {
            CajaItem caja = ObtenerCaja();
            return ServicioCaja.ObtenerSesionAbierta(caja.Id);
        }

        public static CajaSesion AbrirCaja(decimal montoInicial, int idUsuario)
        {
            ValidarMonto(montoInicial, "El monto inicial");

            CajaItem caja = ObtenerCaja();

            if (ServicioCaja.ObtenerSesionAbierta(caja.Id) != null)
                throw new ReglaNegocioException("La caja ya está abierta.");

            ServicioCaja.AbrirSesion(caja.Id, idUsuario, montoInicial);

            return ServicioCaja.ObtenerSesionAbierta(caja.Id);
        }

        /// <summary>
        /// Arma las cuentas del cierre: monto inicial + efectivo que entró durante el
        /// turno es lo que debería haber, y la diferencia contra lo contado a mano.
        /// </summary>
        public static ArqueoCaja CalcularArqueo(CajaSesion sesion, decimal montoDeclarado)
        {
            if (sesion == null) throw new ArgumentNullException(nameof(sesion));

            return new ArqueoCaja
            {
                IdCajaSesion = sesion.IdCajaSesion,
                MontoInicial = sesion.MontoInicial,
                VentasEnEfectivo = ServicioCaja.TotalEfectivoDeLaSesion(sesion.IdCajaSesion),
                MontoDeclarado = montoDeclarado
            };
        }

        public static ArqueoCaja CerrarCaja(CajaSesion sesion, decimal montoDeclarado,
                                            int idUsuario, string observaciones)
        {
            if (sesion == null)
                throw new ReglaNegocioException("No hay ninguna caja abierta para cerrar.");

            ValidarMonto(montoDeclarado, "El efectivo contado");

            ArqueoCaja arqueo = CalcularArqueo(sesion, montoDeclarado);

            int filas = ServicioCaja.CerrarSesion(
                sesion.IdCajaSesion, idUsuario,
                arqueo.EfectivoEsperado, arqueo.MontoDeclarado, arqueo.Diferencia,
                string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim());

            if (filas == 0)
                throw new ReglaNegocioException("La caja ya estaba cerrada.");

            return arqueo;
        }

        private static CajaItem ObtenerCaja()
        {
            CajaItem caja = ServicioCaja.ObtenerCajaPredeterminada();

            if (caja == null)
                throw new ReglaNegocioException(
                    "No hay ninguna caja configurada en el sistema. " +
                    "Hay que ejecutar el script TiendaUNNE_DatosCaja.sql sobre la base de datos.");

            return caja;
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
