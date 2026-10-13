const CATEGORIAS = ["Televisão", "Celular", "Geladeira", "Fogão", "Micro-ondas", "Máquina de lavar", "Ar-condicionado", "Notebook", "Outros"];

    const K = {
      itens: "gv_itens",
      clientes: "gv_clientes",
      fornecedores: "gv_fornecedores",
      users: "gv_users",
      log: "gv_log"
    };

    const MENU = [
      ["dashboard", "Dashboard"],
      ["estoque", "Estoque"],
      ["entrada", "Entrada de mercadorias"],
      ["saida", "Saída de mercadorias"],
      ["clientes", "Clientes", 1],
      ["fornecedores", "Fornecedores", 1],
      ["relatorios", "Relatórios", 1],
      ["historico", "Histórico"],
      ["usuarios", "Usuários", 1]
    ];

    const CAD = {
      clientes: {
        t: "cliente",
        tt: "Clientes",
        f: [["nome", "Nome"], ["telefone", "Telefone"], ["email", "E-mail"], ["cidade", "Cidade"]]
      },
      fornecedores: {
        t: "fornecedor",
        tt: "Fornecedores",
        f: [["nome", "Empresa"], ["contato", "Contato"], ["telefone", "Telefone"], ["cidade", "Cidade"]]
      }
    };

    const $ = id => document.getElementById(id);

    // API_BASE_URL: informe a URL do backend quando ele estiver disponível.
    // Ex.: const API_BASE_URL = "http://localhost:3000";
    const API_BASE_URL = "";
    const memoria = {};
    let sincronizando = false;

    const ler = (k, d) => memoria[k] ?? d;

    function notificar(mensagem, tipo = "info") {
      const area = $("notificacoes");
      if (!area) { alert(mensagem); return; }
      const el = document.createElement("div");
      el.className = `toast ${tipo}`;
      el.setAttribute("role", tipo === "erro" ? "alert" : "status");
      el.textContent = mensagem;
      area.appendChild(el);
      setTimeout(() => el.remove(), 5000);
    }

    async function requisicaoAPI(caminho, opcoes = {}) {
      if (!API_BASE_URL) throw new Error("Backend não configurado. O sistema está em modo de demonstração e os dados não serão mantidos após atualizar a página.");
      let resposta;
      try {
        resposta = await fetch(`${API_BASE_URL}${caminho}`, {
          ...opcoes,
          headers: { ...(opcoes.body instanceof FormData ? {} : { "Content-Type": "application/json" }), ...opcoes.headers }
        });
      } catch {
        throw new Error("Não foi possível conectar ao servidor. Verifique a conexão e tente novamente.");
      }
      const tipo = resposta.headers.get("content-type") || "";
      const corpo = tipo.includes("application/json") ? await resposta.json().catch(() => ({})) : await resposta.text().catch(() => "");
      if (!resposta.ok) throw new Error(corpo?.message || corpo?.erro || `Erro do servidor (${resposta.status}).`);
      return corpo;
    }

    async function sincronizarEstado() {
      if (!API_BASE_URL || sincronizando) return;
      sincronizando = true;
      try {
        await requisicaoAPI("/api/erp/state", { method: "PUT", body: JSON.stringify({
          itens: D.itens, clientes: D.clientes, fornecedores: D.fornecedores,
          users: D.users, log: D.log
        }) });
        atualizarConexao("Conectado ao servidor", true);
      } catch (e) {
        atualizarConexao("Falha na sincronização", false);
        notificar(e.message, "erro");
      } finally { sincronizando = false; }
    }

    const gravar = (k, v) => {
      memoria[k] = v;
      if (API_BASE_URL) sincronizarEstado();
      else atualizarConexao("Modo demonstração · sem servidor", false);
    };

    function atualizarConexao(texto, online) {
      const el = $("statusConexao");
      if (!el) return;
      el.textContent = texto;
      el.classList.toggle("online", !!online);
      el.classList.toggle("offline", !online);
    }

    async function carregarEstadoServidor() {
      if (!API_BASE_URL) return;
      try {
        const estado = await requisicaoAPI("/api/erp/state");
        ["itens", "clientes", "fornecedores", "users", "log"].forEach(k => {
          if (Array.isArray(estado[k])) { memoria[k] = estado[k]; D[k] = estado[k]; }
        });
        atualizarConexao("Conectado ao servidor", true);
      } catch (e) {
        atualizarConexao("Servidor indisponível", false);
        notificar(e.message, "erro");
        throw e;
      }
    }

    const esc = s => String(s ?? "").replace(/[&<>"']/g, c => ({
      "&": "&amp;",
      "<": "&lt;",
      ">": "&gt;",
      '"': "&quot;",
      "'": "&#39;"
    }[c]));

    const moeda = v => Number(v).toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

    const cp = (id, l, t, v, x = "") => `<div><label>${l}</label><input id="${id}" type="${t}" value="${esc(v)}" ${x}></div>`;

    const D = {};
    for (const t in K) D[t] = ler(K[t], []);
    const salvar = t => gravar(K[t], D[t]);

    let sessao = (() => { try { return JSON.parse(sessionStorage.getItem("gv_sessao")) || null; } catch { return null; } })(),
      ord = { campo: "nome", asc: true },
      telaAtual = "dashboard",
      foto = "",
      aoOk = null;

    const adm = () => sessao && sessao.perfil === "admin";

    /* Aproveita os dados do projeto antigo (indexV8), se existirem */
    function migrar() {
      // Migração antiga removida: os dados passam a vir da API quando configurada.
      // Sem backend, o modo demonstração mantém os dados apenas em memória.
    }

    async function hash(s) {
      if (window.crypto && crypto.subtle) {
        const b = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(s));
        return [...new Uint8Array(b)].map(x => x.toString(16).padStart(2, "0")).join("");
      }
      let h = 0;
      for (const c of s) h = (h * 31 + c.charCodeAt(0)) | 0;
      return "s" + h;
    }

    async function semear() {
      if (D.users.length) return;
      D.users = [
        { email: "admin@garotosvadios.com", nome: "Administrador", perfil: "admin", hash: await hash("123456") },
        { email: "funcionario@garotosvadios.com", nome: "Funcionário", perfil: "funcionario", hash: await hash("123456") }
      ];
      salvar("users");
    }

    /* Login */
    async function entrar() {
      const e = $("lEmail").value.trim().toLowerCase();
      const senha = $("lSenha").value;
      if (!e || !senha) {
        $("lErro").textContent = "Informe e-mail e senha.";
        notificar("Preencha o e-mail e a senha para continuar.", "erro");
        return;
      }
      let u;
      try {
        if (API_BASE_URL) {
          const retorno = await requisicaoAPI("/api/auth/login", {
            method: "POST", body: JSON.stringify({ email: e, senha })
          });
          u = retorno.usuario || retorno.user;
          if (!u) throw new Error("Resposta de login inválida recebida do servidor.");
          if (retorno.token) sessionStorage.setItem("gv_token", retorno.token);
        } else {
          const h = await hash(senha);
          u = D.users.find(x => x.email.toLowerCase() === e && x.hash === h);
        }
      } catch (erro) {
        $("lErro").textContent = erro.message;
        notificar(erro.message, "erro");
        return;
      }
      if (!u) {
        $("lErro").textContent = "E-mail ou senha incorretos.";
        notificar("Não foi possível entrar. Confira o e-mail e a senha.", "erro");
        return;
      }
      sessao = { email: u.email, nome: u.nome, perfil: u.perfil };
      sessionStorage.setItem("gv_sessao", JSON.stringify(sessao));
      $("lErro").textContent = "";
      $("lSenha").value = "";
      if (!API_BASE_URL) reg("Acesso", "Entrou no sistema");
      iniciar();
    }

    function sair() {
      sessionStorage.removeItem("gv_sessao");
      sessionStorage.removeItem("gv_token");
      location.reload();
    }

    function iniciar() {
      $("telaLogin").hidden = true;
      $("app").hidden = false;
      $("quem").textContent = `${sessao.nome} (${adm() ? "Administrador" : "Funcionário"})`;
      $("menu").innerHTML = MENU
        .filter(m => !m[2] || adm())
        .map(m => `<button data-t="${m[0]}" onclick="mostrar('${m[0]}')">${m[1]}</button>`)
        .join("");
      mostrar("dashboard");
    }

    function mostrar(t) {
      const m = MENU.find(x => x[0] === t);
      if (!m || (m[2] && !adm())) return;
      telaAtual = t;
      document.querySelectorAll("#menu button").forEach(b => b.classList.toggle("ativa", b.dataset.t === t));
      (CAD[t] ? () => telaCad(t) : TELAS[t])();
    }

    /* Histórico */
    function reg(tipo, desc) {
      D.log.unshift({
        data: new Date().toLocaleString("pt-BR"),
        usuario: sessao.nome,
        tipo,
        desc
      });
      D.log = D.log.slice(0, 500);
      salvar("log");
    }

    /* Janela genérica */
    function modal(t, html, ok, txt = "Salvar") {
      $("dlgT").textContent = t;
      $("dlgC").innerHTML = html;
      $("dlgOk").textContent = txt;
      aoOk = ok;
      $("dlg").showModal();
    }

    $("dlgOk").onclick = async () => {
      if (await aoOk() !== false) $("dlg").close();
    };

    /* Telas */
    const TELAS = {
      dashboard() {
        const baixos = D.itens.filter(i => i.qtd <= i.minimo);
        const c = [
          ["Produtos", D.itens.length],
          ["Unidades em estoque", D.itens.reduce((s, i) => s + i.qtd, 0)],
          ["Estoque baixo", baixos.length]
        ];
        if (adm()) {
          c.push(
            ["Clientes", D.clientes.length],
            ["Fornecedores", D.fornecedores.length],
            ["Valor do estoque", moeda(D.itens.reduce((s, i) => s + i.qtd * i.preco, 0))]
          );
        }
        $("conteudo").innerHTML = `<h1>Dashboard</h1>
          <div class="cards">${c.map(x => `<div class="card">${x[0]}<b>${x[1]}</b></div>`).join("")}</div>
          <div class="duas">
            <div class="painel"><h3>Precisam de reposição</h3>${baixos.slice(0, 8).map(i => `<div class="linha"><span>${esc(i.nome)}</span><b>${i.qtd} un.</b></div>`).join("") || "<small>Nenhum produto com estoque baixo.</small>"}</div>
            <div class="painel"><h3>Últimas movimentações</h3>${D.log.filter(l => l.tipo !== "Acesso").slice(0, 8).map(l => `<div class="linha"><span>${esc(l.desc)}</span><small>${esc(l.data)}</small></div>`).join("") || "<small>Nenhuma movimentação registrada.</small>"}</div>
          </div>`;
      },

      estoque() {
        const cats = [...new Set([...CATEGORIAS, ...D.itens.map(i => i.categoria)])].sort();
        $("conteudo").innerHTML = `<div class="topo"><h1>Estoque</h1>${adm() ? `<button onclick="modalItem()">Novo produto</button>` : ""}</div>
          <div class="painel"><div class="barra">
            <input id="busca" placeholder="Buscar por nome, marca, código, categoria ou local" oninput="pintarEstoque()">
            <select id="fCatF" onchange="pintarEstoque()"><option value="">Todas as categorias</option>${cats.map(c => `<option>${esc(c)}</option>`).join("")}</select>
            <select id="fSt" onchange="pintarEstoque()"><option value="">Todos os status</option><option value="baixo">Só estoque baixo</option></select>
          </div><div class="rolagem"><table><thead id="cab"></thead><tbody id="corpo"></tbody></table></div></div>`;
        pintarEstoque();
      },

      entrada() {
        $("conteudo").innerHTML = `<h1>Entrada de mercadorias</h1><div class="painel form">
          <div><label>Produto</label><select id="eProd"><option value="">Selecione</option>${opcoesProdutos()}</select></div>
          <div><label>Fornecedor</label><select id="eForn"><option>Não informado</option>${D.fornecedores.map(f => `<option>${esc(f.nome)}</option>`).join("")}</select></div>
          <div><label>Quantidade</label><input id="eQtd" type="number" min="1"></div>
          <button onclick="if(mover($('eProd').value,1,parseInt($('eQtd').value),' - Fornecedor: '+$('eForn').value)){alert('Entrada registrada.');mostrar('entrada')}">Registrar entrada</button></div>`;
      },

      saida() {
        $("conteudo").innerHTML = `<h1>Saída de mercadorias</h1><div class="painel form">
          <div><label>Produto</label><select id="sProd"><option value="">Selecione</option>${opcoesProdutos()}</select></div>
          <div><label>Motivo</label><select id="sMot"><option>Venda</option><option>Transferência</option><option>Perda ou avaria</option></select></div>
          ${adm() ? `<div><label>Cliente (opcional)</label><select id="sCli"><option value="">Nenhum</option>${D.clientes.map(c => `<option>${esc(c.nome)}</option>`).join("")}</select></div>` : ""}
          <div><label>Quantidade</label><input id="sQtd" type="number" min="1"></div>
          <button onclick="enviarSaida()">Registrar saída</button></div>`;
      },

      relatorios() {
        const por = {};
        D.itens.forEach(i => {
          const p = por[i.categoria] || (por[i.categoria] = { n: 0, q: 0, v: 0 });
          p.n++;
          p.q += i.qtd;
          p.v += i.qtd * i.preco;
        });
        const baixos = D.itens.filter(i => i.qtd <= i.minimo);
        $("conteudo").innerHTML = `<div class="topo"><h1>Relatórios</h1><button class="sec" onclick="exportarCSV()">Exportar estoque em CSV</button></div>
          <div class="duas">
            <div class="painel"><h3>Resumo geral</h3>
              <div class="linha"><span>Produtos</span><b>${D.itens.length}</b></div>
              <div class="linha"><span>Clientes</span><b>${D.clientes.length}</b></div>
              <div class="linha"><span>Fornecedores</span><b>${D.fornecedores.length}</b></div>
              <div class="linha"><span>Valor do estoque</span><b>${moeda(D.itens.reduce((s, i) => s + i.qtd * i.preco, 0))}</b></div></div>
            <div class="painel"><h3>Estoque baixo</h3>${baixos.map(i => `<div class="linha"><span>${esc(i.nome)}</span><b>${i.qtd} un.</b></div>`).join("") || "<small>Nenhum produto com estoque baixo.</small>"}</div>
          </div>
          <div class="painel"><h3>Por categoria</h3><div class="rolagem"><table><thead><tr><th>Categoria</th><th>Produtos</th><th>Unidades</th><th>Valor em estoque</th></tr></thead><tbody>
            ${Object.entries(por).map(([c, p]) => `<tr><td>${esc(c)}</td><td>${p.n}</td><td>${p.q}</td><td>${moeda(p.v)}</td></tr>`).join("") || `<tr><td colspan="4"><small>Sem dados.</small></td></tr>`}
          </tbody></table></div></div>`;
      },

      historico() {
        $("conteudo").innerHTML = `<h1>Histórico</h1><div class="painel"><div class="rolagem"><table><thead><tr><th>Data</th><th>Usuário</th><th>Tipo</th><th>Detalhe</th></tr></thead><tbody>
          ${D.log.map(l => `<tr><td>${esc(l.data)}</td><td>${esc(l.usuario)}</td><td>${esc(l.tipo)}</td><td>${esc(l.desc)}</td></tr>`).join("") || `<tr><td colspan="4"><small>Nada registrado ainda.</small></td></tr>`}
          </tbody></table></div></div>`;
      },

      usuarios() {
        $("conteudo").innerHTML = `<h1>Usuários</h1><div class="painel"><h3>Novo usuário</h3><div class="form2">
          ${cp("uEmail", "E-mail", "email", "")}${cp("uNome", "Nome", "text", "")}${cp("uSenha", "Senha (mínimo 4 caracteres)", "password", "")}
          <div><label>Perfil</label><select id="uPerfil"><option value="funcionario">Funcionário</option><option value="admin">Administrador</option></select></div>
        </div><div class="rodape"><button onclick="criarUsuario()">Criar usuário</button></div></div>
        <div class="painel"><table><thead><tr><th>E-mail</th><th>Nome</th><th>Perfil</th><th></th></tr></thead><tbody>
        ${D.users.map((u, i) => `<tr><td>${esc(u.email)}</td><td>${esc(u.nome)}</td><td>${u.perfil === "admin" ? "Administrador" : "Funcionário"}</td>
          <td>${u.email === sessao.email ? "" : `<button class="perigo" onclick="apagarUsuario(${i})">Excluir</button>`}</td></tr>`).join("")}
        </tbody></table></div>`;
      }
    };

    /* Estoque */
    const opcoesProdutos = () => D.itens.map(i => `<option value="${i.id}">${esc(i.codigo)} - ${esc(i.nome)} (${i.qtd} un.)</option>`).join("");

    function ordenar(c) {
      ord.asc = ord.campo === c ? !ord.asc : true;
      ord.campo = c;
      pintarEstoque();
    }

    function pintarEstoque() {
      const q = $("busca").value.toLowerCase(),
        cat = $("fCatF").value,
        baixo = $("fSt").value === "baixo";

      const L = D.itens
        .filter(i =>
          [i.nome, i.marca, i.codigo, i.categoria, i.local].join(" ").toLowerCase().includes(q) &&
          (!cat || i.categoria === cat) &&
          (!baixo || i.qtd <= i.minimo)
        )
        .sort((a, b) => {
          const x = a[ord.campo],
            y = b[ord.campo];
          const r = typeof x === "number" ? x - y : String(x).localeCompare(String(y), "pt-BR");
          return ord.asc ? r : -r;
        });

      const s = c => ord.campo === c ? (ord.asc ? " ▲" : " ▼") : "";

      $("cab").innerHTML = `<tr><th>Foto</th><th class="o" onclick="ordenar('codigo')">Código${s("codigo")}</th><th class="o" onclick="ordenar('nome')">Produto${s("nome")}</th><th class="o" onclick="ordenar('categoria')">Categoria${s("categoria")}</th><th class="o" onclick="ordenar('qtd')">Quantidade${s("qtd")}</th>${adm() ? `<th class="o" onclick="ordenar('preco')">Preço${s("preco")}</th>` : ""}<th class="o" onclick="ordenar('local')">Local${s("local")}</th><th>Status</th><th>Ações</th></tr>`;

      $("corpo").innerHTML = L.map(i => {
        const b = i.qtd <= i.minimo;
        const st = i.qtd === 0
          ? `<span class="tag zero">Sem estoque</span>`
          : b
            ? `<span class="tag baixo">Estoque baixo</span>`
            : `<span class="tag ok">Normal</span>`;
        return `<tr class="${b ? "baixo" : ""}"><td>${i.foto ? `<img class="thumb" src="${i.foto}">` : `<div class="sf">Sem foto</div>`}</td>
          <td>${esc(i.codigo)}</td><td><b>${esc(i.nome)}</b><br><small>${esc(i.marca)}</small></td><td>${esc(i.categoria)}</td>
          <td><b>${i.qtd}</b> <small>(mín. ${i.minimo})</small></td>${adm() ? `<td>${moeda(i.preco)}</td>` : ""}<td>${esc(i.local)}</td><td>${st}</td>
          <td class="acoes"><button onclick="modalMov(${i.id},1)">Entrada</button><button class="sec" onclick="modalMov(${i.id},-1)">Saída</button>
          ${adm() ? `<button class="sec" onclick="modalItem(${i.id})">Editar</button><button class="perigo" onclick="removerItem(${i.id})">Excluir</button>` : ""}</td></tr>`;
      }).join("") || `<tr><td colspan="9"><small>Nenhum produto encontrado.</small></td></tr>`;
    }

    function reduzirFoto(arq) {
      return new Promise(res => {
        const r = new FileReader();
        r.onload = () => {
          const img = new Image();
          img.onload = () => {
            const e = Math.min(1, 320 / Math.max(img.width, img.height)),
              c = document.createElement("canvas");
            c.width = Math.round(img.width * e);
            c.height = Math.round(img.height * e);
            c.getContext("2d").drawImage(img, 0, 0, c.width, c.height);
            res(c.toDataURL("image/jpeg", .75));
          };
          img.src = r.result;
        };
        r.readAsDataURL(arq);
      });
    }

    async function previa() {
      const f = $("fFoto").files[0];
      if (!f) return;
      if (!f.type.startsWith("image/")) { notificar("Selecione um arquivo de imagem válido.", "erro"); $("fFoto").value = ""; return; }
      if (f.size > 10 * 1024 * 1024) { notificar("A imagem deve ter no máximo 10 MB.", "erro"); $("fFoto").value = ""; return; }
      foto = await reduzirFoto(f);
      $("prev").src = foto;
      $("prev").hidden = false;
      if (API_BASE_URL) {
        try {
          const arquivo = new FormData();
          arquivo.append("foto", f);
          const retorno = await requisicaoAPI("/api/uploads", { method: "POST", body: arquivo });
          if (retorno.url) foto = retorno.url;
          notificar("Foto enviada ao servidor.", "sucesso");
        } catch (e) { notificar(`Prévia disponível, mas o envio falhou: ${e.message}`, "erro"); }
      } else {
        notificar("Prévia carregada. O envio ao servidor será ativado quando a API estiver configurada.", "info");
      }
    }

    function modalItem(id) {
      if (!adm()) return;
      const i = D.itens.find(x => x.id === id) || {};
      foto = i.foto || "";
      const cats = [...new Set([...CATEGORIAS, ...D.itens.map(x => x.categoria)])];

      modal(id ? "Editar produto" : "Novo produto", `<div class="form2">
        ${cp("fCod", "Código / SKU", "text", i.codigo)}${cp("fCat", "Categoria", "text", i.categoria, 'list="cats"')}
        <datalist id="cats">${cats.map(c => `<option value="${esc(c)}">`).join("")}</datalist>
        <div class="cheio">${cp("fNome", "Produto", "text", i.nome)}</div>
        ${cp("fMarca", "Marca", "text", i.marca)}${cp("fLocal", "Local de armazenagem", "text", i.local)}
        ${cp("fPreco", "Preço (R$)", "number", i.preco, 'min="0" step="0.01"')}${cp("fQtd", "Quantidade", "number", i.qtd, 'min="0"')}
        ${cp("fMin", "Estoque mínimo", "number", i.minimo, 'min="0"')}
        <div class="cheio"><label>Foto do produto</label><input id="fFoto" type="file" accept="image/*" onchange="previa()"><img id="prev" ${foto ? `src="${foto}"` : "hidden"}></div></div>`,
        () => {
          const codigo = $("fCod").value.trim(),
            nome = $("fNome").value.trim();

          if (!codigo || !nome) {
            alert("Informe código e nome do produto.");
            return false;
          }
          if (D.itens.some(x => x.codigo === codigo && x.id !== id)) {
            alert("Já existe um produto com esse código.");
            return false;
          }

          const d = {
            codigo,
            nome,
            marca: $("fMarca").value.trim(),
            categoria: $("fCat").value.trim() || "Outros",
            local: $("fLocal").value.trim(),
            preco: Math.max(0, +$("fPreco").value || 0),
            qtd: Math.max(0, parseInt($("fQtd").value) || 0),
            minimo: Math.max(0, parseInt($("fMin").value) || 0),
            foto
          };

          if (id) {
            const it = D.itens.find(x => x.id === id),
              antes = it.qtd;
            Object.assign(it, d);
            reg("Edição", `Produto editado: ${nome}` + (antes !== d.qtd ? ` (quantidade ${antes} para ${d.qtd})` : ""));
          } else {
            D.itens.push({ id: Date.now(), ...d });
            reg("Cadastro", `Produto cadastrado: ${nome} (${d.qtd} un.)`);
          }
          salvar("itens");
          mostrar("estoque");
        });
    }

    function removerItem(id) {
      const i = D.itens.find(x => x.id === id);
      if (!adm() || !i || !confirm(`Excluir "${i.nome}" do estoque?`)) return;
      reg("Exclusão", `Produto removido: ${i.nome}`);
      D.itens = D.itens.filter(x => x.id !== id);
      salvar("itens");
      mostrar("estoque");
    }

    /* Entrada e saída */
    function mover(id, sinal, n, extra = "") {
      const i = D.itens.find(x => x.id == id);
      if (!i) return alert("Selecione um produto."), false;
      if (!(n > 0)) return alert("Informe uma quantidade maior que zero."), false;
      if (sinal < 0 && n > i.qtd) return alert(`Estoque insuficiente. Disponível: ${i.qtd}.`), false;
      i.qtd += sinal * n;
      salvar("itens");
      reg(sinal > 0 ? "Entrada" : "Saída", `${sinal > 0 ? "+" : "-"}${n} un. de ${i.nome} (saldo ${i.qtd})${extra}`);
      return true;
    }

    function modalMov(id, sinal) {
      const i = D.itens.find(x => x.id === id);
      modal(
        `${sinal > 0 ? "Entrada" : "Saída"}: ${i.nome}`,
        `<label>Quantidade de unidades</label><input id="mQtd" type="number" min="1" style="width:100%">`,
        () => {
          if (!mover(id, sinal, parseInt($("mQtd").value))) return false;
          mostrar("estoque");
        },
        "Confirmar"
      );
    }

    function enviarSaida() {
      const cli = $("sCli") && $("sCli").value ? " - Cliente: " + $("sCli").value : "";
      if (mover($("sProd").value, -1, parseInt($("sQtd").value), ` - ${$("sMot").value}${cli}`)) {
        alert("Saída registrada.");
        mostrar("saida");
      }
    }

    /* Clientes e fornecedores (mesma tela para os dois) */
    function telaCad(tp) {
      const C = CAD[tp];
      $("conteudo").innerHTML = `<div class="topo"><h1>${C.tt}</h1><button onclick="modalCad('${tp}')">Novo ${C.t}</button></div>
        <div class="painel"><div class="rolagem"><table><thead><tr>${C.f.map(f => `<th>${f[1]}</th>`).join("")}<th></th></tr></thead><tbody>
        ${D[tp].map(r => `<tr>${C.f.map(f => `<td>${esc(r[f[0]])}</td>`).join("")}<td class="acoes"><button class="sec" onclick="modalCad('${tp}',${r.id})">Editar</button><button class="perigo" onclick="apagarCad('${tp}',${r.id})">Excluir</button></td></tr>`).join("") || `<tr><td colspan="5"><small>Nenhum registro cadastrado.</small></td></tr>`}
        </tbody></table></div></div>`;
    }

    function modalCad(tp, id) {
      const C = CAD[tp],
        r = D[tp].find(x => x.id === id) || {};

      modal(
        id ? `Editar ${C.t}` : `Novo ${C.t}`,
        `<div class="form2">${C.f.map(f => cp("c_" + f[0], f[1], "text", r[f[0]])).join("")}</div>`,
        () => {
          const d = {};
          C.f.forEach(f => d[f[0]] = $("c_" + f[0]).value.trim());

          if (!d.nome) {
            alert("Informe o nome.");
            return false;
          }

          if (id) Object.assign(r, d);
          else D[tp].push({ id: Date.now(), ...d });

          reg("Cadastro", `${C.t[0].toUpperCase() + C.t.slice(1)} ${id ? "editado" : "cadastrado"}: ${d.nome}`);
          salvar(tp);
          mostrar(tp);
        }
      );
    }

    function apagarCad(tp, id) {
      const r = D[tp].find(x => x.id === id);
      if (!r || !confirm(`Excluir "${r.nome}"?`)) return;
      reg("Exclusão", `${CAD[tp].t[0].toUpperCase() + CAD[tp].t.slice(1)} removido: ${r.nome}`);
      D[tp] = D[tp].filter(x => x.id !== id);
      salvar(tp);
      mostrar(tp);
    }

    /* Usuários */
    async function criarUsuario() {
      if (!adm()) return;
      const email = $("uEmail").value.trim(),
        nome = $("uNome").value.trim(),
        senha = $("uSenha").value;

      if (!email || !nome || senha.length < 4) return alert("Preencha tudo. A senha precisa de ao menos 4 caracteres.");
      if (D.users.some(u => u.email.toLowerCase() === email.toLowerCase())) return alert("Esse e-mail já está cadastrado.");

      D.users.push({ email, nome, perfil: $("uPerfil").value, hash: await hash(senha) });
      salvar("users");
      reg("Usuário", `Usuário criado: ${email}`);
      mostrar("usuarios");
    }

    function apagarUsuario(i) {
      if (!adm() || !confirm(`Excluir o usuário "${D.users[i].email}"?`)) return;
      reg("Usuário", `Usuário removido: ${D.users[i].email}`);
      D.users.splice(i, 1);
      salvar("users");
      mostrar("usuarios");
    }

    /* Exportar */
    function exportarCSV() {
      if (!adm()) return;
      const l = [
        "codigo;nome;marca;categoria;local;quantidade;minimo;preco",
        ...D.itens.map(i => [i.codigo, i.nome, i.marca, i.categoria, i.local, i.qtd, i.minimo, i.preco].join(";"))
      ];
      const a = document.createElement("a");
      a.href = URL.createObjectURL(new Blob(["\ufeff" + l.join("\n")], { type: "text/csv" }));
      a.download = "estoque.csv";
      a.click();
    }

    /* Início */
    migrar();
    (async () => {
      try { await carregarEstadoServidor(); } catch { /* a tela de login continua acessível */ }
      await semear();
      if (sessao) iniciar();
    })();
