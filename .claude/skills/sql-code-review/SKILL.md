---
name: sql-code-review
description: Checklist para revisar código SQL y acceso a datos en TiendaUNNE — tanto queries dentro de las clases Servicio*.cs (carpeta Datos/) como el script TiendaUNNE_CreateTables.sql. Usar esta skill SIEMPRE que se escriba o revise una query, se agregue un método nuevo a un Servicio*.cs, se modifique Db.cs, se toque el script de creación de tablas, o el usuario pida revisar/auditar el acceso a datos, pregunte por SQL injection, o mencione cualquiera de: query, consulta SQL, SqlCommand, parámetros, transacción, ServicioProducto, ServicioUsuario, ServicioCategoria, ServicioAuditoria, ServicioPerfil, ServicioAutenticacion. No es solo para bugs de seguridad — también cubre consistencia de convenciones y manejo correcto de conexiones.
---

# Revisión de código SQL en TiendaUNNE

Checklist para revisar cualquier código que toque la base de datos: las clases `Servicio*.cs` en `Datos/` (ver [[dotnet-solution-navigation]] para el mapa completo del proyecto) y el script `TiendaUNNE_CreateTables.sql`.

## Por qué importa

Es una app WinForms con `System.Data.SqlClient` clásico (ADO.NET, no un ORM) — no hay una capa que te proteja automáticamente de errores comunes como concatenar strings en una query. Cada `Servicio*.cs` construye SQL a mano, así que cada uno es un lugar donde se puede colar una vulnerabilidad o una fuga de conexión si no se revisa con cuidado.

## 1. Prevención de SQL injection

Lo más importante a chequear primero. Buscar cualquier concatenación de valores de usuario dentro de un string SQL:

**Mal** (vulnerable):
```csharp
var query = $"SELECT * FROM Producto WHERE Nombre LIKE '%{nombre}%'";
```

**Bien** (parametrizado):
```csharp
var query = "SELECT * FROM Producto WHERE Nombre LIKE @Nombre";
cmd.Parameters.AddWithValue("@Nombre", "%" + nombre + "%");
```

Regla simple: si un valor viene de un parámetro del método, un campo de formulario, o cualquier input que no sea una constante literal en el código, tiene que entrar como `SqlParameter`, nunca interpolado o concatenado directamente en el string de la query. Esto aplica también a nombres de columnas u ordenamiento dinámico (`ORDER BY {columna}`) — ahí no se puede parametrizar directamente, así que hay que validar contra una lista blanca fija de columnas permitidas.

## 2. Manejo de conexiones

Toda conexión debe pasar por `Datos/Db.cs`, nunca instanciar un `SqlConnection` suelto en un `Servicio*.cs` o, peor, en un formulario. Verificar que:
- Las conexiones y comandos estén dentro de un `using` (o try/finally con `Dispose()`) — una conexión sin cerrar es una fuga de recursos que en SQL Express agota el pool de conexiones rápido
- No se abra una conexión más tiempo del necesario (abrir, ejecutar, cerrar — no dejarla abierta durante lógica de UI)
- Si `Db.cs` ya expone un método helper para ejecutar comandos (ej. `EjecutarQuery`, `EjecutarNonQuery`), usarlo en vez de repetir el boilerplate de abrir conexión + crear comando en cada `Servicio*.cs`

## 3. Transacciones

Cuando una operación toca más de una tabla y ambas escrituras tienen que tener éxito o fallar juntas (por ejemplo, dar de baja un producto y registrar la baja en auditoría), verificar que estén envueltas en una `SqlTransaction` con commit/rollback explícito. Una operación de una sola sentencia `INSERT`/`UPDATE`/`DELETE` no necesita transacción explícita.

## 4. Integridad referencial

Al revisar el script `TiendaUNNE_CreateTables.sql` o una migración nueva:
- Toda FK debe tener su `FOREIGN KEY` declarada explícitamente, no solo asumida por convención de nombre
- Decidir a propósito el comportamiento de `ON DELETE` (¿`CASCADE`, `NO ACTION`, o soft-delete con una columna `Activo`/`Eliminado`?) — para este proyecto, dado que hay `ServicioAuditoria`, es más probable que el patrón sea soft-delete antes que borrado físico; si un `Servicio*.cs` hace `DELETE` físico sobre una entidad auditada, señalarlo como inconsistencia
- Nombres de columnas y tablas deben seguir la convención ya usada en el script existente (revisar `TiendaUNNE_CreateTables.sql` para el estilo exacto antes de agregar tablas nuevas, en vez de asumir una convención genérica)

## 5. Consistencia con el resto del proyecto

Antes de aprobar un método nuevo en un `Servicio*.cs`, comparar su forma con los métodos existentes en esa misma clase (o en una clase hermana como `ServicioCategoria.cs` si `ServicioProducto.cs` es el que se está editando): mismo estilo de manejo de excepciones (¿usa `ReglaNegocioException` para validaciones de negocio?), mismo patrón de apertura de conexión, mismo estilo de nombres de parámetros SQL (`@NombreParametro` en PascalCase). Un método que resuelve el problema de forma correcta pero con un estilo distinto al resto del archivo igual vale la pena señalarlo.
