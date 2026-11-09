using System;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Views
{
    public class ViewHistory : Page
    {
        // Superfície de design segura: nenhuma consulta SQL no Designer.
        public ViewHistory() : base(new UserSession { Nome = "Prévia", Perfil = "admin" }, key => { })
        {
            Fixed(Header("Histórico"), 64);
        }
        public ViewHistory(UserSession user, Action<string> go) : base(user, go)
        {
            Fixed(Header("Histórico"), 64);
            var tabs = new TabControl { Dock = DockStyle.Fill, Font = Theme.Normal };
            var auditTab = new TabPage("Auditoria");
            var movTab = new TabPage("Movimentações");
            auditTab.BackColor = Theme.Panel; movTab.BackColor = Theme.Panel;
            var audit = Theme.Grid();
            audit.Columns.Add(Theme.Col("Data", "Data", 85));
            audit.Columns.Add(Theme.Col("Usuario", "Usuário", 110));
            audit.Columns.Add(Theme.Col("Tipo", "Tipo", 90));
            audit.Columns.Add(Theme.Col("Descricao", "Detalhe", 260));
            foreach (var h in Repository.History(500))
                audit.Rows.Add(h.Data.ToString("dd/MM/yyyy HH:mm:ss"), h.Usuario, h.Tipo, h.Descricao);
            auditTab.Controls.Add(audit);
            var movements = Theme.Grid();
            movements.Columns.Add(Theme.Col("Data", "Data", 95));
            movements.Columns.Add(Theme.Col("Produto", "Produto", 145));
            movements.Columns.Add(Theme.Col("Tipo", "Tipo", 75));
            movements.Columns.Add(Theme.Col("Quantidade", "Quantidade", 85));
            movements.Columns.Add(Theme.Col("Saldo", "Saldo final", 85));
            movements.Columns.Add(Theme.Col("Motivo", "Motivo", 155));
            movements.Columns.Add(Theme.Col("Usuario", "Usuário", 105));
            foreach (var m in Repository.Movements(500))
                movements.Rows.Add(m.Data.ToString("dd/MM/yyyy HH:mm:ss"), m.Produto, m.Tipo,
                    m.Quantidade, m.SaldoApos, m.Motivo, m.Usuario);
            movTab.Controls.Add(movements);
            tabs.TabPages.Add(auditTab); tabs.TabPages.Add(movTab);
            Fill(tabs);
        }
    }
}
