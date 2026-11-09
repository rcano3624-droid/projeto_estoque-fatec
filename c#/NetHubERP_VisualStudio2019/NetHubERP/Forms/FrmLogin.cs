using System;
using System.Drawing;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.UI;

namespace NetHubERP.Forms
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
            Theme.Form(this, 920, 610);
            loginPanel.BackColor = Theme.Panel;
            lblBrand.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblBrand.ForeColor = Theme.AccentLight;
            lblSubtitle.ForeColor = Theme.Muted;
            lblStatus.ForeColor = Theme.Warning;
            lblEmail.ForeColor = Theme.Muted;
            lblPassword.ForeColor = Theme.Muted;
            txtEmail.BackColor = Theme.Field;
            txtEmail.ForeColor = Theme.Text;
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.MaxLength = 180;
            txtPassword.BackColor = Theme.Field;
            txtPassword.ForeColor = Theme.Text;
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.MaxLength = 128;
            StyleButton(btnLogin, Theme.Accent);
            StyleButton(btnFirstAdmin, Theme.Field);
            btnLogin.Click += (s,e) => Login();
            btnFirstAdmin.Click += (s,e) => Setup();
            txtPassword.KeyDown += (s,e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Login(); } };
            AcceptButton = btnLogin;
            Resize += (s,e) => CenterPanel();
            Shown += (s,e) => RefreshStatus();
            CenterPanel();
        }
        private void StyleButton(Button b, Color color)
        {
            b.BackColor = color; b.ForeColor = Theme.Text;
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.Cursor = Cursors.Hand;
        }
        private void CenterPanel()
        {
            loginPanel.Left = Math.Max(8, (ClientSize.Width - loginPanel.Width) / 2);
            loginPanel.Top = Math.Max(8, (ClientSize.Height - loginPanel.Height) / 2);
        }
        private void RefreshStatus()
        {
            try
            {
                Db.Check();
                var users = Repository.HasUsers();
                lblStatus.Text = users ? "Conectado ao SQL Server" : "Banco pronto · crie o administrador";
                lblStatus.ForeColor = Theme.Success;
                lblStatus.Tag = null;
                lblStatus.Click -= StatusClick;
                lblStatus.Cursor = Cursors.Default;
                btnFirstAdmin.Visible = !users;
                btnLogin.Enabled = users;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "SQL Server indisponível — confira App.config";
                lblStatus.ForeColor = Theme.Danger;
                btnFirstAdmin.Visible = false;
                btnLogin.Enabled = false;
                // A mensagem completa pode ser exibida após clicar no status.
                lblStatus.Cursor = Cursors.Hand;
                lblStatus.Click -= StatusClick;
                lblStatus.Click += StatusClick;
                lblStatus.Tag = ex;
            }
        }
        private void StatusClick(object sender, EventArgs e)
        {
            if (lblStatus.Tag is Exception) Theme.Error(this, (Exception)lblStatus.Tag);
        }
        private void Setup()
        {
            using (var form = new FrmInitialAdmin())
                if (form.ShowDialog(this) == DialogResult.OK) RefreshStatus();
        }
        private void Login()
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrEmpty(txtPassword.Text))
            { Theme.Info(this, "Informe e-mail e senha."); return; }
            try
            {
                var user = Repository.Authenticate(txtEmail.Text, txtPassword.Text);
                if (user == null) { MessageBox.Show(this, "E-mail ou senha incorretos.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                Repository.RegisterLogin(user);
                txtPassword.Clear();
                Hide();
                try { using (var main = new FrmMain(user)) main.ShowDialog(this); }
                finally { Show(); RefreshStatus(); }
            }
            catch (Exception ex) { Theme.Error(this, ex); }
        }
    }
}
