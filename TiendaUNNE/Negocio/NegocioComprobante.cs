using System;
using System.Linq;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de los comprobantes que se emiten al cobrar:
    ///   Ticket     no fiscal, para cualquier cliente o para consumidor final; no separa el IVA.
    ///   Factura B  para consumidor final, monotributista o exento; el precio incluye el IVA.
    ///   Factura A  solo para un cliente Responsable Inscripto con CUIT; separa neto e IVA.
    /// Los precios de los productos son finales (ya incluyen el IVA del 21 %): en las facturas se
    /// separa el neto del IVA sin cambiar el total que paga el cliente.
    /// La numeración es correlativa por tipo de comprobante y punto de venta (la caja).
    /// </summary>
    public static class NegocioComprobante
    {
        public const string Ticket = "TKT";
        public const string FacturaB = "FB";
        public const string FacturaA = "FA";

        public const string NotaCreditoA = "NCA";
        public const string NotaCreditoB = "NCB";

        public const decimal AlicuotaIva = 21m;

        public static readonly TipoComprobanteItem[] Tipos =
        {
            new TipoComprobanteItem { Codigo = Ticket,   Nombre = "Ticket",    DiscriminaIva = false },
            new TipoComprobanteItem { Codigo = FacturaB, Nombre = "Factura B", DiscriminaIva = true },
            new TipoComprobanteItem { Codigo = FacturaA, Nombre = "Factura A", DiscriminaIva = true }
        };

        /// <summary>Notas de crédito: no se eligen al cobrar, se emiten desde el historial de ventas.</summary>
        public static readonly TipoComprobanteItem[] TiposNotaCredito =
        {
            new TipoComprobanteItem { Codigo = NotaCreditoA, Nombre = "Nota de crédito A", DiscriminaIva = true },
            new TipoComprobanteItem { Codigo = NotaCreditoB, Nombre = "Nota de crédito B", DiscriminaIva = true }
        };

        /// <summary>La nota de crédito que corresponde a una venta: A para la Factura A; B para la Factura B y el Ticket.</summary>
        public static string NotaCreditoPara(string codigoVenta)
        {
            return string.Equals(codigoVenta, FacturaA, StringComparison.OrdinalIgnoreCase) ? NotaCreditoA : NotaCreditoB;
        }

        public static TipoComprobanteItem Buscar(string codigo)
        {
            return Tipos.Concat(TiposNotaCredito).FirstOrDefault(t => string.Equals(t.Codigo, codigo, StringComparison.OrdinalIgnoreCase))
                ?? throw new ReglaNegocioException("El tipo de comprobante no existe.");
        }

        /// <summary>Comprueba que ese comprobante se pueda emitir para el cliente de la venta.</summary>
        public static void Validar(VentaEditModel venta, string codigo)
        {
            if (venta == null) throw new ArgumentNullException(nameof(venta));

            TipoComprobanteItem tipo = Buscar(codigo);
            ClienteItem cliente = venta.Cliente;

            if (tipo.Codigo == FacturaA)
            {
                if (cliente == null)
                    throw new ReglaNegocioException("La Factura A necesita un cliente: elegilo con F3.");

                if (cliente.CondicionIva != NegocioCliente.ResponsableInscripto)
                    throw new ReglaNegocioException(
                        "La Factura A es solo para clientes Responsable Inscripto. " +
                        "Este cliente figura como " + cliente.CondicionIva + ".");

                if (string.IsNullOrEmpty(cliente.Cuit))
                    throw new ReglaNegocioException("Este cliente no tiene CUIT cargado, y la Factura A lo exige.");
            }

            if (tipo.Codigo == FacturaB && cliente != null
                && cliente.CondicionIva == NegocioCliente.ResponsableInscripto)
                throw new ReglaNegocioException(
                    "A un Responsable Inscripto le corresponde Factura A, no B.");
        }

        /// <summary>
        /// Calcula el neto y el IVA de cada renglón y de la venta, según el tipo de comprobante.
        /// El neto de cada renglón se redondea y el IVA es la diferencia hasta el subtotal, así
        /// neto + IVA da siempre el total exacto, sin centavos de más ni de menos.
        /// </summary>
        public static void AplicarIva(VentaEditModel venta)
        {
            TipoComprobanteItem tipo = Buscar(venta.CodigoComprobante);

            decimal neto = 0, iva = 0;

            foreach (RenglonVenta r in venta.Renglones)
            {
                if (tipo.DiscriminaIva)
                {
                    decimal netoRenglon = decimal.Round(
                        r.Subtotal / (1 + AlicuotaIva / 100m), 2, MidpointRounding.AwayFromZero);

                    r.AlicuotaIva = AlicuotaIva;
                    r.ImporteIva = r.Subtotal - netoRenglon;
                    neto += netoRenglon;
                    iva += r.ImporteIva;
                }
                else
                {
                    r.AlicuotaIva = 0;
                    r.ImporteIva = 0;
                    neto += r.Subtotal;
                }
            }

            venta.ImporteNeto = neto;
            venta.ImporteIva = iva;
        }

        /// <summary>"0001-00000012": punto de venta de 4 dígitos y número de 8.</summary>
        public static string FormatearNumero(int puntoVenta, long numero)
        {
            return puntoVenta.ToString("D4") + "-" + numero.ToString("D8");
        }

        /// <summary>Arma el comprobante emitido a partir de la venta ya guardada.</summary>
        public static ComprobanteVista ArmarVista(VentaEditModel venta, VentaRegistrada registrada, decimal vuelto)
        {
            TipoComprobanteItem tipo = Buscar(venta.CodigoComprobante);
            ClienteItem c = venta.Cliente;

            var vista = new ComprobanteVista
            {
                Codigo = tipo.Codigo,
                Nombre = tipo.Nombre,
                DiscriminaIva = tipo.DiscriminaIva,
                PuntoVenta = venta.IdCaja,
                Numero = registrada.Numero,
                NumeroFormateado = FormatearNumero(venta.IdCaja, registrada.Numero),
                FechaHora = registrada.FechaHora,
                Cajero = SesionActual.Usuario == null ? string.Empty : SesionActual.Usuario.NombreCompleto,
                ClienteNombre = c == null ? "Consumidor final" : c.NombreCompleto,
                ClienteDocumento = c == null ? "—" : (string.IsNullOrEmpty(c.Cuit) ? "DNI " + c.Dni : "CUIT " + c.Cuit),
                ClienteCondicionIva = c == null ? NegocioCliente.ConsumidorFinal : c.CondicionIva,
                ImporteNeto = venta.ImporteNeto,
                ImporteIva = venta.ImporteIva,
                Total = venta.Total,
                Vuelto = vuelto
            };

            foreach (RenglonVenta r in venta.Renglones)
                vista.Renglones.Add(new ComprobanteRenglon
                {
                    Descripcion = r.Descripcion,
                    Cantidad = r.Cantidad,
                    PrecioUnitario = r.PrecioUnitario,
                    Subtotal = r.Subtotal
                });

            foreach (PagoVenta p in venta.Pagos)
                vista.Pagos.Add(new ComprobantePago { Medio = p.NombreMedioPago, Importe = p.Importe });

            return vista;
        }
    }
}
