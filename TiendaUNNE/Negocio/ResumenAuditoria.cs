using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TiendaUNNE
{
    /// <summary>
    /// Convierte lo que la auditoría guarda (textos largos «campo=valor») en una frase corta que se
    /// entiende de un vistazo, por ejemplo «Baja de producto: Campera rompeviento» o «Cierre de caja:
    /// contó $ 116.300,00, diferencia $ 0,00». No toca la base ni lo que quedó registrado: solo
    /// interpreta el texto al mostrarlo, así sirve también para los registros viejos.
    /// </summary>
    public static class ResumenAuditoria
    {
        private const int LargoMaximoResumen = 110;

        // Campos que puede traer un texto «campo=valor», y cómo se llaman al decir qué cambió.
        private static readonly Dictionary<string, string> Etiquetas =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Nombre", "nombre" },
                { "Categoría", "categoría" },
                { "Desc", "descripción" },
                { "Descripción", "descripción" },
                { "PVenta", "precio" },
                { "Stock", "stock" },
                { "StockMin", "stock mínimo" },
                { "DNI", "DNI" },
                { "Dir", "dirección" },
                { "Tel", "teléfono" },
                { "Email", "email" },
                { "FNac", "nacimiento" },
                { "Perfil", "perfil" },
                { "Activo", "activo" },
                { "Usuario", "usuario" },
                { Contrasena, "contraseña" }
            };

        // Cuando se cambia la contraseña, el texto guardado lo anota al final con esta marca (nunca la clave).
        private const string Contrasena = "Contraseña";
        private const string MarcaContrasena = @"\s*\[contraseña actualizada\]";

        /// <summary>La frase corta de un movimiento.</summary>
        public static string De(string accion, string tabla, string anterior, string nuevo)
        {
            anterior = (anterior ?? string.Empty).Trim();
            nuevo = (nuevo ?? string.Empty).Trim();

            string frase;
            switch (tabla)
            {
                case "Caja_sesion": frase = Caja(nuevo); break;
                case "Venta_cabecera": frase = Venta(anterior, nuevo); break;
                case "Cliente": frase = Cliente(nuevo); break;
                case NegocioBackup.TablaAuditoria: frase = BaseDeDatos(nuevo); break;
                default: frase = Registro(accion, tabla, anterior, nuevo); break;
            }

            return Acortar(string.IsNullOrEmpty(frase) ? (nuevo.Length > 0 ? nuevo : anterior) : frase);
        }

        // ---------------------------------------------------------------------
        // Movimientos sensibles: los que conviene mirar con más atención
        // ---------------------------------------------------------------------

        /// <summary>
        /// Si el movimiento es de los que merecen atención, por qué (para mostrarlo); si no, null.
        /// Son los que mueven plata o ponen en juego toda la base: notas de crédito, ventas anuladas,
        /// restauraciones de backup y aperturas o cierres de caja con diferencia.
        /// </summary>
        public static string Sensible(string accion, string tabla, string anterior, string nuevo)
        {
            nuevo = (nuevo ?? string.Empty).Trim();

            switch (tabla)
            {
                case "Venta_cabecera":
                    if (nuevo.StartsWith("Nota de crédito", StringComparison.OrdinalIgnoreCase))
                        return "devuelve dinero y stock (nota de crédito)";
                    if (nuevo.StartsWith("ANULADA", StringComparison.OrdinalIgnoreCase))
                        return "venta anulada";
                    return null;

                case NegocioBackup.TablaAuditoria:
                    return nuevo.IndexOf("RESTAURACIÓN", StringComparison.OrdinalIgnoreCase) >= 0
                        ? "reemplaza toda la base de datos por una copia" : null;

                case "Caja_sesion":
                    string diferencia = Monto(nuevo, "diferencia");
                    bool difiere = diferencia != null && !EsCero(diferencia);
                    if (!difiere) return null;
                    return nuevo.StartsWith("Cerrada", StringComparison.OrdinalIgnoreCase)
                        ? "el cierre de caja tiene diferencia"
                        : "la apertura no coincide con el cierre anterior";

                default:
                    return null;
            }
        }

        // ---------------------------------------------------------------------
        // Detalle de un movimiento: solo lo que cambió
        // ---------------------------------------------------------------------

        /// <summary>
        /// El detalle de un movimiento. En usuarios, categorías y productos separa los datos y deja solo
        /// los que cambiaron («Precio: 12.000 → 15.000»); en el resto (caja, ventas, backup) muestra el
        /// texto completo con una idea por renglón.
        /// </summary>
        public static DetalleAuditoria Detalle(string accion, string tabla, string anterior, string nuevo)
        {
            anterior = (anterior ?? string.Empty).Trim();
            nuevo = (nuevo ?? string.Empty).Trim();

            var detalle = new DetalleAuditoria();
            List<KeyValuePair<string, string>> campoNuevo = Campos(nuevo);
            List<KeyValuePair<string, string>> campoAnterior = Campos(anterior);

            bool esRegistro = tabla != "Caja_sesion" && tabla != "Venta_cabecera" && tabla != "Cliente"
                              && tabla != NegocioBackup.TablaAuditoria;

            if (esRegistro && (campoNuevo.Count > 0 || campoAnterior.Count > 0))
            {
                switch ((accion ?? string.Empty).ToUpperInvariant())
                {
                    case "ALTA":
                        foreach (KeyValuePair<string, string> c in campoNuevo)
                            detalle.Filas.Add(Fila(c.Key, null, c.Value));
                        break;

                    case "BAJA":
                        detalle.Filas.Add(new FilaDetalle { Campo = "Estado", Antes = "Activo", Despues = "Dado de baja" });
                        break;

                    default:
                        if (nuevo.IndexOf("reactivad", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            detalle.Filas.Add(new FilaDetalle { Campo = "Estado", Antes = "Dado de baja", Despues = "Activo" });
                            break;
                        }

                        foreach (KeyValuePair<string, string> c in campoNuevo)
                        {
                            string previo = Previo(campoAnterior, c.Key);
                            if (previo != null && !string.Equals(previo, c.Value, StringComparison.Ordinal))
                                detalle.Filas.Add(Fila(c.Key, previo, c.Value));
                        }
                        break;
                }

                if (detalle.TieneFilas) return detalle;

                detalle.TextoDespues = "No cambió ningún dato de los que se muestran (por ejemplo, solo se cambió la contraseña).";
                return detalle;
            }

            detalle.TextoAntes = UnaIdeaPorRenglon(anterior);
            detalle.TextoDespues = UnaIdeaPorRenglon(nuevo);
            return detalle;
        }

        private static FilaDetalle Fila(string clave, string antes, string despues)
        {
            string campo = Etiqueta(clave);
            return new FilaDetalle
            {
                Campo = char.ToUpperInvariant(campo[0]) + campo.Substring(1),
                Antes = antes == null ? string.Empty : Legible(antes),
                Despues = Legible(despues)
            };
        }

        private static string Legible(string valor)
        {
            if (valor.Length == 0) return "(vacío)";
            if (string.Equals(valor, "True", StringComparison.OrdinalIgnoreCase)) return "Sí";
            if (string.Equals(valor, "False", StringComparison.OrdinalIgnoreCase)) return "No";
            return valor;
        }

        private static string UnaIdeaPorRenglon(string texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? string.Empty
                : texto.Replace("; ", Environment.NewLine).Replace(". Llave", Environment.NewLine + "Llave");
        }

        // ---------------------------------------------------------------------
        // Altas, bajas y modificaciones de usuarios, categorías y productos
        // ---------------------------------------------------------------------

        private static string Registro(string accion, string tabla, string anterior, string nuevo)
        {
            string sustantivo = Sustantivo(tabla);

            List<KeyValuePair<string, string>> campoNuevo = Campos(nuevo);
            List<KeyValuePair<string, string>> campoAnterior = Campos(anterior);

            // Un texto sin campos (por ejemplo «Alta automática del administrador inicial») se muestra tal cual.
            // Los datos que identifican al registro (nombre, DNI) están en el lado que los trae: en una baja,
            // el valor nuevo es solo «activo = 0» y el nombre está en el anterior.
            List<KeyValuePair<string, string>> fuente = Valor(campoNuevo, "Nombre") != null ? campoNuevo : campoAnterior;
            if (fuente.Count == 0) fuente = campoNuevo;
            if (fuente.Count == 0) return null;

            string nombre = Valor(fuente, "Nombre");
            string dni = Valor(fuente, "DNI");
            string quien = string.IsNullOrEmpty(nombre) ? sustantivo : nombre;
            if (tabla == "Usuario" && !string.IsNullOrEmpty(dni))
                quien += " (DNI " + dni + ")";

            switch ((accion ?? string.Empty).ToUpperInvariant())
            {
                case "ALTA":
                    return "Alta de " + sustantivo + ": " + quien;

                case "BAJA":
                    return "Baja de " + sustantivo + ": " + quien;

                default:
                    if (nuevo.IndexOf("reactivad", StringComparison.OrdinalIgnoreCase) >= 0)
                        return "Reactivación de " + sustantivo + ": " + quien;

                    List<string> cambios = Cambios(campoAnterior, campoNuevo);
                    string detalle = cambios.Count == 0
                        ? string.Empty
                        : " · cambió " + string.Join(", ", cambios.Take(3)) +
                          (cambios.Count > 3 ? " y " + (cambios.Count - 3) + " más" : string.Empty);
                    return "Modificación de " + sustantivo + ": " + quien + detalle;
            }
        }

        /// <summary>Qué campos son distintos entre el valor anterior y el nuevo, con su nombre amable.</summary>
        private static List<string> Cambios(List<KeyValuePair<string, string>> antes, List<KeyValuePair<string, string>> despues)
        {
            var cambios = new List<string>();
            foreach (KeyValuePair<string, string> campo in despues)
            {
                string previo = Previo(antes, campo.Key);
                if (previo != null && !string.Equals(previo, campo.Value, StringComparison.Ordinal))
                    cambios.Add(Etiqueta(campo.Key));
            }
            return cambios;
        }

        // La contraseña solo figura cuando cambió, así que antes no estaba: se cuenta igual como cambio.
        private static string Previo(List<KeyValuePair<string, string>> antes, string clave)
        {
            string previo = Valor(antes, clave);
            return previo == null && clave == Contrasena ? "—" : previo;
        }

        // ---------------------------------------------------------------------
        // Caja, clientes, ventas y base de datos
        // ---------------------------------------------------------------------

        private static string Caja(string nuevo)
        {
            if (nuevo.StartsWith("Apertura de caja", StringComparison.OrdinalIgnoreCase))
            {
                string monto = Monto(nuevo, "monto inicial");
                string diferencia = Monto(nuevo, "diferencia");

                string frase = "Apertura de caja con $ " + (monto ?? "?");
                if (diferencia != null && !EsCero(diferencia))
                    frase += " (difiere $ " + diferencia + " del cierre anterior)";
                return frase;
            }

            if (nuevo.StartsWith("Cerrada", StringComparison.OrdinalIgnoreCase))
            {
                string contado = Monto(nuevo, "contado");
                string diferencia = Monto(nuevo, "diferencia");
                return "Cierre de caja: contó $ " + (contado ?? "?") + ", diferencia $ " + (diferencia ?? "?");
            }

            return null;
        }

        private static string Cliente(string nuevo)
        {
            Match m = Regex.Match(nuevo, @"^Alta de cliente:\s*DNI\s*=\s*(\S+?);\s*(.+?);\s*condición");
            return m.Success ? "Alta de cliente: " + m.Groups[2].Value.Trim() + " (DNI " + m.Groups[1].Value + ")" : null;
        }

        private static string Venta(string anterior, string nuevo)
        {
            Match nota = Regex.Match(nuevo, @"^(Nota de crédito [AB]) sobre (.+?);.*?total devuelto = \$ (-?[\d.,]+)");
            if (nota.Success)
                return nota.Groups[1].Value + " sobre " + nota.Groups[2].Value + " por $ " + nota.Groups[3].Value;

            // Anulaciones de cuando el sistema todavía las permitía: quedan en el historial.
            if (nuevo.StartsWith("ANULADA", StringComparison.OrdinalIgnoreCase))
            {
                int corte = anterior.IndexOf(" del ", StringComparison.Ordinal);
                string venta = corte > 0 ? anterior.Substring(0, corte) : "una venta";
                return "Venta anulada (función retirada): " + venta;
            }

            return null;
        }

        private static string BaseDeDatos(string nuevo)
        {
            Match m = Regex.Match(nuevo, @"RESTAURACIÓN de la copia (.+?)\. Llave");
            return m.Success ? "Restauración de la base con la copia " + m.Groups[1].Value : null;
        }

        // ---------------------------------------------------------------------
        // Ayudas de lectura de los textos guardados
        // ---------------------------------------------------------------------

        /// <summary>
        /// Parte un texto «Campo=valor; Campo=valor» en pares. Un trozo solo empieza un campo nuevo si su
        /// nombre es uno conocido: así una descripción con «; » adentro no se rompe en dos.
        /// </summary>
        public static List<KeyValuePair<string, string>> Campos(string texto)
        {
            var campos = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrWhiteSpace(texto)) return campos;

            bool cambioContrasena = Regex.IsMatch(texto, MarcaContrasena, RegexOptions.IgnoreCase);
            if (cambioContrasena)
                texto = Regex.Replace(texto, MarcaContrasena, string.Empty, RegexOptions.IgnoreCase);

            foreach (string parte in texto.Split(new[] { "; " }, StringSplitOptions.None))
            {
                int igual = parte.IndexOf('=');
                string clave = igual > 0 ? NormalizarClave(parte.Substring(0, igual).Trim()) : null;

                if (clave != null && Etiquetas.ContainsKey(clave))
                {
                    campos.Add(new KeyValuePair<string, string>(clave, parte.Substring(igual + 1).Trim()));
                }
                else if (campos.Count > 0)
                {
                    KeyValuePair<string, string> ultimo = campos[campos.Count - 1];
                    campos[campos.Count - 1] = new KeyValuePair<string, string>(ultimo.Key, ultimo.Value + "; " + parte);
                }
            }

            if (cambioContrasena)
                campos.Add(new KeyValuePair<string, string>(Contrasena, "Actualizada"));
            return campos;
        }

        // Los registros más viejos escribían «DNI/CUIT» donde hoy dice «DNI».
        private static string NormalizarClave(string clave)
        {
            return clave.StartsWith("DNI", StringComparison.OrdinalIgnoreCase) ? "DNI" : clave;
        }

        public static string Etiqueta(string clave)
        {
            string etiqueta;
            return Etiquetas.TryGetValue(clave, out etiqueta) ? etiqueta : clave;
        }

        private static string Valor(List<KeyValuePair<string, string>> campos, string clave)
        {
            foreach (KeyValuePair<string, string> c in campos)
                if (string.Equals(c.Key, clave, StringComparison.OrdinalIgnoreCase))
                    return c.Value;
            return null;
        }

        /// <summary>El importe que sigue a «etiqueta = $ », tal como se escribió (ej. «116.300,00»).</summary>
        private static string Monto(string texto, string etiqueta)
        {
            Match m = Regex.Match(texto, Regex.Escape(etiqueta) + @"\s*=\s*\$\s*(-?[\d.,]+)");
            return m.Success ? m.Groups[1].Value : null;
        }

        private static bool EsCero(string importe)
        {
            decimal valor;
            return decimal.TryParse(importe, System.Globalization.NumberStyles.Number,
                       System.Globalization.CultureInfo.CurrentCulture, out valor) && valor == 0;
        }

        private static string Sustantivo(string tabla)
        {
            switch (tabla)
            {
                case "Usuario": return "usuario";
                case "Categoria": return "categoría";
                case "Producto": return "producto";
                case "Cliente": return "cliente";
                default: return "registro";
            }
        }

        private static string Acortar(string texto)
        {
            texto = Regex.Replace(texto ?? string.Empty, @"\s+", " ").Trim();
            return texto.Length <= LargoMaximoResumen ? texto : texto.Substring(0, LargoMaximoResumen - 1) + "…";
        }
    }
}
