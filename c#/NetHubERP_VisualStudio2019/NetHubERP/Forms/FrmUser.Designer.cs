namespace NetHubERP.Forms
{
    partial class FrmUser
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox nameBox, emailBox, passwordBox;
        private System.Windows.Forms.ComboBox roleBox;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }
        private void InitializeComponent()
        {
            nameBox = new System.Windows.Forms.TextBox(); emailBox = new System.Windows.Forms.TextBox();
            passwordBox = new System.Windows.Forms.TextBox(); roleBox = new System.Windows.Forms.ComboBox();
            this.Name = "FrmUser"; this.Text = "Criar usuário";
        }
    }
}
