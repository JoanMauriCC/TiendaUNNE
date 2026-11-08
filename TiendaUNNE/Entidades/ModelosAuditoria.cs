namespace TiendaUNNE
{
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
