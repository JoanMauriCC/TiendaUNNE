using System.Collections.Generic;

namespace TiendaUNNE
{
    /// <summary>
    /// Faltan campos obligatorios. Además del mensaje, lleva cuáles son, para que la
    /// pantalla pueda marcarlos. Hereda de ReglaNegocioException, así cualquier código
    /// que ya capture esa excepción sigue mostrando el mensaje sin cambios.
    /// </summary>
    public class CamposIncompletosException : ReglaNegocioException
    {
        public CamposIncompletosException(IReadOnlyList<CampoUsuario> campos)
            : base("Debes llenar todos los campos obligatorios.")
        {
            Campos = campos;
        }

        /// <summary>Campos que quedaron vacíos.</summary>
        public IReadOnlyList<CampoUsuario> Campos { get; }
    }
}
