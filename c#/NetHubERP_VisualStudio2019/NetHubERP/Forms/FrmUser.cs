using System;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Forms
{
    public partial class FrmUser : Form
    {
        private readonly UserSession _current;
        public FrmUser() : this(null) { } // Prévia no Designer.
        public FrmUser(UserSession current)
        {
            InitializeComponent(); Theme.Form(this, 585, 370);
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
            _current = current;
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Panel,
                ColumnCount = 2, RowCount = 3, Padding = new Padding(16) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(grid);
            foreach (var box in new[] {nameBox,emailBox,passwordBox})
            { box.BackColor = Theme.Field; box.ForeColor = Theme.Text; box.BorderStyle = BorderStyle.FixedSingle; }
            nameBox.MaxLength = 120; emailBox.MaxLength = 180;
            passwordBox.MaxLength = 128; passwordBox.UseSystemPasswordChar = true;
            roleBox.BackColor = Theme.Field; roleBox.ForeColor = Theme.Text;
            roleBox.DropDownStyle = ComboBoxStyle.DropDownList;
            roleBox.Items.AddRange(new object[] { "Funcionário", "Administrador" });
            roleBox.SelectedIndex = 0;
            Theme.AddField(grid, 0, "Nome", nameBox);
            Theme.AddField(grid, 0, "E-mail", emailBox, 1);
            Theme.AddField(grid, 1, "Senha (mínimo 8 caracteres)", passwordBox);
            Theme.AddField(grid, 1, "Perfil", roleBox, 1);
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var save = Theme.Button("Criar usuário", (s,e) => Save()); save.Width = 150;
            var cancel = Theme.Button("Cancelar", (s,e) => { DialogResult = DialogResult.Cancel; Close(); }, "secondary");
            cancel.Width = 130; footer.Controls.Add(save); footer.Controls.Add(cancel);
            grid.Controls.Add(footer, 0, 2); grid.SetColumnSpan(footer, 2);
        }
        private void Save()
        {
            try
            {
                Repository.CreateUser(emailBox.Text, nameBox.Text, passwordBox.Text,
                    roleBox.SelectedIndex == 1 ? "admin" : "funcionario", _current);
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { Theme.Error(this, ex); }
        }
    }
}
