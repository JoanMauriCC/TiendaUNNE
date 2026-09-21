using System.Windows.Forms;

namespace TiendaUNNE
{
    /// <summary>Qué tiene que hacer una pantalla cuando se aprieta un atajo de teclado.</summary>
    public enum AccionAtajo
    {
        Ninguna,
        Guardar,
        Limpiar,
        Buscar,
        Actualizar
    }

    /// <summary>
    /// Atajos de teclado comunes a las pantallas de carga (Usuarios, Productos, Categorías).
    /// Interpretar la tecla es lo único que hace esta clase: qué significa "guardar" o
    /// "limpiar" en cada pantalla lo resuelve la pantalla, que llama a lo suyo.
    ///
    ///   Enter        guardar, si el foco está en un campo del formulario
    ///   Ctrl+S       guardar, esté donde esté el foco
    ///   Esc          limpiar el formulario (cierra primero una lista desplegada)
    ///   Ctrl+N       limpiar el formulario para cargar uno nuevo
    ///   Ctrl+F       ir al buscador
    ///   F5           actualizar el listado
    ///   Alt+letra    ir a un campo (la letra subrayada de cada etiqueta)
    /// </summary>
    public static class AtajosTeclado
    {
        /// <param name="teclas">Tecla apretada (con sus modificadores).</param>
        /// <param name="activo">Control que tiene el foco ahora.</param>
        /// <param name="formulario">Panel que contiene los campos de carga.</param>
        /// <param name="buscador">Caja de búsqueda: Enter ahí no guarda.</param>
        public static AccionAtajo Interpretar(Keys teclas, Control activo, Control formulario, Control buscador)
        {
            switch (teclas)
            {
                case Keys.Control | Keys.S:
                    return AccionAtajo.Guardar;

                case Keys.Control | Keys.N:
                    return AccionAtajo.Limpiar;

                case Keys.Control | Keys.F:
                    return AccionAtajo.Buscar;

                case Keys.F5:
                    return AccionAtajo.Actualizar;

                case Keys.Escape:
                    // Si hay una lista desplegada, Esc solo la cierra: no borra lo cargado.
                    var combo = activo as ComboBox;
                    return combo != null && combo.DroppedDown ? AccionAtajo.Ninguna : AccionAtajo.Limpiar;

                case Keys.Enter:
                    return EsCampoDeCarga(activo, formulario, buscador)
                        ? AccionAtajo.Guardar
                        : AccionAtajo.Ninguna;

                default:
                    return AccionAtajo.Ninguna;
            }
        }

        /// <summary>
        /// Enter guarda solo desde un campo de carga. No desde un botón (Enter ya lo aprieta),
        /// ni desde el buscador, ni desde un texto de varias líneas (ahí Enter es un salto de
        /// línea), ni con una lista desplegada (ahí Enter elige un ítem).
        /// </summary>
        private static bool EsCampoDeCarga(Control activo, Control formulario, Control buscador)
        {
            if (activo == null || activo == buscador || !formulario.Contains(activo))
                return false;

            if (activo is ButtonBase || activo is TarjetaAccion)
                return false;

            var texto = activo as TextBox;
            if (texto != null && texto.Multiline)
                return false;

            var combo = activo as ComboBox;
            if (combo != null && combo.DroppedDown)
                return false;

            return true;
        }
    }
}
