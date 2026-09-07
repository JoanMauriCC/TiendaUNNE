---
name: dotnet-solution-navigation
description: Mapa de la estructura del proyecto TiendaUNNE (solución .NET Framework 4.7.2 / WinForms / SQL Server) para ubicarse rápido sin tener que re-explorar todo el repo cada vez. Usar esta skill siempre que haya que encontrar dónde vive algo en el proyecto, entender qué carpeta es responsable de qué, agregar una pantalla o servicio nuevo, o cuando el usuario pregunte "dónde está X", "cómo se agrega un formulario/servicio nuevo", o mencione cualquiera de: Formularios, Datos, Modelo, Seguridad, Bootstrap, frmX, ServicioX, .slnx, App.config. Consultar esto ANTES de usar Glob/Grep para explorar la estructura desde cero.
---

# Navegación de la solución TiendaUNNE

Esta skill existe para no tener que re-descubrir la estructura del proyecto en cada conversación. Es un mapa, no un tutorial de WinForms — si necesitás detalle de sintaxis de WinForms, esa es otra skill.

## Por qué importa

TiendaUNNE sigue una separación en capas simple y consistente (UI en `Formularios/`, acceso a datos en `Datos/`, entidades en `Modelo/`). Antes de tocar código, ubicar la capa correcta evita mezclar responsabilidades — por ejemplo, escribir SQL directo en un formulario en vez de agregarlo a la clase `Servicio*` correspondiente.

## Estructura de la solución

```
TiendaUNNE_TallerDeProgramacion2.slnx   ← solución (formato .slnx, requiere VS 2022 17.10+)
TiendaUNNE_CreateTables.sql             ← script de creación de la base de datos SQL Server
TiendaUNNE/                             ← único proyecto, .NET Framework 4.7.2, WinForms (OutputType WinExe)
  App.config                           ← connection string a SQLEXPRESS (Integrated Security, TrustServerCertificate)
  Program.cs                           ← entry point (Main)
  Arranque/
    Bootstrap.cs                       ← lógica de primer arranque: crea el usuario admin inicial (admin / Admin.1234)
  Controles/
    TarjetaMenu.cs                     ← control de UI custom reutilizable
  Datos/                               ← CAPA DE ACCESO A DATOS — todo el SQL vive acá
    Db.cs                              ← helper de conexión (abrir/cerrar SqlConnection, ejecutar comandos)
    ReglaNegocioException.cs           ← excepción custom para validaciones de negocio
    ServicioAuditoria.cs
    ServicioAutenticacion.cs
    ServicioCategoria.cs
    ServicioPerfil.cs
    ServicioProducto.cs
    ServicioUsuario.cs
  Formularios/                         ← pantallas WinForms, siempre en pares
    frmLogin.cs / frmLogin.Designer.cs
    frmPrincipal.cs / frmPrincipal.Designer.cs
    frmUsuarios.cs / frmUsuarios.Designer.cs
    frmUsuarioEditor.cs / frmUsuarioEditor.Designer.cs
    frmCategorias.cs / frmCategorias.Designer.cs
    frmCategoriaEditor.cs / frmCategoriaEditor.Designer.cs
    frmProductos.cs / frmProductos.Designer.cs
    frmProductoEditor.cs / frmProductoEditor.Designer.cs
    frmProductosMenu.cs / frmProductosMenu.Designer.cs
    frmAuditoria.cs / frmAuditoria.Designer.cs
  Modelo/                              ← POCOs / entidades planas, sin lógica de acceso a datos
    ModelosCategoria.cs
    ModelosProducto.cs
    ModelosUsuario.cs
  Seguridad/
    PasswordHasher.cs                  ← hashing de contraseñas
    Sesion.cs                          ← estado de la sesión del usuario logueado
  Properties/
    AssemblyInfo.cs, Resources.resx, Settings.settings
```

## Cómo decidir dónde tocar código

| Necesito... | Voy a... |
|---|---|
| Agregar/editar una pantalla | `Formularios/frmX.cs` (lógica) + `frmX.Designer.cs` (layout, generado por el Designer — evitar editarlo a mano salvo que sepas lo que hacés) |
| Agregar una query o regla de negocio | La clase `Servicio*` correspondiente en `Datos/` (ej: todo lo de productos va en `ServicioProducto.cs`) |
| Agregar un campo/entidad nueva | `Modelo/ModelosX.cs` + la tabla correspondiente en `TiendaUNNE_CreateTables.sql` |
| Cambiar cómo se conecta a la base | `Datos/Db.cs` o `App.config` (connection string) |
| Tocar login/permisos/sesión | `Seguridad/` y `Datos/ServicioAutenticacion.cs` |
| Cambiar qué pasa en el primer arranque (usuario admin, seed data) | `Arranque/Bootstrap.cs` |

## Convención de nombres

- Formularios: prefijo `frm` + nombre en PascalCase (`frmProductoEditor`)
- Servicios de datos: prefijo `Servicio` + entidad (`ServicioProducto`)
- Cada formulario editor (`frmXEditor`) acompaña a un listado (`frmX`) — es el patrón "listado + editor modal" repetido para Usuarios, Categorías y Productos

## Al agregar un formulario o servicio nuevo

Seguir el patrón existente en vez de inventar uno nuevo:
1. Nuevo servicio → nueva clase `Servicio<Entidad>.cs` en `Datos/`, usando `Db.cs` para la conexión (nunca abrir un `SqlConnection` a mano fuera de ahí)
2. Nuevo listado + editor → copiar el patrón de `frmProductos`/`frmProductoEditor` (es el más completo) en vez de `frmCategorias` (más simple) si la entidad tiene relaciones
3. Agregar la entidad al proyecto en el `.csproj` — Visual Studio lo hace solo si usás el Designer/Add New Item, pero si se crea el archivo a mano hay que agregarlo al `<ItemGroup>` de `TiendaUNNE.csproj`
