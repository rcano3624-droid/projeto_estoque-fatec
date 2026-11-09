using System;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Forms;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Views
{
    public class ViewContacts : Page
    {
        // Superfície de design segura: nenhuma consulta SQL no Designer.
        public ViewContacts() : base(new UserSession { Nome = "Prévia", Perfil = "admin" }, key => { })
        {
            Fixed(Header("Clientes"), 64);
        }
        private readonly bool _suppliers;
        private readonly DataGridView grid;
        public ViewContacts(UserSession user, Action<string> go, bool suppliers) : base(user, go)
        {
            if (!user.Admin) throw new UnauthorizedAccessException("Módulo disponível somente para administradores.");
            _suppliers = suppliers;
            Fixed(Header(suppliers ? "Fornecedores" : "Clientes", (s,e) => Edit(null),
                suppliers ? "Novo fornecedor" : "Novo cliente"), 64);
            grid = Theme.Grid();
            grid.Columns.Add(Theme.Col("Nome", suppliers ? "Empresa" : "Nome", 150));
            grid.Columns.Add(Theme.Col("Extra", suppliers ? "Contato" : "E-mail", 130));
            grid.Columns.Add(Theme.Col("Telefone", "Telefone", 100));
            grid.Columns.Add(Theme.Col("Cidade", "Cidade", 110));
            grid.Columns.Add(Theme.ActionCol("Editar", "Editar"));
            grid.Columns.Add(Theme.ActionCol("Excluir", "Excluir"));
            grid.CellContentClick += ClickRow;
            var wrap = Theme.Card(); wrap.Controls.Add(grid); Fill(wrap);
            LoadData();
        }
        private void LoadData()
        {
            grid.Rows.Clear();
            foreach (var c in Repository.Contacts(_suppliers))
            {
                int r = grid.Rows.Add(c.Nome, _suppliers ? c.Contato : c.Email, c.Telefone, c.Cidade);
                grid.Rows[r].Tag = c;
            }
        }
        private void ClickRow(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var c = grid.Rows[e.RowIndex].Tag as Contact;
            if (c == null) return;
            string action = grid.Columns[e.ColumnIndex].Name;
            if (action == "Editar") Edit(c);
            if (action == "Excluir") Delete(c);
        }
        private void Edit(Contact c)
        {
            using (var f = new FrmContact(c, _suppliers, Current))
                if (f.ShowDialog(FindForm()) == DialogResult.OK) Navigate(_suppliers ? "fornecedores" : "clientes");
        }
        private void Delete(Contact c)
        {
            if (MessageBox.Show(FindForm(), "Excluir \"" + c.Nome + "\"?", "Confirmação",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { Repository.DeleteContact(c.Id, _suppliers, Current); Navigate(_suppliers ? "fornecedores" : "clientes"); }
            catch (Exception ex) { Theme.Error(FindForm(), ex); }
        }
    }
}
