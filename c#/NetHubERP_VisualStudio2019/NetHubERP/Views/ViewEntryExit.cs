using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetHubERP.Data;
using NetHubERP.Models;
using NetHubERP.UI;

namespace NetHubERP.Views
{
    public class ViewEntryExit : Page
    {
        // Superfície de design segura: não consulta o banco.
        public ViewEntryExit() : base(new UserSession { Nome = "Prévia", Perfil = "admin" }, key => { })
        { Fixed(Header("Entrada de mercadorias"), 64); }
        private sealed class Choice
        {
            public int Id { get; set; }
            public string Label { get; set; }
        }
        private readonly bool _entry;
        private readonly ComboBox product, person, reason;
        private readonly NumericUpDown quantity;
        public ViewEntryExit(UserSession user, Action<string> go, bool entry) : base(user, go)
        {
            _entry = entry;
            Fixed(Header(entry ? "Entrada de mercadorias" : "Saída de mercadorias"), 70);
            var box = Theme.Card();
            box.Margin = new Padding(6, 6, 6, 6);
            var inner = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Panel };
            box.Controls.Add(inner);
            var prodLabel = Theme.Hint("Produto"); prodLabel.SetBounds(12, 9, 160, 23);
            product = Theme.Combo(); product.SetBounds(12, 35, 435, 34);
            var personLabel = Theme.Hint(entry ? "Fornecedor" : "Cliente (opcional)");
            personLabel.SetBounds(12, 80, 180, 23);
            person = Theme.Combo(); person.SetBounds(12, 107, 435, 34);
            var reasonLabel = Theme.Hint("Motivo da saída"); reasonLabel.SetBounds(12, 149, 160, 23);
            reason = Theme.Combo(); reason.SetBounds(12, 175, 435, 34);
            reason.Items.AddRange(new object[] { "Venda", "Transferência", "Perda ou avaria" });
            reason.SelectedIndex = 0;
            var qtyLabel = Theme.Hint("Quantidade");
            qtyLabel.SetBounds(12, entry ? 150 : (user.Admin ? 219 : 150), 160, 23);
            quantity = Theme.Number(100000000);
            quantity.Minimum = 1; quantity.Value = 1; quantity.SetBounds(12, qtyLabel.Bottom + 4, 435, 31);
            var btn = Theme.Button(entry ? "Registrar entrada" : "Registrar saída", (s,e) => Register());
            btn.SetBounds(12, quantity.Bottom + 25, 215, 44);
            inner.Controls.Add(prodLabel); inner.Controls.Add(product);
            inner.Controls.Add(personLabel); inner.Controls.Add(person);
            inner.Controls.Add(reasonLabel); inner.Controls.Add(reason);
            inner.Controls.Add(qtyLabel); inner.Controls.Add(quantity); inner.Controls.Add(btn);
            reasonLabel.Visible = !entry;
            reason.Visible = !entry;
            personLabel.Visible = entry || user.Admin;
            person.Visible = entry || user.Admin;
            if (entry)
            {
                qtyLabel.Top = person.Bottom + 20; quantity.Top = qtyLabel.Bottom + 4; btn.Top = quantity.Bottom + 25;
            }
            if (!entry && !user.Admin)
            {
                reasonLabel.Top = product.Bottom + 20; reason.Top = reasonLabel.Bottom + 4;
                qtyLabel.Top = reason.Bottom + 20; quantity.Top = qtyLabel.Bottom + 4; btn.Top = quantity.Bottom + 25;
            }
            Fixed(box, 420);
            var products = Repository.Products().Select(p => new Choice {
                Id = p.Id, Label = p.Codigo + " - " + p.Nome + " (" + p.Quantidade + " un.)" }).ToList();
            product.DataSource = products; product.DisplayMember = "Label"; product.ValueMember = "Id";
            var choices = new List<Choice> { new Choice { Id = 0, Label = entry ? "Não informado" : "Nenhum" } };
            if (entry)
                choices.AddRange(Repository.Contacts(true).Select(c => new Choice { Id = c.Id, Label = c.Nome }));
            else if (user.Admin)
                choices.AddRange(Repository.Contacts(false).Select(c => new Choice { Id = c.Id, Label = c.Nome }));
            person.DataSource = choices; person.DisplayMember = "Label"; person.ValueMember = "Id";
            var blank = Theme.Hint("Cada registro atualiza o estoque e salva uma movimentação com data e usuário.");
            blank.Margin = new Padding(10, 20, 0, 0);
            Fixed(blank, 46);
            Fill(new Panel { BackColor = Theme.Bg });
        }
        private void Register()
        {
            try
            {
                var item = product.SelectedItem as Choice;
                if (item == null) throw new ArgumentException("Cadastre um produto antes de registrar movimentações.");
                var selected = person.SelectedItem as Choice;
                int? contactId = selected == null || selected.Id == 0 ? (int?)null : selected.Id;
                string why = _entry
                    ? "Fornecedor: " + (selected == null ? "Não informado" : selected.Label)
                    : Convert.ToString(reason.SelectedItem) +
                      (Current.Admin && contactId.HasValue ? " - Cliente: " + selected.Label : "");
                Repository.Move(item.Id, _entry, (int)quantity.Value, why,
                    _entry ? (int?)null : contactId, _entry ? contactId : (int?)null, Current);
                Theme.Success(FindForm(), _entry ? "Entrada registrada." : "Saída registrada.");
                Navigate(_entry ? "entrada" : "saida");
            }
            catch (Exception ex) { Theme.Error(FindForm(), ex); }
        }
    }
}
