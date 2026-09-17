using System;

namespace TiendaUNNE
{
    /// <summary>Ítem de la tabla Perfil para enlazar al ComboBox (ValueMember = Id).</summary>
    public sealed class PerfilItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public override string ToString() => Nombre;
    }

    /// <summary>
    /// Datos combinados de Persona + Usuario que viajan entre ucUsuarios y ServicioUsuario.
    /// <see cref="IdUsuario"/> == 0 indica ALTA; distinto de 0 indica EDICIÓN.
    /// </summary>
    public sealed class UsuarioEditModel
    {
        public int IdUsuario { get; set; }
        public int IdPersona { get; set; }

        // Persona
        public string DniCuit { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        // Usuario (el login es con DniCuit; no tiene nombre propio)
        /// <summary>Contraseña en claro. En edición, vacío = no cambiarla.</summary>
        public string PasswordPlano { get; set; }
        public int IdPerfil { get; set; }
        public string NombrePerfil { get; set; }

        public bool EsAlta => IdUsuario == 0;
    }
}
