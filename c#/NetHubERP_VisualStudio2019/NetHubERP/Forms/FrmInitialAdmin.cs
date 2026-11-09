using System;
using System.Drawing;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.UI;

namespace NetHubERP.Forms
{
    public partial class FrmInitialAdmin : Form
    {
        public FrmInitialAdmin()
        {
            InitializeComponent(); Theme.Form(this, 440, 370);
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
            var heading = Theme.Label("Primeiro administrador", 17, true);
            heading.Location = new Point(25, 14); Controls.Add(heading);
            var sub = Theme.Hint("A senha inicial será definida por você (mín. 8 caracteres).");
            sub.Location = new Point(25, 57); Controls.Add(sub);
            AddText("Nome", txtName, 95, false);
            AddText("E-mail", txtEmail, 156, false);
            AddText("Senha", txtPass, 217, true);
            btnSave = Theme.Button("Criar administrador", (s,e) => Save());
            btnSave.SetBounds(25, 296, 233, 40); Controls.Add(btnSave);
            btnCancel = Theme.Button("Cancelar", (s,e) => { DialogResult = DialogResult.Cancel; Close(); }, "secondary");
            btnCancel.SetBounds(268, 296, 145, 40); Controls.Add(btnCancel);
        }
        private void AddText(string title, TextBox text, int top, bool password)
        {
            var l = Theme.Hint(title); l.Location = new Point(26, top); Controls.Add(l);
            text.SetBounds(25, top + 23, 388, 30); text.BackColor = Theme.Field;
            text.ForeColor = Theme.Text; text.BorderStyle = BorderStyle.FixedSingle;
            text.MaxLength = password ? 128 : 180;
            text.UseSystemPasswordChar = password; Controls.Add(text);
        }
        private void Save()
        {
            try
            {
                Repository.CreateFirstAdmin(txtName.Text, txtEmail.Text, txtPass.Text);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { Theme.Error(this, ex); }
        }
    }
}
