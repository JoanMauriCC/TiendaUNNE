using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>
    /// Lo que cumple una pantalla que la ayuda puede recorrer: traducir una clave de AnclaGuia
    /// («Principal.Menu», «Caja.Buscador»...) al control que la representa. Así el contenido de
    /// la guía (Negocio) no conoce los controles, y renombrar uno solo toca su pantalla.
    /// </summary>
    public interface IPantallaConGuia
    {
        /// <summary>El control de esa clave, o null si esta pantalla no lo tiene o no está a la vista.</summary>
        Control Ancla(string clave);
    }
}
