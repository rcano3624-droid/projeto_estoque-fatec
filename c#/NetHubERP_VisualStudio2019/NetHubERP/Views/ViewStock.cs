using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Forms;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Views
{
    public class ViewStock : Page
    {
        // Superfície de design segura: nenhuma consulta SQL no Designer.
        public ViewStock() : base(new UserSession { Nome = "Prévia", Perfil = "admin" }, key => { })
        {
            Fixed(Header("Estoque"), 64);
        }
        private readonly DataGridView grid;
        private readonly TextBox search;
        private readonly ComboBox categories;
        private readonly ComboBox status;
        private List<Product> _all;
        private string _orderBy = "Nome";
        private bool _ascending = true;
        public ViewStock(UserSession user, Action<string> go) : base(user, go)
        {
            Fixed(Header("Estoque", user.Admin ? (EventHandler)((s,e) => Edit(null)) : null,
                "Novo produto"), 64);
            var filters = Theme.Card();
            filters.Padding = new Padding(13);
            var filterFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true };
            search = Theme.Input(180); search.Width = 350;
            categories = Theme.Combo(); categories.Width = 230;
            status = Theme.Combo(); status.Width = 186;
            status.Items.AddRange(new object[] { "Todos os status", "Só estoque baixo" });
            status.SelectedIndex = 0;
            filterFlow.Controls.Add(search);
            filterFlow.Controls.Add(categories);
            filterFlow.Controls.Add(status);
            filters.Controls.Add(filterFlow);
            Fixed(filters, 116);
            grid = Theme.Grid();
            grid.RowTemplate.Height = 62;
            var picture = new DataGridViewImageColumn { Name = "Foto", HeaderText = "Foto", FillWeight = 43,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                SortMode = DataGridViewColumnSortMode.NotSortable };
            picture.DefaultCellStyle.NullValue = null;
            grid.Columns.Add(picture);
            grid.Columns.Add(Theme.Col("Codigo", "Código", 60));
            grid.Columns.Add(Theme.Col("Nome", "Produto", 120));
            grid.Columns.Add(Theme.Col("Categoria", "Categoria", 90));
            grid.Columns.Add(Theme.Col("Quantidade", "Quantidade", 67));
            grid.Columns.Add(Theme.Col("Preco", "Preço", 77));
            grid.Columns.Add(Theme.Col("Local", "Local", 90));
            grid.Columns.Add(Theme.Col("Status", "Status", 85));
            grid.Columns.Add(Theme.ActionCol("Entrada", "Entrada"));
            grid.Columns.Add(Theme.ActionCol("Saida", "Saída"));
            grid.Columns.Add(Theme.ActionCol("Editar", "Editar"));
            grid.Columns.Add(Theme.ActionCol("Excluir", "Excluir"));
            grid.Columns["Preco"].Visible = user.Admin;
            grid.Columns["Editar"].Visible = user.Admin;
            grid.Columns["Excluir"].Visible = user.Admin;
            grid.CellContentClick += ClickRow;
            grid.ColumnHeaderMouseClick += SortColumn;
            var wrap = Theme.Card(); wrap.Controls.Add(grid);
            Fill(wrap);
            search.TextChanged += (s,e) => Render();
            categories.SelectedIndexChanged += (s,e) => Render();
            status.SelectedIndexChanged += (s,e) => Render();
            LoadData();
        }
        private static Image Decode(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            using (var m = new MemoryStream(bytes))
            using (var original = Image.FromStream(m)) return new Bitmap(original);
        }
        private void LoadData()
        {
            _all = Repository.Products();
            categories.Items.Clear(); categories.Items.Add("Todas as categorias");
            foreach (var category in _all.Select(p => p.Categoria).Distinct().OrderBy(x => x))
                categories.Items.Add(category);
            categories.SelectedIndex = 0;
            Render();
        }
        private void Render()
        {
            if (_all == null) return;
            string query = search.Text.Trim();
            string cat = categories.SelectedIndex <= 0 ? "" : Convert.ToString(categories.SelectedItem);
            bool lowOnly = status.SelectedIndex == 1;
            IEnumerable<Product> list = _all.Where(p =>
                (p.Nome + " " + p.Marca + " " + p.Codigo + " " + p.Categoria + " " + p.Local)
                    .IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0 &&
                (cat == "" || p.Categoria == cat) && (!lowOnly || p.Baixo));
            Func<Product, IComparable> key;
            switch (_orderBy)
            {
                case "Codigo": key = p => p.Codigo; break;
                case "Categoria": key = p => p.Categoria; break;
                case "Quantidade": key = p => p.Quantidade; break;
                case "Preco": key = p => p.Preco; break;
                case "Local": key = p => p.Local; break;
                default: key = p => p.Nome; break;
            }
            list = _ascending ? list.OrderBy(key) : list.OrderByDescending(key);
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["Foto"].Value is Image) ((Image)row.Cells["Foto"].Value).Dispose();
            }
            grid.Rows.Clear();
            foreach (var p in list)
            {
                int idx = grid.Rows.Add(Decode(p.Foto), p.Codigo, p.Nome + "\n" + p.Marca,
                    p.Categoria, p.Quantidade + " (mín. " + p.Minimo + ")", p.Preco.ToString("C2"), p.Local,
                    p.Quantidade == 0 ? "Sem estoque" : p.Baixo ? "Estoque baixo" : "Normal");
                var row = grid.Rows[idx]; row.Tag = p;
                if (p.Baixo) row.DefaultCellStyle.BackColor = Color.FromArgb(69, 44, 49);
                row.Cells["Status"].Style.ForeColor = p.Quantidade == 0 ? Theme.Warning : p.Baixo ? Theme.Danger : Theme.Success;
            }
        }
        private void SortColumn(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0) return;
            var col = grid.Columns[e.ColumnIndex].Name;
            if (col != "Codigo" && col != "Nome" && col != "Categoria" && col != "Quantidade" && col != "Preco" && col != "Local") return;
            _ascending = _orderBy == col ? !_ascending : true;
            _orderBy = col;
            Render();
        }
        private void ClickRow(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var p = grid.Rows[e.RowIndex].Tag as Product;
            if (p == null) return;
            var col = grid.Columns[e.ColumnIndex].Name;
            if (col != "Entrada" && col != "Saida" && col != "Editar" && col != "Excluir") return;
            if (!Current.Admin && (col == "Editar" || col == "Excluir")) return;
            if (col == "Editar") Edit(p);
            else if (col == "Excluir") Remove(p);
            else
            {
                using (var form = new FrmMove(p, col == "Entrada", Current))
                    if (form.ShowDialog(FindForm()) == DialogResult.OK) RefreshPage();
            }
        }
        private void Edit(Product p)
        {
            using (var form = new FrmProduct(p, Current))
                if (form.ShowDialog(FindForm()) == DialogResult.OK) RefreshPage();
        }
        private void Remove(Product p)
        {
            if (MessageBox.Show(FindForm(), "Excluir \"" + p.Nome + "\" do estoque?",
                "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { Repository.DeleteProduct(p.Id, Current); RefreshPage(); }
            catch (Exception ex) { Theme.Error(FindForm(), ex); }
        }
        private void RefreshPage() { Navigate("estoque"); }
        protected override void Dispose(bool disposing)
        {
            if (disposing && grid != null)
                foreach (DataGridViewRow row in grid.Rows)
                    if (row.Cells["Foto"].Value is Image) ((Image)row.Cells["Foto"].Value).Dispose();
            base.Dispose(disposing);
        }
    }
}
