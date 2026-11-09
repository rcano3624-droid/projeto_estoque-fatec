namespace NetHubERP.Forms
{
    partial class FrmContact
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtName, txtExtra, txtPhone, txtCity;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }
        private void InitializeComponent()
        {
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtExtra = new System.Windows.Forms.TextBox();
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.txtCity = new System.Windows.Forms.TextBox();
            this.Name = "FrmContact";
        }
    }
}
