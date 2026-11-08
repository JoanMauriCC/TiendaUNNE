using System.Collections.Generic;

namespace TiendaUNNE
{
    /// <summary>Una línea del detalle: un dato, cómo estaba y cómo quedó.</summary>
    public sealed class FilaDetalle
    {
        public string Campo { get; set; }
        public string Antes { get; set; }
        public string Despues { get; set; }
    }

    /// <summary>
    /// Lo que se muestra al elegir un movimiento. Si se pudieron separar los datos, vienen en
    /// <see cref="Filas"/> (solo los que cambiaron); si no, el texto completo en <see cref="TextoAntes"/>
    /// y <see cref="TextoDespues"/>, una idea por renglón.
    /// </summary>
    public sealed class DetalleAuditoria
    {
        public List<FilaDetalle> Filas { get; } = new List<FilaDetalle>();
        public string TextoAntes { get; set; }
        public string TextoDespues { get; set; }
        public bool TieneFilas => Filas.Count > 0;
    }

    /// <summary>
    /// Un módulo del sistema sobre el que se registran movimientos, con el nombre que ve el usuario
    /// y la tabla de la base que le corresponde (null = todos los módulos).
    /// </summary>
    public sealed class ModuloAuditoria
    {
        public string Nombre { get; set; }
        public string Tabla { get; set; }
        public override string ToString() => Nombre;
    }
}
