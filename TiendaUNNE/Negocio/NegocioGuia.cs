using System;

namespace TiendaUNNE
{
    /// <summary>
    /// Contenido y reglas de la ayuda (visita guiada): qué capítulo explica cada pantalla y
    /// cuándo se muestra la Bienvenida sola. No sabe de controles: cada paso señala una parte
    /// de la pantalla con una clave de AnclaGuia, y la pantalla la traduce a su control.
    /// </summary>
    public static class NegocioGuia
    {
        /// <summary>
        /// El capítulo que se recorre al pedir ayuda en una pantalla. La Caja tiene dos: uno para
        /// abrir el turno y otro para vender, según si hay un turno abierto. Una pantalla que el rol
        /// no puede abrir, o que no tiene capítulo propio, ofrece la Bienvenida.
        /// </summary>
        public static CapituloGuia CapituloDe(OpcionMenu pantalla)
        {
            if (pantalla != OpcionMenu.Inicio && !NegocioSeguridad.PuedeAcceder(pantalla))
                return Bienvenida();

            switch (pantalla)
            {
                case OpcionMenu.Caja: return CajaAbierta() ? Caja() : CajaApertura();
                case OpcionMenu.Productos: return Productos();
                case OpcionMenu.Categorias: return Categorias();
                case OpcionMenu.Usuarios: return Usuarios();
                case OpcionMenu.Reportes: return Reportes();
                case OpcionMenu.Auditoria: return Auditoria();
                case OpcionMenu.Backup: return Backup();
                default: return Bienvenida();
            }
        }

        /// <summary>La Bienvenida se muestra sola la primera vez que el usuario entra al sistema.</summary>
        public static bool DebeMostrarBienvenida(UsuarioLogueado usuario)
        {
            return usuario != null && usuario.PrimerIngreso;
        }

        private static bool CajaAbierta()
        {
            try { return NegocioCaja.ObtenerSesionAbierta() != null; }
            catch (Exception) { return true; }   // sin base, se explica la venta (lo más usado)
        }

        // ---------------------------------------------------------------------
        // Capítulos
        // ---------------------------------------------------------------------

        public static CapituloGuia Bienvenida()
        {
            var c = new CapituloGuia { Nombre = "Bienvenida" };

            Paso(c, null, "Bienvenido a TiendaUNNE",
                "Esta guía te muestra en un minuto cómo está organizado el sistema. " +
                "Podés volver a verla cuando quieras con el botón ? de arriba o con la tecla F1.");
            Paso(c, AnclaGuia.Menu, "El menú",
                "Desde acá llegás a cada módulo. Solo ves los que tu rol puede usar: " +
                "si falta alguno, es porque tu usuario no tiene permiso para esa parte.");
            Paso(c, AnclaGuia.Contenido, "Tu espacio de trabajo",
                "Acá se abre el módulo que elijas. En Inicio tenés accesos rápidos: " +
                "te movés entre ellos con las flechas y los abrís con Enter.");
            Paso(c, AnclaGuia.Titulo, "Dónde estás",
                "El título dice en qué módulo estás. En los formularios, Alt más la letra " +
                "subrayada de cada campo te lleva directo a ese campo.");
            Paso(c, AnclaGuia.Ayuda, "La ayuda",
                "En cada pantalla, este botón (o F1) recorre esa pantalla paso a paso.");
            Paso(c, AnclaGuia.Sesion, "Tu sesión",
                "Acá ves con qué usuario y rol entraste. Cerrá la sesión al terminar, " +
                "sobre todo si la computadora es compartida.");
            return c;
        }

        private static CapituloGuia CajaApertura()
        {
            var c = new CapituloGuia { Nombre = "Caja: apertura" };

            Paso(c, AnclaGuia.CajaApertura, "Abrir el turno",
                "Para vender, primero se abre la caja con el efectivo que hay en el cajón. " +
                "Ese monto es el punto de partida del arqueo al cerrar.");
            Paso(c, AnclaGuia.CajaContarBilletes, "Monto o conteo de billetes",
                "Podés escribir el total directamente o contar billete por billete y que el " +
                "sistema sume solo.");
            Paso(c, AnclaGuia.CajaAbrir, "Abrir la caja (Ctrl+Enter)",
                "Si el monto no coincide con lo que se contó al cerrar el turno anterior, el sistema " +
                "pide anotar el motivo, y queda en la auditoría.");
            return c;
        }

        private static CapituloGuia Caja()
        {
            var c = new CapituloGuia { Nombre = "Caja" };

            Paso(c, AnclaGuia.CajaBuscador, "Buscar productos (F2)",
                "Escribí parte del nombre para filtrar el catálogo. F2 o Ctrl+F te traen acá " +
                "desde cualquier lado y Esc limpia la búsqueda.");
            Paso(c, AnclaGuia.CajaCategorias, "Categorías",
                "Tocá una categoría para ver solo sus productos.");
            Paso(c, AnclaGuia.CajaCatalogo, "El catálogo",
                "Un clic agrega el producto al ticket. El stock que ves ya descuenta lo que tienen " +
                "reservado otras cajas. F5 lo vuelve a leer.");
            Paso(c, AnclaGuia.CajaTicket, "El ticket",
                "Lo cargado queda reservado para esta venta. Con los botones de abajo sumás o " +
                "restás unidades, quitás un renglón o cancelás la venta (y la reserva se libera).");
            Paso(c, AnclaGuia.CajaCliente, "Cliente y comprobante",
                "Con F3 elegís o cargás el cliente; sin cliente es consumidor final. Abajo elegís " +
                "Ticket, Factura B o Factura A (esta, solo para Responsables Inscriptos con CUIT).");
            Paso(c, AnclaGuia.CajaCobrar, "Cobrar (F4)",
                "Elegís los medios de pago; si se paga en efectivo, calcula el vuelto. Al confirmar " +
                "se descuenta el stock y se emite el comprobante.");
            Paso(c, AnclaGuia.CajaHistorial, "Historial de ventas (F6)",
                "Para volver a imprimir un comprobante o hacer una nota de crédito, que devuelve " +
                "el stock y la plata.");
            Paso(c, AnclaGuia.CajaCerrar, "Cerrar la caja",
                "Al final del turno contás el efectivo y el sistema lo compara con lo que debería " +
                "haber. Si hay diferencia, queda registrada.");
            return c;
        }

        private static CapituloGuia Productos()
        {
            var c = new CapituloGuia { Nombre = "Productos" };

            Paso(c, AnclaGuia.ProductosFormulario, "El formulario",
                "Para cargar un producto nuevo o editar el elegido en la lista. Los campos con * son " +
                "obligatorios y el precio no puede ser negativo.");
            Paso(c, AnclaGuia.ProductosBuscar, "Buscar",
                "Filtra la lista mientras escribís y carga la primera coincidencia en el formulario.");
            Paso(c, AnclaGuia.ProductosLista, "La lista",
                "Reservado es lo que hay en tickets en curso y Disponible lo que queda para vender. " +
                "Estado avisa cuando el stock está bajo o agotado.");
            Paso(c, AnclaGuia.ProductosInactivos, "Activos e inactivos",
                "Alterna entre los productos a la venta y los dados de baja.");
            Paso(c, AnclaGuia.ProductosBaja, "Dar de baja",
                "Lo saca de la venta sin borrarlo. No se puede si algún ticket abierto lo tiene reservado.");
            return c;
        }

        private static CapituloGuia Categorias()
        {
            var c = new CapituloGuia { Nombre = "Categorías" };

            Paso(c, AnclaGuia.CategoriasFormulario, "El formulario",
                "Nombre y descripción de la categoría. El nombre no se puede repetir.");
            Paso(c, AnclaGuia.CategoriasBuscar, "Buscar",
                "Filtra la lista mientras escribís.");
            Paso(c, AnclaGuia.CategoriasFiltro, "Activas e inactivas",
                "Elegí si ver las categorías en uso o las dadas de baja.");
            Paso(c, AnclaGuia.CategoriasLista, "La lista",
                "Elegí una categoría para editarla en el formulario.");
            Paso(c, AnclaGuia.CategoriasBaja, "Dar de baja",
                "No se puede dar de baja una categoría que todavía tiene productos activos.");
            return c;
        }

        private static CapituloGuia Usuarios()
        {
            var c = new CapituloGuia { Nombre = "Usuarios" };

            Paso(c, AnclaGuia.UsuariosFormulario, "El formulario",
                "El DNI tiene 8 números y al escribirlo aparecen las personas que ya existen. " +
                "Al editar, la contraseña en blanco no se cambia. El perfil define qué puede hacer.");
            Paso(c, AnclaGuia.UsuariosBuscar, "Buscar",
                "Filtra la lista por DNI, apellido, nombre o email.");
            Paso(c, AnclaGuia.UsuariosLista, "La lista",
                "Los usuarios bloqueados aparecen en rojo.");
            Paso(c, AnclaGuia.UsuariosBaja, "Dar de baja",
                "El usuario deja de poder entrar, sin borrarlo. No podés darte de baja a vos mismo.");
            Paso(c, AnclaGuia.UsuariosDesbloquear, "Desbloquear",
                "Un usuario se bloquea con " + NegocioAutenticacion.IntentosParaBloquear +
                " contraseñas mal seguidas. Desbloqueado, vuelve a entrar con su contraseña de siempre.");
            return c;
        }

        private static CapituloGuia Reportes()
        {
            var c = new CapituloGuia { Nombre = "Reportes" };

            Paso(c, AnclaGuia.ReportesTipos, "Qué reporte ver",
                "Ventas, Productos, Recaudación o Clientes. Todos salen de las ventas guardadas; " +
                "las anuladas no cuentan.");
            Paso(c, AnclaGuia.ReportesFechas, "El período",
                "Elegí desde y hasta y tocá Aplicar. Un solo día se ve por hora; hasta un mes, por día; " +
                "más largo, por mes.");
            Paso(c, AnclaGuia.ReportesIndicadores, "Los números clave",
                "El resumen del período elegido.");
            Paso(c, AnclaGuia.ReportesTipoGrafico, "Tipo de gráfico",
                "Barras, bastones, línea, escalonado, ojiva o torta: solo se ofrecen los que tienen " +
                "sentido para los datos de cada reporte.");
            Paso(c, AnclaGuia.ReportesDetalle, "El detalle",
                "La tabla con los datos del gráfico.");
            Paso(c, AnclaGuia.ReportesPdf, "Exportar PDF (Alt+E)",
                "Guarda el reporte que estás viendo, con el gráfico elegido, listo para imprimir o mandar.");
            return c;
        }

        private static CapituloGuia Auditoria()
        {
            var c = new CapituloGuia { Nombre = "Auditoría" };

            Paso(c, AnclaGuia.AuditoriaFiltros, "Filtros",
                "Por módulo, usuario, acción, fechas o un texto. La búsqueda no distingue tildes ni mayúsculas.");
            Paso(c, AnclaGuia.AuditoriaRapidos, "Atajos de fecha",
                "Hoy, los últimos 7 o los últimos 30 días, con un clic.");
            Paso(c, AnclaGuia.AuditoriaLista, "Los movimientos",
                "Cada cambio del sistema resumido en una frase. Los sensibles (notas de crédito, " +
                "restauraciones, diferencias de caja, bloqueos) aparecen resaltados.");
            Paso(c, AnclaGuia.AuditoriaDetalle, "El detalle",
                "Del movimiento elegido: qué datos cambiaron, cómo estaban antes y cómo quedaron.");
            return c;
        }

        private static CapituloGuia Backup()
        {
            var c = new CapituloGuia { Nombre = "Backup" };

            Paso(c, AnclaGuia.BackupResumen, "Resumen",
                "En qué carpeta se guardan las copias, cuántas hay y cuándo se hizo la última.");
            Paso(c, AnclaGuia.BackupLista, "Las copias",
                "Las manuales y las que el sistema hace solo antes de cada restauración.");
            Paso(c, AnclaGuia.BackupCrear, "Crear una copia",
                "Guarda el estado actual de toda la base en un archivo.");
            Paso(c, AnclaGuia.BackupRestaurar, "Restaurar (dos llaves)",
                "Vuelve la base al momento de la copia elegida. Pide tu contraseña y la de otro " +
                "Administrador o Supervisor, guarda antes una copia del estado actual y queda en la auditoría.");
            Paso(c, AnclaGuia.BackupCopiar, "Guardar en otro lugar",
                "Copia el archivo elegido a otra carpeta o a un pendrive.");
            Paso(c, AnclaGuia.BackupDetalle, "El detalle",
                "Los datos de la copia elegida.");
            return c;
        }

        private static void Paso(CapituloGuia capitulo, string ancla, string titulo, string texto)
        {
            capitulo.Pasos.Add(new PasoGuia { Ancla = ancla, Titulo = titulo, Texto = texto });
        }
    }
}
