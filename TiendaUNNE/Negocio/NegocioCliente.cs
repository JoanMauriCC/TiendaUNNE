using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de Cliente: qué datos hacen falta para cargar uno, cómo se valida el DNI y el CUIT,
    /// y cómo se busca. "Consumidor final" no es un cliente cargado: una venta sin cliente ya lo es.
    /// </summary>
    public static class NegocioCliente
    {
        public const string ConsumidorFinal = "Consumidor Final";
        public const string ResponsableInscripto = "Responsable Inscripto";
        public const string Monotributista = "Monotributista";
        public const string Exento = "Exento";

        public const int LargoMaximoNombre = 100;
        public const int LargoMaximoRazonSocial = 150;
        public const int LargoMaximoEmail = 150;
        public const int LargoMaximoTelefono = 30;
        public const int LargoCuit = 11;

        /// <summary>Opciones de condición frente al IVA, en el orden en que se muestran.</summary>
        public static readonly string[] CondicionesIva =
            { ConsumidorFinal, ResponsableInscripto, Monotributista, Exento };

        private static readonly Regex RegexCuit = new Regex(@"^\d{11}$", RegexOptions.Compiled);

        // ---------------------------------------------------------------------
        // Búsqueda
        // ---------------------------------------------------------------------

        /// <summary>Clientes que coinciden con lo escrito (documento, apellido, nombre o razón social).</summary>
        public static List<ClienteItem> Buscar(string texto)
        {
            string buscado = Validaciones.NormalizarEspacios(texto);
            if (string.IsNullOrEmpty(buscado))
                return new List<ClienteItem>();

            return ServicioCliente.Buscar(buscado);
        }

        /// <summary>
        /// Cliente con ese DNI (8 dígitos) o CUIT (11 dígitos), o null si todavía no está cargado.
        /// Cualquier otra cosa no es un documento y se rechaza con un mensaje claro.
        /// </summary>
        public static ClienteItem ObtenerPorDocumento(string documento)
        {
            string doc = (documento ?? string.Empty).Trim();

            if (!Validaciones.EsDniValido(doc) && !RegexCuit.IsMatch(doc))
                throw new ReglaNegocioException("Escribí un DNI de 8 dígitos o un CUIT de 11, solo números.");

            return ServicioCliente.ObtenerPorDocumento(doc);
        }

        // ---------------------------------------------------------------------
        // Alta
        // ---------------------------------------------------------------------

        /// <summary>
        /// Devuelve el cliente con ese DNI; si no existe lo crea con los datos cargados.
        /// <paramref name="creado"/> dice si se creó ahora o ya estaba.
        /// </summary>
        public static ClienteItem ObtenerOCrear(ClienteEditModel m, int idUsuarioSesion, out bool creado)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            Normalizar(m);
            Validar(m);

            try
            {
                return ServicioCliente.ObtenerOCrear(m, idUsuarioSesion, Resumen(m), out creado);
            }
            catch (DuplicadoException ex)
            {
                throw TraducirDuplicado(ex);
            }
        }

        // ---------------------------------------------------------------------
        // Validación y normalización
        // ---------------------------------------------------------------------

        private static void Validar(ClienteEditModel m)
        {
            if (!Validaciones.EsDniValido(m.Dni))
                throw new ReglaNegocioException("El DNI tiene que tener exactamente 8 dígitos, sin puntos.");

            if (string.IsNullOrEmpty(m.Nombre))
                throw new ReglaNegocioException("El nombre es obligatorio.");
            if (m.Nombre.Length > LargoMaximoNombre)
                throw new ReglaNegocioException("El nombre no puede superar los " + LargoMaximoNombre + " caracteres.");
            if (!Validaciones.EsNombrePersonaValido(m.Nombre))
                throw new ReglaNegocioException("El nombre solo puede tener letras, espacios, apóstrofos y guiones.");

            if (string.IsNullOrEmpty(m.Apellido))
                throw new ReglaNegocioException("El apellido es obligatorio.");
            if (m.Apellido.Length > LargoMaximoNombre)
                throw new ReglaNegocioException("El apellido no puede superar los " + LargoMaximoNombre + " caracteres.");
            if (!Validaciones.EsNombrePersonaValido(m.Apellido))
                throw new ReglaNegocioException("El apellido solo puede tener letras, espacios, apóstrofos y guiones.");

            if (m.Email != null)
            {
                if (m.Email.Length > LargoMaximoEmail)
                    throw new ReglaNegocioException("El email no puede superar los " + LargoMaximoEmail + " caracteres.");
                if (!Validaciones.EsEmailValido(m.Email))
                    throw new ReglaNegocioException("El email no tiene un formato válido (ejemplo: nombre@correo.com).");
            }

            if (m.Telefono != null)
            {
                if (m.Telefono.Length > LargoMaximoTelefono)
                    throw new ReglaNegocioException("El teléfono no puede superar los " + LargoMaximoTelefono + " caracteres.");
                if (!Validaciones.EsTelefonoValido(m.Telefono))
                    throw new ReglaNegocioException("El teléfono solo puede tener números, espacios, + - ( ) y entre 6 y 30 caracteres.");
            }

            if (m.RazonSocial != null && m.RazonSocial.Length > LargoMaximoRazonSocial)
                throw new ReglaNegocioException("La razón social no puede superar los " + LargoMaximoRazonSocial + " caracteres.");

            if (!CondicionesIva.Contains(m.CondicionIva))
                throw new ReglaNegocioException("Elegí la condición frente al IVA.");

            if (m.Cuit != null && !EsCuitValido(m.Cuit))
                throw new ReglaNegocioException("El CUIT no es válido: son 11 dígitos y el último es de control.");

            if (m.CondicionIva == ResponsableInscripto && m.Cuit == null)
                throw new ReglaNegocioException("Un Responsable Inscripto necesita CUIT.");
        }

        /// <summary>Recorta espacios y deja en null lo opcional que quedó vacío.</summary>
        private static void Normalizar(ClienteEditModel m)
        {
            m.Dni = (m.Dni ?? string.Empty).Trim();
            m.Nombre = Validaciones.NormalizarEspacios(m.Nombre) ?? string.Empty;
            m.Apellido = Validaciones.NormalizarEspacios(m.Apellido) ?? string.Empty;
            m.Email = VacioANull(m.Email);
            m.Telefono = VacioANull(m.Telefono);
            m.RazonSocial = VacioANull(Validaciones.NormalizarEspacios(m.RazonSocial));
            m.Cuit = VacioANull(m.Cuit);
            m.CondicionIva = string.IsNullOrWhiteSpace(m.CondicionIva) ? ConsumidorFinal : m.CondicionIva.Trim();
        }

        private static string VacioANull(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        /// <summary>
        /// CUIT/CUIL: 11 dígitos y el último es de control. Se multiplican los primeros 10 por
        /// 5-4-3-2-7-6-5-4-3-2, se suma, y el dígito es 11 menos el resto de dividir por 11
        /// (11 pasa a 0; si diera 10 no existe ningún CUIT con esa base).
        /// </summary>
        public static bool EsCuitValido(string cuit)
        {
            if (cuit == null || !RegexCuit.IsMatch(cuit)) return false;

            int[] pesos = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int suma = 0;
            for (int i = 0; i < 10; i++)
                suma += (cuit[i] - '0') * pesos[i];

            int digito = 11 - (suma % 11);
            if (digito == 11) digito = 0;
            if (digito == 10) return false;

            return digito == cuit[10] - '0';
        }

        private static ReglaNegocioException TraducirDuplicado(DuplicadoException ex)
        {
            if (ex.Restriccion.IndexOf("UQ_Cliente_cuit", StringComparison.OrdinalIgnoreCase) >= 0)
                return new ReglaNegocioException("Ya hay un cliente cargado con ese CUIT.");

            if (ex.Restriccion.IndexOf("UQ_Persona_dni_cuit", StringComparison.OrdinalIgnoreCase) >= 0)
                return new ReglaNegocioException("Ya hay una persona cargada con ese DNI.");

            return new ReglaNegocioException("Ya existe un cliente con esos datos.");
        }

        /// <summary>Texto para la auditoría del alta.</summary>
        private static string Resumen(ClienteEditModel m)
        {
            return string.Format(
                "Alta de cliente: DNI = {0}; {1}, {2}; condición IVA = {3}; CUIT = {4}",
                m.Dni, m.Apellido, m.Nombre, m.CondicionIva, m.Cuit ?? "—");
        }
    }
}
