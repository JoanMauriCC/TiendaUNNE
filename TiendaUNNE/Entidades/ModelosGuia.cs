using System.Collections.Generic;

namespace TiendaUNNE
{
    /// <summary>
    /// Un paso de la visita guiada: qué parte de la pantalla se señala (Ancla) y qué se dice.
    /// El ancla es una clave, no un control: Negocio no sabe de botones ni grillas, y cada
    /// pantalla traduce la clave a su control. Sin ancla (null) el paso se muestra centrado.
    /// </summary>
    public sealed class PasoGuia
    {
        public string Ancla { get; set; }
        public string Titulo { get; set; }
        public string Texto { get; set; }
    }

    /// <summary>Un capítulo de la guía: los pasos que explican una pantalla.</summary>
    public sealed class CapituloGuia
    {
        public string Nombre { get; set; }
        public List<PasoGuia> Pasos { get; } = new List<PasoGuia>();
    }

    /// <summary>
    /// Las claves de las partes que la guía puede señalar. Están acá, en Entidades, porque las
    /// usan las dos capas: Negocio las pone en los pasos y Presentación las traduce a controles.
    /// </summary>
    public static class AnclaGuia
    {
        // Ventana principal (presentes en todas las pantallas)
        public const string Menu = "Principal.Menu";
        public const string Titulo = "Principal.Titulo";
        public const string Ayuda = "Principal.Ayuda";
        public const string Sesion = "Principal.Sesion";
        public const string Contenido = "Principal.Contenido";

        // Caja cerrada (apertura del turno)
        public const string CajaApertura = "Caja.Apertura";
        public const string CajaContarBilletes = "Caja.ContarBilletes";
        public const string CajaAbrir = "Caja.Abrir";

        // Caja abierta (venta)
        public const string CajaBuscador = "Caja.Buscador";
        public const string CajaCategorias = "Caja.Categorias";
        public const string CajaCatalogo = "Caja.Catalogo";
        public const string CajaTicket = "Caja.Ticket";
        public const string CajaCliente = "Caja.Cliente";
        public const string CajaCobrar = "Caja.Cobrar";
        public const string CajaHistorial = "Caja.Historial";
        public const string CajaCerrar = "Caja.Cerrar";

        // Productos
        public const string ProductosFormulario = "Productos.Formulario";
        public const string ProductosBuscar = "Productos.Buscar";
        public const string ProductosLista = "Productos.Lista";
        public const string ProductosInactivos = "Productos.Inactivos";
        public const string ProductosBaja = "Productos.Baja";

        // Categorías
        public const string CategoriasFormulario = "Categorias.Formulario";
        public const string CategoriasBuscar = "Categorias.Buscar";
        public const string CategoriasFiltro = "Categorias.Filtro";
        public const string CategoriasLista = "Categorias.Lista";
        public const string CategoriasBaja = "Categorias.Baja";

        // Usuarios
        public const string UsuariosFormulario = "Usuarios.Formulario";
        public const string UsuariosBuscar = "Usuarios.Buscar";
        public const string UsuariosLista = "Usuarios.Lista";
        public const string UsuariosBaja = "Usuarios.Baja";
        public const string UsuariosDesbloquear = "Usuarios.Desbloquear";

        // Reportes
        public const string ReportesTipos = "Reportes.Tipos";
        public const string ReportesFechas = "Reportes.Fechas";
        public const string ReportesIndicadores = "Reportes.Indicadores";
        public const string ReportesTipoGrafico = "Reportes.TipoGrafico";
        public const string ReportesDetalle = "Reportes.Detalle";
        public const string ReportesPdf = "Reportes.Pdf";

        // Auditoría
        public const string AuditoriaFiltros = "Auditoria.Filtros";
        public const string AuditoriaRapidos = "Auditoria.Rapidos";
        public const string AuditoriaLista = "Auditoria.Lista";
        public const string AuditoriaDetalle = "Auditoria.Detalle";

        // Backup
        public const string BackupResumen = "Backup.Resumen";
        public const string BackupLista = "Backup.Lista";
        public const string BackupCrear = "Backup.Crear";
        public const string BackupRestaurar = "Backup.Restaurar";
        public const string BackupCopiar = "Backup.Copiar";
        public const string BackupDetalle = "Backup.Detalle";

        // Asistente
        public const string AsistenteConversacion = "Asistente.Conversacion";
        public const string AsistenteSugerencias = "Asistente.Sugerencias";
        public const string AsistentePregunta = "Asistente.Pregunta";
        public const string AsistenteNueva = "Asistente.Nueva";
    }
}
