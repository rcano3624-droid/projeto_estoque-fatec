using System;
using System.Drawing;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Forms
{
    public partial class FrmMove : Form
    {
        private readonly Product _product;
        private readonly bool _entry;
        private readonly UserSession _actor;
        public FrmMove() : this(new Product { Nome = "Prévia" }, true, null) { } // Designer.
        public FrmMove(Product product, bool entry, UserSession actor)
        {
            InitializeComponent(); Theme.Form(this, 450, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
            _product = product; _entry = entry; _actor = actor;
            Text = (entry ? "Entrada: " : "Saída: ") + product.Nome;
            var l = Theme.Hint("Quantidade de unidades (disponível: " + product.Quantidade + ")");
            l.Location = new Point(24, 25); Controls.Add(l);
            number.SetBounds(24, 59, 393, 30);
            number.Minimum = 1; number.Maximum = 100000000; number.Value = 1;
            number.ForeColor = Theme.Text; number.BackColor = Theme.Field;
            Controls.Add(number);
            var info = Theme.Hint(entry ? "A entrada será registrada com motivo 'Movimentação manual'." :
                "A saída será registrada com motivo 'Movimentação manual'.");
            info.SetBounds(24, 105, 390, 38); info.AutoSize = false;
            Controls.Add(info);
            var save = Theme.Button("Confirmar", (s,e) => Save()); save.SetBounds(24, 155, 198, 40);
            var cancel = Theme.Button("Cancelar", (s,e) => { DialogResult = DialogResult.Cancel; Close(); }, "secondary");
            cancel.SetBounds(230, 155, 187, 40);
            Controls.Add(save); Controls.Add(cancel);
        }
        private void Save()
        {
            try
            {
                Repository.Move(_product.Id, _entry, (int)number.Value, "Movimentação manual", null, null, _actor);
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { Theme.Error(this, ex); }
        }
    }
}
