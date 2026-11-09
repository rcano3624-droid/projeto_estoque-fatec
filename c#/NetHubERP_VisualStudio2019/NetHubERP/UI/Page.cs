using System;
using System.Windows.Forms;
using NetHubERP.Models;

namespace NetHubERP.UI
{
    public class Page : UserControl
    {
        protected readonly UserSession Current;
        protected readonly Action<string> Navigate;
        protected readonly TableLayoutPanel Body;
        protected Page(UserSession current, Action<string> navigate)
        {
            Current = current; Navigate = navigate;
            Dock = DockStyle.Fill; BackColor = Theme.Bg; Font = Theme.Normal;
            Body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 0,
                BackColor = Theme.Bg, Padding = new Padding(20), Margin = new Padding(0) };
            Body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(Body);
        }
        protected void Fixed(Control c, int height)
        {
            int row = Body.RowCount++;
            Body.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            c.Dock = DockStyle.Fill;
            Body.Controls.Add(c, 0, row);
        }
        protected void Fill(Control c)
        {
            int row = Body.RowCount++;
            Body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            c.Dock = DockStyle.Fill;
            Body.Controls.Add(c, 0, row);
        }
        protected Panel Header(string title, EventHandler click = null, string action = null)
        {
            Panel right;
            var panel = Theme.Toolbar(title, out right);
            if (click != null)
            {
                var b = Theme.Button(action ?? "Novo", click);
                b.Dock = DockStyle.Fill;
                right.Controls.Add(b);
            }
            return panel;
        }
    }
}
