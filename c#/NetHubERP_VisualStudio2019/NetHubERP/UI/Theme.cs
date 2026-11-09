using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetHubERP.UI
{
    public static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(27, 32, 38);
        public static readonly Color Panel = Color.FromArgb(36, 43, 51);
        public static readonly Color Field = Color.FromArgb(44, 52, 61);
        public static readonly Color Border = Color.FromArgb(55, 65, 76);
        public static readonly Color Text = Color.FromArgb(227, 232, 238);
        public static readonly Color Muted = Color.FromArgb(154, 166, 178);
        public static readonly Color Accent = Color.FromArgb(70, 136, 130);
        public static readonly Color AccentLight = Color.FromArgb(90, 169, 163);
        public static readonly Color Danger = Color.FromArgb(201, 107, 107);
        public static readonly Color Warning = Color.FromArgb(209, 164, 90);
        public static readonly Color Success = Color.FromArgb(107, 179, 138);
        public static readonly Font Normal = new Font("Segoe UI", 9.5f);

        public static void Form(Form f, int width = 960, int height = 700)
        {
            f.BackColor = Bg; f.ForeColor = Text; f.Font = Normal;
            f.StartPosition = FormStartPosition.CenterScreen;
            f.ClientSize = new Size(width, height);
            f.MinimumSize = new Size(Math.Min(width, 820), Math.Min(height, 550));
        }
        public static Button Button(string text, EventHandler click, string kind = "primary")
        {
            var b = new Button { Text = text, Height = 39, AutoSize = false, FlatStyle = FlatStyle.Flat,
                BackColor = kind == "danger" ? Danger : kind == "secondary" ? Field : Accent,
                ForeColor = Text, Font = Normal, Cursor = Cursors.Hand, Margin = new Padding(4) };
            b.FlatAppearance.BorderSize = kind == "secondary" ? 1 : 0;
            b.FlatAppearance.BorderColor = Border;
            if (click != null) b.Click += click;
            return b;
        }
        public static TextBox Input(int max = 180, bool password = false)
        {
            return new TextBox { BorderStyle = BorderStyle.FixedSingle, BackColor = Field, ForeColor = Text,
                Font = Normal, Width = 260, Height = 30, MaxLength = max, UseSystemPasswordChar = password,
                Margin = new Padding(2, 3, 2, 8) };
        }
        public static NumericUpDown Number(decimal max = 1000000, int decimalPlaces = 0)
        {
            return new NumericUpDown { Minimum = 0, Maximum = max, DecimalPlaces = decimalPlaces,
                Increment = decimalPlaces > 0 ? 0.50m : 1, BackColor = Field, ForeColor = Text,
                Font = Normal, BorderStyle = BorderStyle.FixedSingle, Width = 250,
                ThousandsSeparator = true, Margin = new Padding(2, 3, 2, 8) };
        }
        public static ComboBox Combo()
        {
            return new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Field,
                ForeColor = Text, Font = Normal, FlatStyle = FlatStyle.Flat,
                Height = 32, Width = 260, Margin = new Padding(2, 3, 2, 8) };
        }
        public static Label Label(string text, int size = 10, bool bold = false)
        {
            return new Label { Text = text, ForeColor = Text, AutoSize = true, BackColor = Color.Transparent,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), Margin = new Padding(3, 5, 3, 5) };
        }
        public static Label Hint(string text)
        {
            var l = Label(text, 9); l.ForeColor = Muted; return l;
        }
        public static Label Title(string text)
        {
            var l = Label(text, 21, true); l.Dock = DockStyle.Left; return l;
        }
        public static Panel Card()
        {
            return new Panel { Dock = DockStyle.Fill, BackColor = Panel, Padding = new Padding(16),
                Margin = new Padding(6), BorderStyle = BorderStyle.FixedSingle };
        }
        public static DataGridView Grid()
        {
            var g = new DataGridView {
                Dock = DockStyle.Fill, BackgroundColor = Panel, BorderStyle = BorderStyle.None,
                GridColor = Border, ForeColor = Text, EnableHeadersVisualStyles = false,
                ReadOnly = true, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = Normal, ColumnHeadersHeight = 38, RowTemplate = { Height = 38 },
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, Margin = new Padding(5)
            };
            g.DefaultCellStyle.BackColor = Panel;
            g.DefaultCellStyle.ForeColor = Text;
            g.DefaultCellStyle.SelectionBackColor = Accent;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.DefaultCellStyle.Padding = new Padding(5);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 48, 56);
            g.AlternatingRowsDefaultCellStyle.ForeColor = Text;
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Accent;
            g.ColumnHeadersDefaultCellStyle.BackColor = Field;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Muted;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Field;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;
            return g;
        }
        public static DataGridViewTextBoxColumn Col(string name, string title, float fill = 100)
        {
            return new DataGridViewTextBoxColumn { Name = name, HeaderText = title, FillWeight = fill,
                SortMode = DataGridViewColumnSortMode.Programmatic };
        }
        public static DataGridViewButtonColumn ActionCol(string name, string text)
        {
            return new DataGridViewButtonColumn { Name = name, HeaderText = "", Text = text,
                UseColumnTextForButtonValue = true, FillWeight = 54, FlatStyle = FlatStyle.Flat,
                SortMode = DataGridViewColumnSortMode.NotSortable };
        }
        public static Panel Toolbar(string text, out Panel right)
        {
            var p = new Panel { Dock = DockStyle.Fill, Height = 52 };
            var label = Title(text); p.Controls.Add(label);
            right = new Panel { Dock = DockStyle.Right, Width = 170 };
            p.Controls.Add(right);
            return p;
        }
        public static void Error(IWin32Window owner, Exception ex)
        {
            string message = ex.Message;
            var sql = ex as System.Data.SqlClient.SqlException;
            if (sql != null && (sql.Number == 2601 || sql.Number == 2627))
                message = "Já existe um registro com o mesmo código ou e-mail.";
            else if (sql != null && (sql.Number == 53 || sql.Number == 40 || sql.Number == 4060))
                message = "Não foi possível conectar ao SQL Server. Confira o App.config, a instância e o banco NetHubERP.";
            MessageBox.Show(owner, message, "NetHub ERP — Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void Info(IWin32Window owner, string msg)
        {
            MessageBox.Show(owner, msg, "NetHub ERP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static void AddField(TableLayoutPanel layout, int row, string caption, Control input, int labelColumn = 0)
        {
            // Layout de diálogo: uma legenda e seu campo em duas linhas.
            var area = new Panel { Dock = DockStyle.Fill, Margin = new Padding(4), BackColor = Panel };
            var label = Hint(caption); label.Location = new Point(2, 0); label.AutoSize = true;
            input.Location = new Point(2, 25); input.Width = 260;
            input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            area.Resize += (sender, e) => input.Width = Math.Max(80, area.ClientSize.Width - 8);
            area.Controls.Add(label); area.Controls.Add(input);
            layout.Controls.Add(area, labelColumn, row);
        }
    }
}
