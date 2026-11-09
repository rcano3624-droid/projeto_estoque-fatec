namespace NetHubERP.Forms
{
    partial class FrmMain
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel sidebar;
        private System.Windows.Forms.Panel area;
        private System.Windows.Forms.FlowLayoutPanel menu;
        private System.Windows.Forms.Label brand;
        private System.Windows.Forms.Label subtitle;
        private System.Windows.Forms.Label who;
        private System.Windows.Forms.Button logout;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.sidebar = new System.Windows.Forms.Panel();
            this.area = new System.Windows.Forms.Panel();
            this.menu = new System.Windows.Forms.FlowLayoutPanel();
            this.brand = new System.Windows.Forms.Label();
            this.subtitle = new System.Windows.Forms.Label();
            this.who = new System.Windows.Forms.Label();
            this.logout = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.sidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebar.Width = 250;
            this.sidebar.Name = "sidebar";
            this.area.Dock = System.Windows.Forms.DockStyle.Fill;
            this.area.Name = "area";
            this.Controls.Add(this.area);
            this.Controls.Add(this.sidebar);
            this.Name = "FrmMain";
            this.Text = "NetHub ERP — Controle de estoque";
            this.ResumeLayout(false);
        }
    }
}
