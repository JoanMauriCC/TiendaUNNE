namespace TiendaUNNE
{
    /// <summary>Un cliente tal como se lo busca y se lo elige desde la caja.</summary>
    public sealed class ClienteItem
    {
        public int IdCliente { get; set; }
        public string Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string RazonSocial { get; set; }
        public string Cuit { get; set; }
        public string CondicionIva { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }

        public string NombreCompleto => Apellido + ", " + Nombre;

        public override string ToString() => NombreCompleto + "  ·  DNI " + Dni;
    }

    /// <summary>Datos que se cargan para dar de alta un cliente nuevo. Solo datos: valida NegocioCliente.</summary>
    public sealed class ClienteEditModel
    {
        public string Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string RazonSocial { get; set; }
        public string Cuit { get; set; }
        public string CondicionIva { get; set; }
    }
}
