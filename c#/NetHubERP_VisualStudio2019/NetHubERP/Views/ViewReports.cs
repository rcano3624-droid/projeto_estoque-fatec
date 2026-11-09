using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Views
{
    public class ViewReports : Page
    {
        // Superfície de design segura: nenhuma consulta SQL no Designer.
        public ViewReports() : base(new UserSession { Nome = "Prévia", Perfil = "admin" }, key => { })
        {
            Fixed(Header("Relatórios"), 64);
        }
        public ViewReports(UserSession user, Action<string> go) : base(user, go)
        {
            if (!user.Admin) throw new UnauthorizedAccessException("Relatórios disponíveis apenas para administradores.");
            var products = Repository.Products();
            Fixed(Header("Relatórios", (s,e) => Export(), "Exportar CSV"), 64);
            var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            var summary = Theme.Card();
            var sumTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6 };
            sumTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 33));
            for (int i=0; i<5; i++) sumTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            sumTable.Controls.Add(Theme.Label("Resumo geral", 12, true), 0, 0);
            sumTable.Controls.Add(Theme.Hint("Produtos:   " + products.Count), 0, 1);
            sumTable.Controls.Add(Theme.Hint("Clientes:   " + Repository.Contacts(false).Count), 0, 2);
            sumTable.Controls.Add(Theme.Hint("Fornecedores:   " + Repository.Contacts(true).Count), 0, 3);
            sumTable.Controls.Add(Theme.Hint("Unidades em estoque:   " + products.Sum(p => (long)p.Quantidade).ToString("N0")), 0, 4);
            sumTable.Controls.Add(Theme.Hint("Valor do estoque:   " + products.Sum(p => p.Preco * p.Quantidade).ToString("C2")), 0, 5);
            summary.Controls.Add(sumTable);
            var low = Theme.Card();
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true };
            var head = Theme.Label("Estoque baixo", 12, true); bottom.Controls.Add(head);
            var lowProducts = products.Where(p => p.Baixo).ToList();
            foreach (var p in lowProducts)
                bottom.Controls.Add(Theme.Hint(p.Nome + " — " + p.Quantidade + " un."));
            if (lowProducts.Count == 0) bottom.Controls.Add(Theme.Hint("Nenhum produto com estoque baixo."));
            low.Controls.Add(bottom);
            top.Controls.Add(summary, 0, 0); top.Controls.Add(low, 1, 0);
            Fixed(top, 270);
            var group = Theme.Card();
            var groupGrid = Theme.Grid();
            groupGrid.Columns.Add(Theme.Col("Categoria", "Categoria", 140));
            groupGrid.Columns.Add(Theme.Col("Produtos", "Produtos", 80));
            groupGrid.Columns.Add(Theme.Col("Unidades", "Unidades", 80));
            groupGrid.Columns.Add(Theme.Col("Valor", "Valor em estoque", 130));
            foreach (var g in products.GroupBy(p => p.Categoria).OrderBy(g => g.Key))
                groupGrid.Rows.Add(g.Key, g.Count(), g.Sum(p => (long)p.Quantidade),
                    g.Sum(p => p.Quantidade * p.Preco).ToString("C2"));
            var title = Theme.Label("Por categoria", 12, true); title.Dock = DockStyle.Fill;
            var section = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            section.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            section.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            section.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            section.Controls.Add(title, 0, 0);
            section.Controls.Add(groupGrid, 0, 1);
            group.Controls.Add(section);
            Fill(group);
        }
        private static string CsvCell(string value)
        {
            value = value ?? "";
            // Impede que valores textuais de usuários sejam interpretados como fórmulas no Excel.
            string trimmed = value.TrimStart();
            if (trimmed.StartsWith("=") || trimmed.StartsWith("+") || trimmed.StartsWith("-") ||
                trimmed.StartsWith("@") || trimmed.StartsWith("\t")) value = "'" + value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        private void Export()
        {
            try
            {
                using (var dialog = new SaveFileDialog { Title = "Exportar estoque", Filter = "Arquivo CSV (*.csv)|*.csv",
                    FileName = "estoque.csv" })
                {
                    if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
                    var lines = new System.Collections.Generic.List<string> {
                        "codigo;nome;marca;categoria;local;quantidade;minimo;preco" };
                    foreach (var p in Repository.Products())
                        lines.Add(string.Join(";", new[] { CsvCell(p.Codigo), CsvCell(p.Nome), CsvCell(p.Marca),
                            CsvCell(p.Categoria), CsvCell(p.Local), p.Quantidade.ToString(), p.Minimo.ToString(),
                            p.Preco.ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")) }));
                    File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(true));
                    Theme.Info(FindForm(), "Arquivo CSV exportado com sucesso.");
                }
            }
            catch (Exception ex) { Theme.Error(FindForm(), ex); }
        }
    }
}
