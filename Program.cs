using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InventarioTiendaISB08
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Precio { get; set; }
        public int Existencias { get; set; }

        public override string ToString() =>
            $"{Id} - {Nombre} ({Categoria}) | ₡{Precio:N2} | Stock: {Existencias}";
    }

    public class Movimiento
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = "";
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = "";
        public int Cantidad { get; set; }
        public string Detalle { get; set; } = "";

        public override string ToString() =>
            $"Mov. {Id} | {Fecha:dd/MM HH:mm} | {Tipo} | {ProductoNombre} | Cant.: {Cantidad} | {Detalle}";
    }

    // Lista enlazada simple propia: cada nodo guarda un movimiento y apunta al siguiente.
    public class NodoMovimiento
    {
        public Movimiento Dato;
        public NodoMovimiento? Siguiente;

        public NodoMovimiento(Movimiento dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }

    public class ListaMovimientos
    {
        private NodoMovimiento? cabeza;

        public bool EstaVacia => cabeza == null;

        public void Insertar(Movimiento movimiento)
        {
            NodoMovimiento nuevo = new NodoMovimiento(movimiento);
            if (cabeza == null)
            {
                cabeza = nuevo;
                return;
            }

            NodoMovimiento actual = cabeza;
            while (actual.Siguiente != null)
                actual = actual.Siguiente;
            actual.Siguiente = nuevo;
        }

        public Movimiento? Buscar(int id)
        {
            NodoMovimiento? actual = cabeza;
            while (actual != null)
            {
                if (actual.Dato.Id == id)
                    return actual.Dato;
                actual = actual.Siguiente;
            }
            return null;
        }

        public bool Eliminar(int id)
        {
            if (cabeza == null) return false;

            if (cabeza.Dato.Id == id)
            {
                cabeza = cabeza.Siguiente;
                return true;
            }

            NodoMovimiento? anterior = cabeza;
            NodoMovimiento? actual = cabeza.Siguiente;

            while (actual != null)
            {
                if (actual.Dato.Id == id)
                {
                    anterior!.Siguiente = actual.Siguiente;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false;
        }

        public List<Movimiento> Recorrer()
        {
            List<Movimiento> resultado = new List<Movimiento>();
            NodoMovimiento? actual = cabeza;
            while (actual != null)
            {
                resultado.Add(actual.Dato);
                actual = actual.Siguiente;
            }
            return resultado;
        }
    }

    public class MainForm : Form
    {
        // ARREGLO/VECTOR: almacena la suma de existencias por categoría.
        private readonly string[] categorias = { "Alimentos", "Bebidas", "Limpieza", "Hogar", "Otros" };
        private readonly int[] existenciasPorCategoria = new int[5];

        // COLECCIÓN DINÁMICA equivalente a ArrayList en C#: List<Producto>.
        private readonly List<Producto> catalogo = new List<Producto>();

        // LISTA ENLAZADA propia para los movimientos de inventario.
        private readonly ListaMovimientos movimientos = new ListaMovimientos();

        private int siguienteProductoId = 1;
        private int siguienteMovimientoId = 1;

        private readonly TextBox txtNombre = new TextBox();
        private readonly ComboBox cmbCategoria = new ComboBox();
        private readonly TextBox txtPrecio = new TextBox();
        private readonly TextBox txtExistencias = new TextBox();
        private readonly TextBox txtBuscar = new TextBox();
        private readonly TextBox txtCantidadMovimiento = new TextBox();
        private readonly TextBox txtIdMovimiento = new TextBox();
        private readonly DataGridView dgvProductos = new DataGridView();
        private readonly ListBox lstMovimientos = new ListBox();
        private readonly ListBox lstResumen = new ListBox();
        private readonly Label lblEstado = new Label();

        public MainForm()
        {
            Text = "Sistema de Inventario de Tienda - ISB-08";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 780);
            MinimumSize = new Size(1050, 700);
            Font = new Font("Segoe UI", 9F);
            ConstruirInterfaz();
            CargarDatosEjemplo();
            ActualizarTodo();
        }

        private void ConstruirInterfaz()
        {
            var principal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };
            principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
            principal.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            principal.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            Controls.Add(principal);

            var grpDatos = new GroupBox { Text = "Datos del producto", Dock = DockStyle.Fill, Padding = new Padding(10) };
            principal.Controls.Add(grpDatos, 0, 0);
            principal.SetColumnSpan(grpDatos, 2);

            var datos = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 3 };
            datos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            datos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            datos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            datos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            datos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            datos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int i = 0; i < 3; i++) datos.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3F));
            grpDatos.Controls.Add(datos);

            AddLabel(datos, "Nombre:", 0, 0);
            txtNombre.Dock = DockStyle.Fill;
            datos.Controls.Add(txtNombre, 1, 0);
            AddLabel(datos, "Categoría:", 2, 0);
            cmbCategoria.Dock = DockStyle.Fill;
            cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategoria.Items.AddRange(categorias);
            cmbCategoria.SelectedIndex = 0;
            datos.Controls.Add(cmbCategoria, 3, 0);
            AddLabel(datos, "Precio ₡:", 4, 0);
            txtPrecio.Dock = DockStyle.Fill;
            datos.Controls.Add(txtPrecio, 5, 0);

            AddLabel(datos, "Existencias:", 0, 1);
            txtExistencias.Dock = DockStyle.Fill;
            datos.Controls.Add(txtExistencias, 1, 1);
            AddLabel(datos, "Buscar producto:", 2, 1);
            txtBuscar.Dock = DockStyle.Fill;
            datos.Controls.Add(txtBuscar, 3, 1);
            var btnBuscar = new Button { Text = "Buscar", Dock = DockStyle.Fill };
            btnBuscar.Click += (s, e) => BuscarProducto();
            datos.Controls.Add(btnBuscar, 4, 1);
            var btnLimpiar = new Button { Text = "Limpiar", Dock = DockStyle.Fill };
            btnLimpiar.Click += (s, e) => LimpiarCampos();
            datos.Controls.Add(btnLimpiar, 5, 1);

            var botones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };
            datos.Controls.Add(botones, 0, 2);
            datos.SetColumnSpan(botones, 6);
            AddButton(botones, "Registrar", (_, __) => RegistrarProducto());
            AddButton(botones, "Modificar seleccionado", (_, __) => ModificarProducto());
            AddButton(botones, "Eliminar seleccionado", (_, __) => EliminarProducto());
            AddButton(botones, "Ordenar por nombre", (_, __) => OrdenarProductos(false));
            AddButton(botones, "Ordenar por precio", (_, __) => OrdenarProductos(true));

            var grpCatalogo = new GroupBox { Text = "Catálogo de productos (List<Producto>)", Dock = DockStyle.Fill, Padding = new Padding(8) };
            principal.Controls.Add(grpCatalogo, 0, 1);
            dgvProductos.Dock = DockStyle.Fill;
            dgvProductos.ReadOnly = true;
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.AllowUserToDeleteRows = false;
            dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos.MultiSelect = false;
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProductos.SelectionChanged += (s, e) => CargarSeleccionEnCampos();
            grpCatalogo.Controls.Add(dgvProductos);

            var grpResumen = new GroupBox { Text = "Vector: existencias por categoría", Dock = DockStyle.Fill, Padding = new Padding(8) };
            principal.Controls.Add(grpResumen, 1, 1);
            lstResumen.Dock = DockStyle.Fill;
            grpResumen.Controls.Add(lstResumen);

            var grpMov = new GroupBox { Text = "Lista enlazada: movimientos de inventario", Dock = DockStyle.Fill, Padding = new Padding(8) };
            principal.Controls.Add(grpMov, 0, 2);
            var panelMov = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            panelMov.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            panelMov.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            grpMov.Controls.Add(panelMov);
            var filaBotones = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };
            panelMov.Controls.Add(filaBotones, 0, 0);
            filaBotones.Controls.Add(new Label { Text = "Cantidad:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
            txtCantidadMovimiento.Width = 65;
            filaBotones.Controls.Add(txtCantidadMovimiento);
            AddButton(filaBotones, "Entrada", (_, __) => RegistrarMovimiento(true));
            AddButton(filaBotones, "Salida", (_, __) => RegistrarMovimiento(false));
            AddButton(filaBotones, "Recorrer lista", (_, __) => ActualizarMovimientos());
            lstMovimientos.Dock = DockStyle.Fill;
            lstMovimientos.HorizontalScrollbar = true;
            panelMov.Controls.Add(lstMovimientos, 0, 1);

            var grpBuscarMov = new GroupBox { Text = "Consultar o eliminar movimiento por ID", Dock = DockStyle.Fill, Padding = new Padding(8) };
            principal.Controls.Add(grpBuscarMov, 1, 2);
            var panelBuscarMov = new TableLayoutPanel { Dock = DockStyle.Top, Height = 105, ColumnCount = 2, RowCount = 3 };
            panelBuscarMov.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            panelBuscarMov.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panelBuscarMov.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            panelBuscarMov.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            panelBuscarMov.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            grpBuscarMov.Controls.Add(panelBuscarMov);
            AddLabel(panelBuscarMov, "ID movimiento:", 0, 0);
            txtIdMovimiento.Dock = DockStyle.Fill;
            panelBuscarMov.Controls.Add(txtIdMovimiento, 1, 0);
            var btnBuscarMov = new Button { Text = "Buscar movimiento", Dock = DockStyle.Fill };
            btnBuscarMov.Click += (s, e) => BuscarMovimiento();
            panelBuscarMov.Controls.Add(btnBuscarMov, 0, 1);
            panelBuscarMov.SetColumnSpan(btnBuscarMov, 2);
            var btnEliminarMov = new Button { Text = "Eliminar movimiento", Dock = DockStyle.Fill };
            btnEliminarMov.Click += (s, e) => EliminarMovimiento();
            panelBuscarMov.Controls.Add(btnEliminarMov, 0, 2);
            panelBuscarMov.SetColumnSpan(btnEliminarMov, 2);

            lblEstado.Dock = DockStyle.Bottom;
            lblEstado.Height = 28;
            lblEstado.TextAlign = ContentAlignment.MiddleLeft;
            lblEstado.Padding = new Padding(10, 0, 0, 0);
            Controls.Add(lblEstado);
            lblEstado.BringToFront();
        }

        private static void AddLabel(TableLayoutPanel panel, string texto, int col, int row)
        {
            panel.Controls.Add(new Label { Text = texto, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, col, row);
        }

        private static void AddButton(Control parent, string texto, EventHandler accion)
        {
            var b = new Button { Text = texto, AutoSize = true, Height = 30, Margin = new Padding(3) };
            b.Click += accion;
            parent.Controls.Add(b);
        }

        private void CargarDatosEjemplo()
        {
            catalogo.Add(new Producto { Id = siguienteProductoId++, Nombre = "Arroz", Categoria = "Alimentos", Precio = 950m, Existencias = 25 });
            catalogo.Add(new Producto { Id = siguienteProductoId++, Nombre = "Agua", Categoria = "Bebidas", Precio = 800m, Existencias = 18 });
            catalogo.Add(new Producto { Id = siguienteProductoId++, Nombre = "Jabón", Categoria = "Limpieza", Precio = 1200m, Existencias = 12 });
            RegistrarMovimientoEnLista("Entrada inicial", catalogo[0], catalogo[0].Existencias, "Carga de datos de ejemplo");
            RegistrarMovimientoEnLista("Entrada inicial", catalogo[1], catalogo[1].Existencias, "Carga de datos de ejemplo");
            RegistrarMovimientoEnLista("Entrada inicial", catalogo[2], catalogo[2].Existencias, "Carga de datos de ejemplo");
        }

        private void RegistrarProducto()
        {
            if (!ValidarCampos(out string nombre, out string categoria, out decimal precio, out int stock)) return;
            if (catalogo.Any(p => p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Ya existe un producto con ese nombre.");
                return;
            }

            catalogo.Add(new Producto { Id = siguienteProductoId++, Nombre = nombre, Categoria = categoria, Precio = precio, Existencias = stock });
            var producto = catalogo[catalogo.Count - 1];
            RegistrarMovimientoEnLista("Registro", producto, stock, "Producto registrado");
            ActualizarTodo();
            LimpiarCampos();
            Mensaje("Producto registrado correctamente.");
        }

        private bool ValidarCampos(out string nombre, out string categoria, out decimal precio, out int stock)
        {
            nombre = txtNombre.Text.Trim();
            categoria = cmbCategoria.SelectedItem?.ToString() ?? "";
            precio = 0;
            stock = 0;

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("El nombre es obligatorio.");
                txtNombre.Focus();
                return false;
            }
            if (cmbCategoria.SelectedIndex < 0)
            {
                MessageBox.Show("Seleccione una categoría.");
                return false;
            }
            if (!decimal.TryParse(txtPrecio.Text.Trim(), out precio) || precio < 0)
            {
                MessageBox.Show("Ingrese un precio válido (cero o mayor).");
                txtPrecio.Focus();
                return false;
            }
            if (!int.TryParse(txtExistencias.Text.Trim(), out stock) || stock < 0)
            {
                MessageBox.Show("Las existencias deben ser un número entero igual o mayor que cero.");
                txtExistencias.Focus();
                return false;
            }
            return true;
        }

        private Producto? ProductoSeleccionado()
        {
            if (dgvProductos.CurrentRow?.DataBoundItem is Producto p) return p;
            if (dgvProductos.CurrentRow != null && dgvProductos.CurrentRow.Cells["Id"].Value != null)
            {
                int id = Convert.ToInt32(dgvProductos.CurrentRow.Cells["Id"].Value);
                return catalogo.FirstOrDefault(x => x.Id == id);
            }
            return null;
        }

        private void ModificarProducto()
        {
            Producto? p = ProductoSeleccionado();
            if (p == null) { MessageBox.Show("Seleccione un producto de la tabla."); return; }
            if (!ValidarCampos(out string nombre, out string categoria, out decimal precio, out int stock)) return;
            if (catalogo.Any(x => x.Id != p.Id && x.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Otro producto ya utiliza ese nombre.");
                return;
            }

            int diferencia = stock - p.Existencias;
            p.Nombre = nombre;
            p.Categoria = categoria;
            p.Precio = precio;
            p.Existencias = stock;
            if (diferencia != 0)
                RegistrarMovimientoEnLista(diferencia > 0 ? "Ajuste de entrada" : "Ajuste de salida", p, Math.Abs(diferencia), "Existencias modificadas");
            ActualizarTodo();
            Mensaje("Producto modificado correctamente.");
        }

        private void EliminarProducto()
        {
            Producto? p = ProductoSeleccionado();
            if (p == null) { MessageBox.Show("Seleccione un producto de la tabla."); return; }
            if (MessageBox.Show($"¿Desea eliminar el producto {p.Nombre}?", "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            catalogo.Remove(p);
            RegistrarMovimientoEnLista("Eliminación", p, p.Existencias, "Producto eliminado del catálogo");
            ActualizarTodo();
            LimpiarCampos();
            Mensaje("Producto eliminado del catálogo.");
        }

        private void BuscarProducto()
        {
            string termino = txtBuscar.Text.Trim();
            if (string.IsNullOrWhiteSpace(termino)) { MessageBox.Show("Escriba un nombre o parte del nombre para buscar."); return; }
            var encontrados = catalogo.Where(p => p.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase)).ToList();
            dgvProductos.DataSource = null;
            dgvProductos.DataSource = encontrados;
            if (encontrados.Count == 0) MessageBox.Show("No se encontraron productos.");
            else Mensaje($"Se encontraron {encontrados.Count} producto(s).");
        }

        private void OrdenarProductos(bool porPrecio)
        {
            if (catalogo.Count == 0) { MessageBox.Show("No hay productos para ordenar."); return; }
            List<Producto> ordenados = porPrecio
                ? catalogo.OrderBy(p => p.Precio).ToList()
                : catalogo.OrderBy(p => p.Nombre).ToList();
            MostrarProductos(ordenados);
            Mensaje(porPrecio ? "Catálogo ordenado por precio." : "Catálogo ordenado por nombre.");
        }

        private void RegistrarMovimiento(bool entrada)
        {
            Producto? p = ProductoSeleccionado();
            if (p == null) { MessageBox.Show("Seleccione un producto en la tabla."); return; }
            if (!int.TryParse(txtCantidadMovimiento.Text.Trim(), out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Digite una cantidad entera mayor que cero en el campo Cantidad de movimiento.");
                return;
            }
            if (!entrada && cantidad > p.Existencias)
            {
                MessageBox.Show("No se puede retirar más cantidad de la que existe.");
                return;
            }

            p.Existencias += entrada ? cantidad : -cantidad;
            RegistrarMovimientoEnLista(entrada ? "Entrada" : "Salida", p, cantidad, entrada ? "Ingreso de mercadería" : "Salida de mercadería");
            ActualizarTodo();
            Mensaje(entrada ? "Entrada registrada." : "Salida registrada.");
        }

        private void RegistrarMovimientoEnLista(string tipo, Producto p, int cantidad, string detalle)
        {
            movimientos.Insertar(new Movimiento
            {
                Id = siguienteMovimientoId++,
                Fecha = DateTime.Now,
                Tipo = tipo,
                ProductoId = p.Id,
                ProductoNombre = p.Nombre,
                Cantidad = cantidad,
                Detalle = detalle
            });
        }

        private void BuscarMovimiento()
        {
            if (!int.TryParse(txtIdMovimiento.Text.Trim(), out int id) || id <= 0)
            {
                MessageBox.Show("Ingrese un ID de movimiento válido.");
                return;
            }
            Movimiento? m = movimientos.Buscar(id);
            MessageBox.Show(m == null ? "No se encontró el movimiento." : m.ToString(), "Resultado de búsqueda");
        }

        private void EliminarMovimiento()
        {
            if (!int.TryParse(txtIdMovimiento.Text.Trim(), out int id) || id <= 0)
            {
                MessageBox.Show("Ingrese un ID de movimiento válido.");
                return;
            }
            if (movimientos.Eliminar(id))
            {
                ActualizarMovimientos();
                Mensaje("Movimiento eliminado de la lista enlazada.");
            }
            else MessageBox.Show("No existe un movimiento con ese ID.");
        }

        private void ActualizarTodo()
        {
            RecalcularVector();
            MostrarProductos(catalogo);
            ActualizarResumenVector();
            ActualizarMovimientos();
        }

        private void MostrarProductos(List<Producto> datos)
        {
            dgvProductos.DataSource = null;
            dgvProductos.DataSource = datos.ToList();
            if (dgvProductos.Columns["Id"] != null) dgvProductos.Columns["Id"]!.HeaderText = "ID";
            if (dgvProductos.Columns["Nombre"] != null) dgvProductos.Columns["Nombre"]!.HeaderText = "Nombre";
            if (dgvProductos.Columns["Categoria"] != null) dgvProductos.Columns["Categoria"]!.HeaderText = "Categoría";
            if (dgvProductos.Columns["Precio"] != null) dgvProductos.Columns["Precio"]!.HeaderText = "Precio (₡)";
            if (dgvProductos.Columns["Existencias"] != null) dgvProductos.Columns["Existencias"]!.HeaderText = "Existencias";
        }

        private void RecalcularVector()
        {
            // Recorrido del vector mediante índice y asignación por posición de categoría.
            for (int i = 0; i < existenciasPorCategoria.Length; i++)
                existenciasPorCategoria[i] = 0;

            foreach (Producto p in catalogo)
            {
                int indice = Array.IndexOf(categorias, p.Categoria);
                if (indice >= 0)
                    existenciasPorCategoria[indice] += p.Existencias;
            }
        }

        private void ActualizarResumenVector()
        {
            lstResumen.Items.Clear();
            int total = 0;
            for (int i = 0; i < categorias.Length; i++)
            {
                lstResumen.Items.Add($"{categorias[i]}: {existenciasPorCategoria[i]} unidades");
                total += existenciasPorCategoria[i];
            }
            lstResumen.Items.Add("------------------------------");
            lstResumen.Items.Add($"Total de unidades: {total}");
            lstResumen.Items.Add($"Productos en catálogo: {catalogo.Count}");
        }

        private void ActualizarMovimientos()
        {
            lstMovimientos.Items.Clear();
            if (movimientos.EstaVacia)
            {
                lstMovimientos.Items.Add("No hay movimientos registrados.");
                return;
            }
            // Recorrido real de la lista enlazada propia.
            foreach (Movimiento m in movimientos.Recorrer())
                lstMovimientos.Items.Add(m.ToString());
        }

        private void CargarSeleccionEnCampos()
        {
            Producto? p = ProductoSeleccionado();
            if (p == null) return;
            txtNombre.Text = p.Nombre;
            cmbCategoria.SelectedItem = p.Categoria;
            txtPrecio.Text = p.Precio.ToString("0.##");
            txtExistencias.Text = p.Existencias.ToString();
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtPrecio.Clear();
            txtExistencias.Clear();
            txtBuscar.Clear();
            cmbCategoria.SelectedIndex = 0;
            dgvProductos.ClearSelection();
            txtNombre.Focus();
        }

        private void Mensaje(string texto) => lblEstado.Text = texto;
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
