namespace NetHubERP.Forms
{
    partial class FrmProduct
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtCode, txtName, txtBrand, txtCategory, txtLocation;
        private System.Windows.Forms.NumericUpDown numPrice, numQuantity, numMin;
        private System.Windows.Forms.PictureBox imagePreview;
        private System.Windows.Forms.Button btnChoose, btnSave, btnCancel;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.txtCode = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtBrand = new System.Windows.Forms.TextBox();
            this.txtCategory = new System.Windows.Forms.TextBox();
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.numPrice = new System.Windows.Forms.NumericUpDown();
            this.numQuantity = new System.Windows.Forms.NumericUpDown();
            this.numMin = new System.Windows.Forms.NumericUpDown();
            this.imagePreview = new System.Windows.Forms.PictureBox();
            this.btnChoose = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.Name = "FrmProduct";
            this.Text = "Produto";
        }
    }
}
