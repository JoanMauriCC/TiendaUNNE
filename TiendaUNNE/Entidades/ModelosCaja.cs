using System;

namespace TiendaUNNE
{
    /// <summary>Turno de caja abierto: desde cuándo, con cuánto y por quién.</summary>
    public sealed class CajaSesion
    {
        public int IdCajaSesion { get; set; }
        public int IdCaja { get; set; }
        public string NombreCaja { get; set; }
        public int IdUsuarioApertura { get; set; }
        public string UsuarioApertura { get; set; }
        public DateTime FechaApertura { get; set; }
        public decimal MontoInicial { get; set; }
    }

    /// <summary>
    /// Cuentas del cierre de caja: cuánto efectivo debería haber según el sistema,
    /// cuánto se contó realmente y la diferencia entre ambos.
    /// </summary>
    public sealed class ArqueoCaja
    {
        public int IdCajaSesion { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal VentasEnEfectivo { get; set; }
        public decimal MontoDeclarado { get; set; }

        public decimal EfectivoEsperado => MontoInicial + VentasEnEfectivo;

        /// <summary>Negativa = falta plata en el cajón; positiva = sobra.</summary>
        public decimal Diferencia => MontoDeclarado - EfectivoEsperado;
    }

    /// <summary>Medio de pago disponible para cobrar.</summary>
    public sealed class MedioPagoItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool EsEfectivo { get; set; }
        public bool RequiereReferencia { get; set; }
        public override string ToString() => Nombre;
    }
}
