using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Forms
{
    public partial class FrmProduct : Form
    {
        private readonly Product _product;
        private readonly UserSession _actor;
        private byte[] _picture;
        public FrmProduct() : this(null, null) { } // Prévia no Designer.
        public FrmProduct(Product product, UserSession actor)
        {
            InitializeComponent(); Theme.Form(this, 695, 620);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            _product = product ?? new Product();
            _actor = actor;
            Text = _product.Id == 0 ? "Novo produto" : "Editar produto";
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.Panel,
                ColumnCount = 2, Padding = new Padding(18), RowCount = 6 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i=0; i<4; i++) root.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            Controls.Add(root);
            Setup(txtCode, 50); Setup(txtName, 180); Setup(txtBrand, 100);
            Setup(txtCategory, 80); Setup(txtLocation, 120);
            var suggestions = new AutoCompleteStringCollection();
            suggestions.AddRange(new[] { "Televisão", "Celular", "Geladeira", "Fogão", "Micro-ondas",
                "Máquina de lavar", "Ar-condicionado", "Notebook", "Outros" });
            txtCategory.AutoCompleteCustomSource = suggestions;
            txtCategory.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtCategory.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            SetupNumber(numPrice, 999999999m, 2); SetupNumber(numQuantity, 100000000);
            SetupNumber(numMin, 100000000);
            Theme.AddField(root, 0, "Código / SKU", txtCode);
            Theme.AddField(root, 0, "Categoria", txtCategory, 1);
            Theme.AddField(root, 1, "Produto", txtName);
            Theme.AddField(root, 1, "Marca", txtBrand, 1);
            Theme.AddField(root, 2, "Local de armazenagem", txtLocation);
            Theme.AddField(root, 2, "Preço (R$)", numPrice, 1);
            Theme.AddField(root, 3, "Quantidade", numQuantity);
            Theme.AddField(root, 3, "Estoque mínimo", numMin, 1);
            var photoPanel = Theme.Card();
            var label = Theme.Hint("Foto do produto (opcional, até 10 MB)");
            label.Location = new Point(12, 8); photoPanel.Controls.Add(label);
            imagePreview.SizeMode = PictureBoxSizeMode.Zoom;
            imagePreview.SetBounds(12, 40, 185, 150);
            imagePreview.BackColor = Theme.Field;
            photoPanel.Controls.Add(imagePreview);
            btnChoose = Theme.Button("Selecionar imagem", (s,e) => SelectImage(), "secondary");
            btnChoose.SetBounds(220, 42, 194, 40);
            photoPanel.Controls.Add(btnChoose);
            var info = Theme.Hint("Imagem reduzida para 320 px e salva no banco de dados.");
            info.SetBounds(220, 95, 375, 47); info.AutoSize = false;
            photoPanel.Controls.Add(info);
            root.Controls.Add(photoPanel, 0, 4);
            root.SetColumnSpan(photoPanel, 2);
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false, Padding = new Padding(3) };
            btnSave = Theme.Button("Salvar", (s,e) => Save()); btnSave.Width = 150;
            btnCancel = Theme.Button("Cancelar", (s,e) => { DialogResult = DialogResult.Cancel; Close(); }, "secondary");
            btnCancel.Width = 132;
            footer.Controls.Add(btnSave); footer.Controls.Add(btnCancel);
            root.Controls.Add(footer, 0, 5); root.SetColumnSpan(footer, 2);
            txtCode.Text = _product.Codigo ?? "";
            txtName.Text = _product.Nome ?? "";
            txtBrand.Text = _product.Marca ?? "";
            txtCategory.Text = _product.Categoria ?? "Outros";
            txtLocation.Text = _product.Local ?? "";
            numPrice.Value = Clamp(_product.Preco, numPrice.Maximum);
            numQuantity.Value = Clamp(_product.Quantidade, numQuantity.Maximum);
            numMin.Value = Clamp(_product.Minimo, numMin.Maximum);
            _picture = _product.Foto;
            DisplayPicture();
        }
        private static decimal Clamp(decimal v, decimal max) { return Math.Max(0, Math.Min(max, v)); }
        private void Setup(TextBox box, int max)
        {
            box.BackColor = Theme.Field; box.ForeColor = Theme.Text;
            box.BorderStyle = BorderStyle.FixedSingle; box.MaxLength = max;
            box.Font = Theme.Normal;
        }
        private void SetupNumber(NumericUpDown control, decimal max, int digits = 0)
        {
            control.Minimum = 0; control.Maximum = max; control.DecimalPlaces = digits;
            control.ThousandsSeparator = true;
            control.BackColor = Theme.Field; control.ForeColor = Theme.Text;
            control.Font = Theme.Normal;
        }
        private void DisplayPicture()
        {
            if (imagePreview.Image != null) { var old = imagePreview.Image; imagePreview.Image = null; old.Dispose(); }
            if (_picture == null || _picture.Length == 0) return;
            using (var stream = new MemoryStream(_picture))
            using (var img = Image.FromStream(stream)) imagePreview.Image = new Bitmap(img);
        }
        private void SelectImage()
        {
            using (var d = new OpenFileDialog { Title = "Selecione a foto do produto",
                Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp;*.gif" })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var f = new FileInfo(d.FileName);
                    if (f.Length > 10L * 1024 * 1024) throw new ArgumentException("A imagem deve ter no máximo 10 MB.");
                    using (var raw = Image.FromFile(d.FileName))
                    {
                        float factor = Math.Min(1f, 320f / Math.Max(raw.Width, raw.Height));
                        int w = Math.Max(1, (int)Math.Round(raw.Width * factor));
                        int h = Math.Max(1, (int)Math.Round(raw.Height * factor));
                        using (var resized = new Bitmap(w, h))
                        using (var g = Graphics.FromImage(resized))
                        {
                            g.Clear(Color.White);
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(raw, 0, 0, w, h);
                            using (var memory = new MemoryStream())
                            {
                                var codec = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType == "image/jpeg");
                                using (var encoder = new EncoderParameters(1))
                                {
                                    encoder.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 75L);
                                    resized.Save(memory, codec, encoder);
                                }
                                _picture = memory.ToArray();
                            }
                        }
                    }
                    DisplayPicture();
                }
                catch (Exception ex) { Theme.Error(this, ex); }
            }
        }
        private void Save()
        {
            try
            {
                _product.Codigo = txtCode.Text.Trim(); _product.Nome = txtName.Text.Trim();
                _product.Marca = txtBrand.Text.Trim();
                _product.Categoria = string.IsNullOrWhiteSpace(txtCategory.Text) ? "Outros" : txtCategory.Text.Trim();
                _product.Local = txtLocation.Text.Trim();
                _product.Preco = numPrice.Value; _product.Quantidade = (int)numQuantity.Value;
                _product.Minimo = (int)numMin.Value; _product.Foto = _picture;
                Repository.SaveProduct(_product, _actor);
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { Theme.Error(this, ex); }
        }
    }
}
