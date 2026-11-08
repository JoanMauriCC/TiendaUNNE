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

    /// <summary>Una opción del filtro «Usuario»: quién hizo el movimiento (null = todos).</summary>
    public sealed class UsuarioAuditoria
    {
        public string Nombre { get; set; }
        public int? IdUsuario { get; set; }
        public override string ToString() => Nombre;
    }

    /// <summary>Una opción del filtro «Acción»: el nombre que se lee y el valor guardado en la base (null = todas).</summary>
    public sealed class AccionAuditoria
    {
        public string Nombre { get; set; }
        public string Valor { get; set; }
        public override string ToString() => Nombre;
    }

    /// <summary>Todo lo que se puede filtrar en la consulta de auditoría; lo que está vacío no filtra.</summary>
    public sealed class FiltroAuditoria
    {
        public string Tabla { get; set; }
        public int? IdUsuario { get; set; }
        public string Accion { get; set; }
        public System.DateTime? Desde { get; set; }
        public System.DateTime? Hasta { get; set; }
        public string Texto { get; set; }
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
