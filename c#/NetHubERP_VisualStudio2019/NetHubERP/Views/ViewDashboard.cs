using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Views
{
    public class ViewDashboard : Page
    {
        // Superfície de design segura: nenhuma consulta SQL no Designer.
        public ViewDashboard() : base(new UserSession { Nome = "Prévia", Perfil = "admin" }, key => { })
        {
            Fixed(Header("Dashboard"), 64);
        }
        public ViewDashboard(UserSession user, Action<string> go) : base(user, go)
        {
            var products = Repository.Products();
            var low = products.Where(p => p.Baixo).ToList();
            var history = Repository.History(100).Where(h => h.Tipo != "Acesso").Take(8).ToList();
            Fixed(Header("Dashboard"), 64);
            bool admin = user.Admin;
            var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3,
                RowCount = admin ? 2 : 1, Padding = new Padding(0) };
            for (int i=0; i<3; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
            for (int i=0; i<cards.RowCount; i++) cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / cards.RowCount));
            var values = new System.Collections.Generic.List<Tuple<string,string>> {
                Tuple.Create("Produtos", products.Count.ToString()),
                Tuple.Create("Unidades em estoque", products.Sum(p => (long)p.Quantidade).ToString("N0")),
                Tuple.Create("Estoque baixo", low.Count.ToString()) };
            if (admin)
            {
                values.Add(Tuple.Create("Clientes", Repository.Contacts(false).Count.ToString()));
                values.Add(Tuple.Create("Fornecedores", Repository.Contacts(true).Count.ToString()));
                values.Add(Tuple.Create("Valor do estoque", products.Sum(p => p.Quantidade * p.Preco).ToString("C2")));
            }
            for (int i=0; i<values.Count; i++)
            {
                var card = Theme.Card();
                var title = Theme.Hint(values[i].Item1); title.Location = new Point(14, 11);
                var num = Theme.Label(values[i].Item2, 19, true); num.Location = new Point(14, 43);
                card.Controls.Add(title); card.Controls.Add(num);
                cards.Controls.Add(card, i % 3, i / 3);
            }
            Fixed(cards, admin ? 216 : 115);
            var halves = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            halves.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            halves.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            halves.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            halves.Controls.Add(ListCard("Precisam de reposição", low.Take(8).Select(p =>
                p.Nome + "    ·    " + p.Quantidade + " un.").ToArray(), "Nenhum produto com estoque baixo."), 0, 0);
            halves.Controls.Add(ListCard("Últimas movimentações", history.Select(h =>
                h.Descricao + "    ·    " + h.Data.ToString("dd/MM HH:mm")).ToArray(), "Nenhuma movimentação registrada."), 1, 0);
            Fill(halves);
        }
        private static Panel ListCard(string title, string[] lines, string empty)
        {
            var card = Theme.Card();
            var label = Theme.Label(title, 12, true); label.Dock = DockStyle.Fill;
            var section = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            section.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            section.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            section.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            section.Controls.Add(label, 0, 0);
            var list = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, Padding = new Padding(0, 3, 0, 0) };
            list.Resize += (s,e) => {
                foreach (Control item in list.Controls) item.Width = Math.Max(80, list.ClientSize.Width - 18);
            };
            foreach (var text in lines.Length == 0 ? new[] {empty} : lines)
            {
                var line = Theme.Hint(text); line.AutoSize = false; line.Height = 37;
                line.BorderStyle = BorderStyle.FixedSingle;
                list.Controls.Add(line);
            }
            section.Controls.Add(list, 0, 1);
            card.Controls.Add(section);
            return card;
        }
    }
}
