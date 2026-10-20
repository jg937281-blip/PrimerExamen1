# Inventario de tienda - Examen ISB-08

**Estudiante:** [Escriba su nombre]  
**Proyecto:** Opción 1 - Inventario de tienda  
**Lenguaje:** C#  
**Interfaz:** Windows Forms para .NET 8

## Requisitos
- Windows.
- Visual Studio 2022 con la carga de trabajo **Desarrollo de escritorio de .NET**.
- Componente .NET 8 para desarrollo de escritorio.

## Cómo abrir y ejecutar
1. Descomprima la carpeta.
2. Abra `InventarioTiendaISB08.csproj` en Visual Studio.
3. Si Visual Studio pregunta por la restauración de paquetes, acepte.
4. Seleccione la configuración `Debug` y ejecute con `F5` o el botón verde de inicio.

## Estructuras de datos utilizadas
| Estructura | Uso | Ubicación |
|---|---|---|
| Arreglo/vector | `categorias` y `existenciasPorCategoria` guardan categorías y total de unidades por categoría. Se accede y recorre mediante índices. | `MainForm`, métodos `RecalcularVector` y `ActualizarResumenVector`. |
| Colección dinámica | `List<Producto> catalogo` administra el catálogo y permite insertar, consultar, modificar, eliminar y ordenar. `List<T>` es el equivalente de ArrayList en C#. | `MainForm`, métodos `RegistrarProducto`, `BuscarProducto`, `ModificarProducto`, `EliminarProducto` y `OrdenarProductos`. |
| Lista enlazada simple propia | `NodoMovimiento` contiene el movimiento y el enlace `Siguiente`. `ListaMovimientos` implementa insertar, buscar, eliminar y recorrer nodos. | Clases `NodoMovimiento` y `ListaMovimientos`; se utiliza desde `MainForm`. |

## Datos de prueba
Al iniciar se cargan tres productos de ejemplo: Arroz, Agua y Jabón, además de movimientos iniciales. Para probar:
1. Registre un producto con nombre, categoría, precio y existencias.
2. Seleccione un producto y use Entrada o Salida. Escriba primero la cantidad en el campo `Cantidad de movimiento`.
3. Busque por nombre o parte del nombre.
4. Seleccione un producto, cambie los campos y presione Modificar seleccionado.
5. Ordene por nombre y por precio.
6. Busque o elimine un movimiento escribiendo su ID.

## Antes de entregar
- Reemplace `[Escriba su nombre]` por su nombre real.
- Revise y comprenda el código; debe poder explicarlo con sus propias palabras.
- Cree su propio repositorio de GitHub y suba el código y este README.
- Verifique que el profesor pueda acceder al repositorio.
