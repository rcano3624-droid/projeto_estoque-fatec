using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NetHubERP.Models;
using NetHubERP.UI;
using NetHubERP.Views;

namespace NetHubERP.Forms
{
    public partial class FrmMain : Form
    {
        private readonly UserSession _current;
        private readonly Dictionary<string, Button> _buttons = new Dictionary<string, Button>();
        private UserControl _page;
        // Construtor sem parâmetros permite visualizar a moldura no Designer do VS2019 sem consultar o SQL Server.
        public FrmMain() : this(null) { }
        public FrmMain(UserSession current)
        {
            _current = current ?? new UserSession { Nome = "Prévia", Perfil = "admin" };
            InitializeComponent();
            Theme.Form(this, 1180, 760);
            MinimumSize = new Size(990, 650);
            sidebar.BackColor = Theme.Panel;
            area.BackColor = Theme.Bg;
            BuildSide();
            if (current != null) Go("dashboard");
        }
        private void BuildSide()
        {
            brand = Theme.Label("NetHub", 18, true);
            brand.ForeColor = Theme.AccentLight;
            brand.Location = new Point(22, 20);
            subtitle = Theme.Hint("ERP - Linha branca");
            subtitle.Location = new Point(22, 53);
            sidebar.Controls.Add(brand);
            sidebar.Controls.Add(subtitle);
            menu = new FlowLayoutPanel { Location = new Point(10, 90), Width = 230,
                Height = sidebar.ClientSize.Height - 200, Anchor = AnchorStyles.Top | AnchorStyles.Bottom,
                FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            sidebar.Controls.Add(menu);
            AddMenu("dashboard", "Dashboard");
            AddMenu("estoque", "Estoque");
            AddMenu("entrada", "Entrada de mercadorias");
            AddMenu("saida", "Saída de mercadorias");
            if (_current.Admin)
            {
                AddMenu("clientes", "Clientes"); AddMenu("fornecedores", "Fornecedores");
                AddMenu("relatorios", "Relatórios");
            }
            AddMenu("historico", "Histórico");
            if (_current.Admin) AddMenu("usuarios", "Usuários");
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 93, Padding = new Padding(12), BackColor = Theme.Panel };
            who = Theme.Hint(_current.Nome + " (" + (_current.Admin ? "Administrador" : "Funcionário") + ")");
            who.Dock = DockStyle.Top; who.AutoEllipsis = true; who.AutoSize = false; who.Height = 28;
            logout = Theme.Button("Sair", (s, e) => Close(), "secondary");
            logout.Dock = DockStyle.Bottom;
            bottom.Controls.Add(who); bottom.Controls.Add(logout);
            sidebar.Controls.Add(bottom);
        }
        private void AddMenu(string id, string title)
        {
            var b = Theme.Button(title, (s, e) => Go(id), "secondary");
            b.FlatAppearance.BorderSize = 0; b.TextAlign = ContentAlignment.MiddleLeft;
            b.BackColor = Theme.Panel; b.ForeColor = Theme.Muted;
            b.Width = 214; b.Height = 42; b.Margin = new Padding(2);
            menu.Controls.Add(b); _buttons[id] = b;
        }
        public void Go(string key)
        {
            if (!_current.Admin && (key == "clientes" || key == "fornecedores" || key == "relatorios" || key == "usuarios"))
                return;
            try
            {
                UserControl view;
                switch (key)
                {
                    case "dashboard": view = new ViewDashboard(_current, Go); break;
                    case "estoque": view = new ViewStock(_current, Go); break;
                    case "entrada": view = new ViewEntryExit(_current, Go, true); break;
                    case "saida": view = new ViewEntryExit(_current, Go, false); break;
                    case "clientes": view = new ViewContacts(_current, Go, false); break;
                    case "fornecedores": view = new ViewContacts(_current, Go, true); break;
                    case "relatorios": view = new ViewReports(_current, Go); break;
                    case "historico": view = new ViewHistory(_current, Go); break;
                    case "usuarios": view = new ViewUsers(_current, Go); break;
                    default: return;
                }
                area.SuspendLayout();
                area.Controls.Clear();
                if (_page != null) _page.Dispose();
                _page = view;
                area.Controls.Add(view);
                area.ResumeLayout();
                foreach (var entry in _buttons)
                {
                    bool active = entry.Key == key;
                    entry.Value.BackColor = active ? Theme.Accent : Theme.Panel;
                    entry.Value.ForeColor = active ? Color.White : Theme.Muted;
                }
            }
            catch (Exception ex) { Theme.Error(this, ex); }
        }
    }
}
