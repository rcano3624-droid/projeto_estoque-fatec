using System;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Forms;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Views
{
    public class ViewUsers : Page
    {
        // Superfície de design segura: nenhuma consulta SQL no Designer.
        public ViewUsers() : base(new UserSession { Nome = "Prévia", Perfil = "admin" }, key => { })
        {
            Fixed(Header("Usuários"), 64);
        }
        private readonly DataGridView grid;
        public ViewUsers(UserSession user, Action<string> go) : base(user, go)
        {
            if (!user.Admin) throw new UnauthorizedAccessException("Módulo restrito a administradores.");
            Fixed(Header("Usuários", (s,e) => Add(), "Novo usuário"), 64);
            grid = Theme.Grid();
            grid.Columns.Add(Theme.Col("Email", "E-mail", 170));
            grid.Columns.Add(Theme.Col("Nome", "Nome", 140));
            grid.Columns.Add(Theme.Col("Perfil", "Perfil", 90));
            grid.Columns.Add(Theme.ActionCol("Excluir", "Excluir"));
            grid.CellContentClick += ClickRow;
            var panel = Theme.Card(); panel.Controls.Add(grid); Fill(panel);
            LoadData();
        }
        private void LoadData()
        {
            grid.Rows.Clear();
            foreach (var u in Repository.Users())
            {
                int i = grid.Rows.Add(u.Email, u.Nome, u.Perfil == "admin" ? "Administrador" : "Funcionário");
                grid.Rows[i].Tag = u;
                if (u.Id == Current.Id)
                    grid.Rows[i].Cells["Excluir"] = new DataGridViewTextBoxCell { Value = "Conta atual" };
            }
        }
        private void ClickRow(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Excluir") return;
            var u = grid.Rows[e.RowIndex].Tag as Account;
            if (u == null || u.Id == Current.Id) return;
            if (MessageBox.Show(FindForm(), "Excluir o usuário \"" + u.Email + "\"?", "Confirmação",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { Repository.DeleteUser(u.Id, Current); Navigate("usuarios"); }
            catch (Exception ex) { Theme.Error(FindForm(), ex); }
        }
        private void Add()
        {
            using (var f = new FrmUser(Current))
                if (f.ShowDialog(FindForm()) == DialogResult.OK) Navigate("usuarios");
        }
    }
}
