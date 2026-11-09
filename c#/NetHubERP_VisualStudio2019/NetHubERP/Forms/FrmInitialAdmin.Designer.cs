namespace NetHubERP.Forms
{
    partial class FrmInitialAdmin
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose(); base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.Text = "Configuração inicial";
            this.Name = "FrmInitialAdmin";
        }
    }
}
