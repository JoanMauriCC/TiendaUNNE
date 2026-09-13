using System.Collections.Generic;

namespace TiendaUNNE
{
    /// <summary>
    /// Perfiles disponibles para asignar a un usuario. Hoy no tiene reglas propias,
    /// pero existe igual para que la Presentación no tenga que hablarle a la capa de Datos.
    /// </summary>
    public static class NegocioPerfil
    {
        public static List<PerfilItem> ListarActivos() => ServicioPerfil.ListarActivos();
    }
}
