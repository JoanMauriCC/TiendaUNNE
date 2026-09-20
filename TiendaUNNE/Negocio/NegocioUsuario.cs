using System;
using System.Collections.Generic;
using System.Data;

namespace TiendaUNNE
{
    /// <summary>
    /// Reglas de negocio de Usuario: validaciones (incluidas las de formato con Regex),
    /// normalización, hasheo de la contraseña antes de guardarla, armado de los textos
    /// de auditoría y la regla de que nadie puede darse de baja a sí mismo.
    /// </summary>
    public static class NegocioUsuario
    {
        public const int LargoMaximoDniCuit = 20;
        public const int LargoMaximoNombre = 100;
        public const int LargoMaximoApellido = 100;
        public const int LargoMaximoDireccion = 200;
        public const int LargoMaximoTelefono = 30;
        public const int LargoMaximoEmail = 150;

        /// <summary>Mínimo exigido al fijar o cambiar una contraseña.</summary>
        public const int LargoMinimoPassword = 8;

        public const int EdadMinima = 16;

        /// <summary>El editor toma de acá los topes del selector de fecha.</summary>
        public static DateTime FechaNacimientoMinima => new DateTime(1900, 1, 1);
        public static DateTime FechaNacimientoMaxima => DateTime.Today.AddYears(-EdadMinima);

        /// <summary>
        /// Encierra una fecha guardada dentro del rango permitido, para que un dato viejo
        /// fuera de rango no rompa el selector de fecha al abrir el editor.
        /// </summary>
        public static DateTime AcotarFechaNacimiento(DateTime fecha)
        {
            if (fecha < FechaNacimientoMinima) return FechaNacimientoMinima;
            if (fecha > FechaNacimientoMaxima) return FechaNacimientoMaxima;
            return fecha;
        }

        // ---------------------------------------------------------------------
        // Consultas
        // ---------------------------------------------------------------------

        /// <summary>Usuarios activos o dados de baja, según <paramref name="activos"/>.</summary>
        public static DataTable Listar(bool activos) => ServicioUsuario.Listar(activos);

        public static UsuarioEditModel ObtenerParaEdicion(int idUsuario)
        {
            var usuario = ServicioUsuario.Obtener(idUsuario);
            if (usuario == null)
                throw new ReglaNegocioException("El usuario ya no existe.");

            return usuario;
        }

        // ---------------------------------------------------------------------
        // Alta y edición
        // ---------------------------------------------------------------------

        /// <summary>
        /// Valida, normaliza, hashea la contraseña si corresponde y persiste.
        /// En edición, una contraseña en blanco significa "no cambiarla".
        /// </summary>
        public static void Guardar(UsuarioEditModel m, int idUsuarioSesion)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));

            Validar(m);
            Normalizar(m);

            bool cambiaPassword = !string.IsNullOrWhiteSpace(m.PasswordPlano);

            byte[] hash = null, salt = null;
            if (cambiaPassword)
                PasswordHasher.Generar(m.PasswordPlano, out hash, out salt);

            try
            {
                if (m.EsAlta)
                {
                    ServicioUsuario.Crear(m, idUsuarioSesion, hash, salt, Resumen(m));
                }
                else
                {
                    var anterior = ServicioUsuario.Obtener(m.IdUsuario);
                    if (anterior == null)
                        throw new ReglaNegocioException("El usuario ya no existe.");

                    string resumenNuevo = Resumen(m) +
                        (cambiaPassword ? "  [contraseña actualizada]" : string.Empty);

                    ServicioUsuario.Actualizar(m, idUsuarioSesion, hash, salt,
                        Resumen(anterior), resumenNuevo);
                }
            }
            catch (DuplicadoException ex)
            {
                throw TraducirDuplicado(ex);
            }
        }

        // ---------------------------------------------------------------------
        // Baja lógica
        // ---------------------------------------------------------------------

        public static void DarDeBaja(int idUsuario, int idUsuarioSesion)
        {
            if (idUsuario == idUsuarioSesion)
                throw new ReglaNegocioException("No podés dar de baja tu propio usuario.");

            var anterior = ServicioUsuario.Obtener(idUsuario);
            if (anterior == null)
                throw new ReglaNegocioException("El usuario ya no existe.");

            int filas = ServicioUsuario.DarDeBaja(idUsuario, idUsuarioSesion, Resumen(anterior));

            if (filas == 0)
                throw new ReglaNegocioException("El usuario ya estaba dado de baja o no existe.");
        }

        // ---------------------------------------------------------------------
        // Reactivación
        // ---------------------------------------------------------------------

        public static void DarDeAlta(int idUsuario, int idUsuarioSesion)
        {
            var anterior = ServicioUsuario.Obtener(idUsuario);
            if (anterior == null)
                throw new ReglaNegocioException("El usuario ya no existe.");

            int filas = ServicioUsuario.DarDeAlta(idUsuario, idUsuarioSesion, Resumen(anterior));

            if (filas == 0)
                throw new ReglaNegocioException("El usuario ya estaba activo.");
        }

        // ---------------------------------------------------------------------
        // Validación y normalización
        // ---------------------------------------------------------------------

        /// <summary>
        /// Campos obligatorios que están vacíos. Es la única definición de "qué es
        /// obligatorio": la contraseña solo lo es en un alta, porque al editar en blanco
        /// significa "no cambiarla". Dirección, teléfono, email y fecha son opcionales.
        /// </summary>
        public static List<CampoUsuario> CamposObligatoriosFaltantes(UsuarioEditModel m)
        {
            var faltantes = new List<CampoUsuario>();

            if (string.IsNullOrWhiteSpace(m.DniCuit)) faltantes.Add(CampoUsuario.Dni);
            if (string.IsNullOrWhiteSpace(m.Nombre)) faltantes.Add(CampoUsuario.Nombre);
            if (string.IsNullOrWhiteSpace(m.Apellido)) faltantes.Add(CampoUsuario.Apellido);
            if (m.IdPerfil <= 0) faltantes.Add(CampoUsuario.Perfil);
            if (m.EsAlta && string.IsNullOrWhiteSpace(m.PasswordPlano)) faltantes.Add(CampoUsuario.Password);

            return faltantes;
        }

        private static void Validar(UsuarioEditModel m)
        {
            // Obligatorios: se avisan todos juntos, no de a uno.
            List<CampoUsuario> faltantes = CamposObligatoriosFaltantes(m);
            if (faltantes.Count > 0)
                throw new CamposIncompletosException(faltantes);

            // Formato (expresiones regulares)
            if (!Validaciones.EsDniValido(m.DniCuit))
                throw new ReglaNegocioException(
                    "El DNI no tiene un formato válido. Usá solo números, sin puntos ni guiones (por ej. 30123456).");

            if (!Validaciones.EsNombrePersonaValido(m.Nombre))
                throw new ReglaNegocioException(
                    "El nombre solo puede tener letras, espacios, apóstrofos o guiones.");

            if (!Validaciones.EsNombrePersonaValido(m.Apellido))
                throw new ReglaNegocioException(
                    "El apellido solo puede tener letras, espacios, apóstrofos o guiones.");

            if (!string.IsNullOrWhiteSpace(m.Email) && !Validaciones.EsEmailValido(m.Email))
                throw new ReglaNegocioException("El email no tiene un formato válido.");

            if (!string.IsNullOrWhiteSpace(m.Telefono) && !Validaciones.EsTelefonoValido(m.Telefono))
                throw new ReglaNegocioException("El teléfono no tiene un formato válido.");

            // Contraseña: obligatoria en el alta; en edición, en blanco = no cambiarla
            if (m.EsAlta && string.IsNullOrWhiteSpace(m.PasswordPlano))
                throw new ReglaNegocioException("La contraseña es obligatoria en un alta.");

            if (!string.IsNullOrWhiteSpace(m.PasswordPlano) &&
                m.PasswordPlano.Length < LargoMinimoPassword)
                throw new ReglaNegocioException(
                    "La contraseña debe tener al menos " + LargoMinimoPassword + " caracteres.");

            // Fecha de nacimiento
            if (m.FechaNacimiento.HasValue && m.FechaNacimiento.Value.Date < FechaNacimientoMinima)
                throw new ReglaNegocioException("La fecha de nacimiento no puede ser anterior a 1900.");

            if (m.FechaNacimiento.HasValue && m.FechaNacimiento.Value.Date > FechaNacimientoMaxima)
                throw new ReglaNegocioException(
                    "El usuario tiene que tener al menos " + EdadMinima + " años.");

            // Largos máximos (coinciden con las columnas de la base)
            ValidarLargo(m.DniCuit, LargoMaximoDniCuit, "El DNI");
            ValidarLargo(m.Nombre, LargoMaximoNombre, "El nombre");
            ValidarLargo(m.Apellido, LargoMaximoApellido, "El apellido");
            ValidarLargo(m.Direccion, LargoMaximoDireccion, "La dirección");
            ValidarLargo(m.Telefono, LargoMaximoTelefono, "El teléfono");
            ValidarLargo(m.Email, LargoMaximoEmail, "El email");

            // Va al final a propósito: es la única validación que consulta la base,
            // así no se hace el viaje si algún otro campo ya estaba mal.
            if (!string.IsNullOrWhiteSpace(m.Email))
            {
                int? personaQueSeEdita = m.EsAlta ? (int?)null : m.IdPersona;

                if (ServicioUsuario.ExisteEmail(m.Email.Trim(), personaQueSeEdita))
                    throw new ReglaNegocioException("Ya hay otra persona registrada con ese email.");
            }
        }

        private static void ValidarLargo(string valor, int maximo, string etiqueta)
        {
            if (!string.IsNullOrWhiteSpace(valor) && valor.Trim().Length > maximo)
                throw new ReglaNegocioException(
                    etiqueta + " no puede superar los " + maximo + " caracteres.");
        }

        /// <summary>
        /// Deja los datos listos para la base: sin espacios sobrantes y con null en los
        /// campos opcionales vacíos. La contraseña NO se recorta, los espacios son parte de ella.
        /// </summary>
        private static void Normalizar(UsuarioEditModel m)
        {
            m.DniCuit = m.DniCuit.Trim();
            m.Nombre = Validaciones.NormalizarEspacios(m.Nombre);
            m.Apellido = Validaciones.NormalizarEspacios(m.Apellido);

            m.Direccion = string.IsNullOrWhiteSpace(m.Direccion) ? null : m.Direccion.Trim();
            m.Telefono = string.IsNullOrWhiteSpace(m.Telefono) ? null : m.Telefono.Trim();
            m.Email = string.IsNullOrWhiteSpace(m.Email) ? null : m.Email.Trim();

            if (m.FechaNacimiento.HasValue)
                m.FechaNacimiento = m.FechaNacimiento.Value.Date;
        }

        // ---------------------------------------------------------------------
        // Auditoría y traducción de errores
        // ---------------------------------------------------------------------

        private static string Resumen(UsuarioEditModel m)
        {
            return string.Format(
                "DNI={0}; Nombre={1}, {2}; Dir={3}; Tel={4}; Email={5}; FNac={6}; Perfil={7}",
                m.DniCuit, m.Apellido, m.Nombre,
                string.IsNullOrWhiteSpace(m.Direccion) ? "-" : m.Direccion.Trim(),
                string.IsNullOrWhiteSpace(m.Telefono) ? "-" : m.Telefono.Trim(),
                string.IsNullOrWhiteSpace(m.Email) ? "-" : m.Email.Trim(),
                m.FechaNacimiento.HasValue ? m.FechaNacimiento.Value.ToString("yyyy-MM-dd") : "-",
                m.NombrePerfil);
        }

        private static ReglaNegocioException TraducirDuplicado(DuplicadoException ex)
        {
            if (ex.Restriccion.IndexOf("UQ_Persona_dni_cuit", StringComparison.OrdinalIgnoreCase) >= 0)
                return new ReglaNegocioException("Ya existe una persona registrada con ese DNI.");

            if (ex.Restriccion.IndexOf("UQ_Usuario_persona", StringComparison.OrdinalIgnoreCase) >= 0)
                return new ReglaNegocioException("La persona seleccionada ya tiene un usuario asociado.");

            return new ReglaNegocioException("Ya existe un registro con esos datos (valor duplicado).");
        }
    }
}
