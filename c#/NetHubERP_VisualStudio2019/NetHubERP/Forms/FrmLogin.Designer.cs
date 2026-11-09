namespace NetHubERP.Forms
{
    partial class FrmLogin
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel loginPanel;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnFirstAdmin;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.loginPanel = new System.Windows.Forms.Panel();
            this.lblBrand = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnFirstAdmin = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.loginPanel.Name = "loginPanel";
            this.loginPanel.Size = new System.Drawing.Size(405, 390);
            this.lblBrand.Text = "NetHub";
            this.lblBrand.Location = new System.Drawing.Point(34, 25);
            this.lblBrand.AutoSize = true;
            this.lblSubtitle.Text = "Sistema de controle de estoque";
            this.lblSubtitle.Location = new System.Drawing.Point(35, 67);
            this.lblSubtitle.AutoSize = true;
            this.lblStatus.Text = "Verificando conexão...";
            this.lblStatus.Location = new System.Drawing.Point(35, 99);
            this.lblStatus.Size = new System.Drawing.Size(338, 30);
            this.lblEmail.Text = "E-mail";
            this.lblEmail.Location = new System.Drawing.Point(35, 148);
            this.lblEmail.AutoSize = true;
            this.txtEmail.Location = new System.Drawing.Point(35, 169);
            this.txtEmail.Size = new System.Drawing.Size(335, 26);
            this.txtEmail.Name = "txtEmail";
            this.lblPassword.Text = "Senha";
            this.lblPassword.Location = new System.Drawing.Point(35, 216);
            this.lblPassword.AutoSize = true;
            this.txtPassword.Location = new System.Drawing.Point(35, 238);
            this.txtPassword.Size = new System.Drawing.Size(335, 26);
            this.txtPassword.Name = "txtPassword";
            this.btnLogin.Text = "Entrar";
            this.btnLogin.Location = new System.Drawing.Point(35, 287);
            this.btnLogin.Size = new System.Drawing.Size(335, 40);
            this.btnFirstAdmin.Text = "Configurar primeiro administrador";
            this.btnFirstAdmin.Location = new System.Drawing.Point(35, 336);
            this.btnFirstAdmin.Size = new System.Drawing.Size(335, 35);
            this.loginPanel.Controls.Add(this.lblBrand);
            this.loginPanel.Controls.Add(this.lblSubtitle);
            this.loginPanel.Controls.Add(this.lblStatus);
            this.loginPanel.Controls.Add(this.lblEmail);
            this.loginPanel.Controls.Add(this.txtEmail);
            this.loginPanel.Controls.Add(this.lblPassword);
            this.loginPanel.Controls.Add(this.txtPassword);
            this.loginPanel.Controls.Add(this.btnLogin);
            this.loginPanel.Controls.Add(this.btnFirstAdmin);
            this.Controls.Add(this.loginPanel);
            this.Name = "FrmLogin";
            this.Text = "NetHub ERP — Login";
            this.ResumeLayout(false);
        }
    }
}
