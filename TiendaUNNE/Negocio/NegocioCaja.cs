using System;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas del turno de caja: cuándo se puede abrir, cuándo cerrar y cómo se calcula
    /// el arqueo. El turno se guarda en Caja_sesion, así que sobrevive al cierre del
    /// programa. Lo único que todavía vive en memoria es el efectivo cobrado durante el
    /// turno: se va a calcular desde las ventas cuando estas se guarden en la base.
    /// </summary>
    public static class NegocioCaja
    {
        public const decimal MontoMaximo = 10000000m;
        public const int LargoMaximoObservaciones = 300;
        public const string FormatoImporte = "N2";

        private static decimal _efectivoCobrado;

        /// <summary>Sesión abierta, o null si la caja está cerrada.</summary>
        public static CajaSesion ObtenerSesionAbierta()
            => ServicioCaja.ObtenerSesionAbierta(IdCajaDelSistema());

        public static CajaSesion AbrirCaja(decimal montoInicial, int idUsuario)
        {
            ValidarMonto(montoInicial, "El monto inicial");

            int idCaja = IdCajaDelSistema();

            if (ServicioCaja.ObtenerSesionAbierta(idCaja) != null)
                throw new ReglaNegocioException("La caja ya está abierta.");

            try
            {
                ServicioCaja.AbrirSesion(idCaja, idUsuario, montoInicial);
            }
            catch (DuplicadoException)
            {
                // Otro puesto la abrió justo antes: la base no admite dos turnos abiertos.
                throw new ReglaNegocioException("La caja ya está abierta.");
            }

            _efectivoCobrado = 0;
            return ServicioCaja.ObtenerSesionAbierta(idCaja);
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
            if (sesion == null)
                throw new ReglaNegocioException("No hay ninguna caja abierta para cerrar.");

            ValidarMonto(montoDeclarado, "El efectivo contado");

            string notas = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
            if (notas != null && notas.Length > LargoMaximoObservaciones)
                throw new ReglaNegocioException(
                    "Las observaciones no pueden superar los " + LargoMaximoObservaciones + " caracteres.");

            ArqueoCaja arqueo = CalcularArqueo(sesion, montoDeclarado);

            int filas = ServicioCaja.CerrarSesion(sesion.IdCajaSesion, idUsuario,
                arqueo.EfectivoEsperado, montoDeclarado, arqueo.Diferencia, notas);

            if (filas == 0)
                throw new ReglaNegocioException("La caja ya estaba cerrada o no existe.");

            _efectivoCobrado = 0;
            return arqueo;
        }

        /// <summary>Suma al turno el efectivo que quedó en el cajón por una venta.</summary>
        internal static void RegistrarEfectivoCobrado(decimal importe)
        {
            _efectivoCobrado += importe;
        }

        /// <summary>El sistema trabaja con la primera caja activa que haya cargada.</summary>
        private static int IdCajaDelSistema()
        {
            int? idCaja = ServicioCaja.ObtenerIdCajaActiva();
            if (!idCaja.HasValue)
                throw new ReglaNegocioException("No hay ninguna caja configurada en el sistema.");

            return idCaja.Value;
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
