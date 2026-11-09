using System;
using System.Drawing;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Forms
{
    public partial class FrmContact : Form
    {
        private readonly Contact _contact;
        private readonly UserSession _actor;
        private readonly bool _supplier;
        public FrmContact() : this(null, false, null) { } // Prévia no Designer.
        public FrmContact(Contact item, bool supplier, UserSession actor)
        {
            InitializeComponent(); Theme.Form(this, 570, 360);
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
            _contact = item ?? new Contact(); _supplier = supplier; _actor = actor;
            Text = (_contact.Id > 0 ? "Editar " : "Novo ") + (supplier ? "fornecedor" : "cliente");
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3,
                Padding = new Padding(16), BackColor = Theme.Panel };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(grid);
            Setup(txtName, 150); Setup(txtExtra, supplier ? 120 : 180);
            Setup(txtPhone, 30); Setup(txtCity, 100);
            Theme.AddField(grid, 0, supplier ? "Empresa" : "Nome", txtName);
            Theme.AddField(grid, 0, supplier ? "Contato" : "E-mail", txtExtra, 1);
            Theme.AddField(grid, 1, "Telefone", txtPhone);
            Theme.AddField(grid, 1, "Cidade", txtCity, 1);
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var save = Theme.Button("Salvar", (s,e) => Save()); save.Width = 140;
            var cancel = Theme.Button("Cancelar", (s,e) => { DialogResult = DialogResult.Cancel; Close(); }, "secondary"); cancel.Width = 130;
            footer.Controls.Add(save); footer.Controls.Add(cancel);
            grid.Controls.Add(footer, 0, 2); grid.SetColumnSpan(footer, 2);
            txtName.Text = _contact.Nome ?? "";
            txtExtra.Text = supplier ? _contact.Contato ?? "" : _contact.Email ?? "";
            txtPhone.Text = _contact.Telefone ?? "";
            txtCity.Text = _contact.Cidade ?? "";
        }
        private static void Setup(TextBox t, int max)
        {
            t.BackColor = Theme.Field; t.ForeColor = Theme.Text;
            t.BorderStyle = BorderStyle.FixedSingle; t.MaxLength = max;
        }
        private void Save()
        {
            try
            {
                _contact.Nome = txtName.Text.Trim();
                _contact.Contato = _supplier ? txtExtra.Text.Trim() : "";
                _contact.Email = _supplier ? "" : txtExtra.Text.Trim();
                _contact.Telefone = txtPhone.Text.Trim(); _contact.Cidade = txtCity.Text.Trim();
                Repository.SaveContact(_contact, _supplier, _actor);
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { Theme.Error(this, ex); }
        }
    }
}
