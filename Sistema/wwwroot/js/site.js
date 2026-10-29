// Comportamentos de interface do Sistema. Nada inline no HTML: a CSP só aceita scripts de arquivo.
// Organizado em módulos pequenos; cada um só age se encontrar seus elementos na página.
(function () {
    "use strict";

    var moeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
    var $ = function (sel, raiz) { return (raiz || document).querySelector(sel); };
    var $$ = function (sel, raiz) { return Array.prototype.slice.call((raiz || document).querySelectorAll(sel)); };

    // ---------------------------------------------------------------
    // Toasts: fecham sozinhos após 5s (pausa com o mouse em cima)
    // ---------------------------------------------------------------
    var pilhaToasts = $("#toasts");

    function fecharToast(toast) {
        if (!toast || toast.classList.contains("saindo")) return;
        toast.classList.add("saindo");
        setTimeout(function () { toast.remove(); }, 200);
    }

    function prepararToast(toast) {
        var timer = setTimeout(function () { fecharToast(toast); }, 5000);
        toast.addEventListener("mouseenter", function () { clearTimeout(timer); });
        toast.addEventListener("mouseleave", function () { timer = setTimeout(function () { fecharToast(toast); }, 2500); });
        var botao = $("[data-toast-fechar]", toast);
        if (botao) botao.addEventListener("click", function () { fecharToast(toast); });
    }

    $$("[data-toast]", pilhaToasts || document).forEach(prepararToast);

    // Cria um toast na hora (tipo: "sucesso", "erro" ou "aviso")
    var icones = {
        sucesso: { classe: "bubble-success", svg: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="m8 12.5 2.5 2.5L16 9.5"/></svg>' },
        erro: { classe: "bubble-danger", svg: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"><circle cx="12" cy="12" r="9"/><path d="M12 7.5v5.5M12 16.5v.01"/></svg>' },
        aviso: { classe: "bubble-warning", svg: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"><circle cx="12" cy="12" r="9"/><path d="M12 7.5v5.5M12 16.5v.01"/></svg>' }
    };
    function mostrarToast(tipo, titulo, texto) {
        var modelo = $("#toast-modelo");
        if (!modelo || !pilhaToasts) return;
        var toast = modelo.content.firstElementChild.cloneNode(true);
        var info = icones[tipo] || icones.aviso;
        toast.classList.add("toast-" + tipo);
        var bolha = $(".bubble", toast);
        bolha.classList.add(info.classe);
        bolha.innerHTML = info.svg;
        $(".toast-x-titulo", toast).textContent = titulo;
        $(".toast-x-texto", toast).textContent = texto;
        pilhaToasts.appendChild(toast);
        prepararToast(toast);
    }
    window.SistemaToast = mostrarToast;

    // ---------------------------------------------------------------
    // Topbar: linha embaixo só depois de rolar a página
    // ---------------------------------------------------------------
    var topbar = $(".topbar");
    if (topbar) {
        var marcarRolagem = function () { topbar.classList.toggle("rolou", window.scrollY > 4); };
        window.addEventListener("scroll", marcarRolagem, { passive: true });
        marcarRolagem();
    }

    // ---------------------------------------------------------------
    // Sidebar como drawer no celular: abre no hambúrguer, fecha no fundo, no X ou com Esc
    // ---------------------------------------------------------------
    var botaoMenu = $("[data-menu-abrir]");
    var sidebar = $("#menu");
    if (botaoMenu && sidebar) {
        var fundo = null;
        var fecharMenu = function (devolverFoco) {
            document.body.classList.remove("menu-aberto");
            botaoMenu.setAttribute("aria-expanded", "false");
            if (fundo) { fundo.remove(); fundo = null; }
            if (devolverFoco) botaoMenu.focus();
        };
        botaoMenu.addEventListener("click", function () {
            document.body.classList.add("menu-aberto");
            botaoMenu.setAttribute("aria-expanded", "true");
            fundo = document.createElement("div");
            fundo.className = "drawer-fundo";
            fundo.addEventListener("click", function () { fecharMenu(true); });
            document.body.appendChild(fundo);
            var primeiro = $(".sidebar-nav a", sidebar);
            if (primeiro) primeiro.focus();
        });
        $$("[data-menu-fechar]").forEach(function (b) { b.addEventListener("click", function () { fecharMenu(true); }); });
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && document.body.classList.contains("menu-aberto")) fecharMenu(true);
        });
        // Se a janela crescer com o menu aberto, desfaz o estado de drawer
        window.addEventListener("resize", function () {
            if (window.innerWidth >= 992 && document.body.classList.contains("menu-aberto")) fecharMenu(false);
        });
    }

    // ---------------------------------------------------------------
    // Tema claro/escuro: botão da topbar, salvo no localStorage (com try/catch)
    // ---------------------------------------------------------------
    function temaAtual() { return document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light"; }
    function atualizarBotoesTema() {
        var escuro = temaAtual() === "dark";
        $$("[data-tema-alternar]").forEach(function (b) {
            b.setAttribute("aria-pressed", escuro ? "true" : "false");
            b.setAttribute("aria-label", escuro ? "Usar tema claro" : "Usar tema escuro");
            b.title = escuro ? "Tema claro" : "Tema escuro";
        });
    }
    $$("[data-tema-alternar]").forEach(function (b) {
        b.addEventListener("click", function () {
            var novo = temaAtual() === "dark" ? "light" : "dark";
            document.documentElement.setAttribute("data-theme", novo);
            try { window.localStorage.setItem("sistema-tema", novo); } catch (e) { /* armazenamento bloqueado: vale só nesta página */ }
            atualizarBotoesTema();
            document.dispatchEvent(new CustomEvent("sistema:tema"));
        });
    });
    atualizarBotoesTema();

    // ---------------------------------------------------------------
    // Menus suspensos (usuário na topbar e "três pontos" nas tabelas)
    // ---------------------------------------------------------------
    var menuAberto = null;
    function fecharMenuAcoes(devolverFoco) {
        if (!menuAberto) return;
        menuAberto.lista.hidden = true;
        menuAberto.botao.setAttribute("aria-expanded", "false");
        if (devolverFoco) menuAberto.botao.focus();
        menuAberto = null;
    }
    $$("[data-menu-acoes]").forEach(function (botao) {
        var lista = document.getElementById(botao.getAttribute("aria-controls"));
        if (!lista) return;
        botao.addEventListener("click", function (e) {
            e.stopPropagation();
            var jaAberto = menuAberto && menuAberto.lista === lista;
            fecharMenuAcoes(false);
            if (jaAberto) return;
            lista.hidden = false;
            botao.setAttribute("aria-expanded", "true");
            menuAberto = { botao: botao, lista: lista };
            var primeiro = $("a, button", lista);
            if (primeiro && e.detail === 0) primeiro.focus(); // aberto pelo teclado: foco no primeiro item
        });
        // Setas para cima/baixo percorrem os itens do menu
        lista.addEventListener("keydown", function (e) {
            if (e.key !== "ArrowDown" && e.key !== "ArrowUp") return;
            e.preventDefault();
            var itens = $$("a, button", lista);
            var i = itens.indexOf(document.activeElement);
            var prox = e.key === "ArrowDown" ? (i + 1) % itens.length : (i - 1 + itens.length) % itens.length;
            itens[prox].focus();
        });
    });
    document.addEventListener("click", function (e) {
        if (menuAberto && !menuAberto.lista.contains(e.target)) fecharMenuAcoes(false);
    });
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") fecharMenuAcoes(true);
    });

    // ---------------------------------------------------------------
    // Busca da topbar: "ir para" uma tela do menu (filtra os links da sidebar)
    // ---------------------------------------------------------------
    var buscaGlobal = $("[data-busca-global]");
    if (buscaGlobal && sidebar) {
        var listaBusca = document.getElementById(buscaGlobal.getAttribute("aria-controls"));
        var destinos = $$(".sidebar-nav a", sidebar).map(function (a) {
            return { texto: a.textContent.trim().replace(/\s+\d+$/, ""), href: a.getAttribute("href"), icone: ($("svg", a) || {}).outerHTML || "" };
        });
        var renderBusca = function () {
            var termo = buscaGlobal.value.trim().toLowerCase();
            listaBusca.innerHTML = "";
            if (!termo) { listaBusca.hidden = true; buscaGlobal.setAttribute("aria-expanded", "false"); return; }
            var achados = destinos.filter(function (d) { return d.texto.toLowerCase().indexOf(termo) !== -1; });
            if (!achados.length) {
                var vazio = document.createElement("div");
                vazio.className = "menu-acoes-cabeca";
                vazio.innerHTML = "<span></span>";
                vazio.firstChild.textContent = "Nenhuma tela encontrada";
                listaBusca.appendChild(vazio);
            }
            achados.forEach(function (d) {
                var a = document.createElement("a");
                a.href = d.href;
                a.setAttribute("role", "option");
                a.innerHTML = d.icone;
                a.appendChild(document.createTextNode(d.texto));
                listaBusca.appendChild(a);
            });
            listaBusca.hidden = false;
            buscaGlobal.setAttribute("aria-expanded", "true");
        };
        buscaGlobal.addEventListener("input", renderBusca);
        buscaGlobal.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                var primeiro = $("a", listaBusca);
                if (primeiro) { e.preventDefault(); window.location.href = primeiro.href; }
            } else if (e.key === "ArrowDown") {
                var item = $("a", listaBusca);
                if (item) { e.preventDefault(); item.focus(); }
            } else if (e.key === "Escape") {
                buscaGlobal.value = ""; renderBusca();
            }
        });
        listaBusca.addEventListener("keydown", function (e) {
            if (e.key !== "ArrowDown" && e.key !== "ArrowUp") return;
            e.preventDefault();
            var itens = $$("a", listaBusca);
            var i = itens.indexOf(document.activeElement);
            if (e.key === "ArrowUp" && i <= 0) { buscaGlobal.focus(); return; }
            itens[Math.min(itens.length - 1, i + (e.key === "ArrowDown" ? 1 : -1))].focus();
        });
        document.addEventListener("click", function (e) {
            if (!buscaGlobal.parentElement.contains(e.target)) { listaBusca.hidden = true; buscaGlobal.setAttribute("aria-expanded", "false"); }
        });
        // Atalho "/" foca a busca (fora de campos de texto)
        document.addEventListener("keydown", function (e) {
            var alvo = e.target.tagName;
            if (e.key === "/" && alvo !== "INPUT" && alvo !== "TEXTAREA" && alvo !== "SELECT") { e.preventDefault(); buscaGlobal.focus(); }
        });
    }

    // ---------------------------------------------------------------
    // Busca nas listas: filtra itens com data-filtro, sem recarregar
    // ---------------------------------------------------------------
    $$("[data-filtro-alvo]").forEach(function (campo) {
        var alvo = $(campo.getAttribute("data-filtro-alvo"));
        if (!alvo) return;
        var vazio = $(campo.getAttribute("data-filtro-vazio") || "#__nada__");
        var contagem = $(campo.getAttribute("data-filtro-contagem") || "#__nada__");
        var singular = contagem ? contagem.getAttribute("data-singular") : "";
        var plural = contagem ? contagem.getAttribute("data-plural") : "";

        campo.addEventListener("input", function () {
            var termo = campo.value.trim().toLowerCase();
            var visiveis = 0;
            $$("[data-filtro]", alvo).forEach(function (item) {
                var texto = (item.getAttribute("data-filtro") || "").toLowerCase();
                var mostra = termo === "" || texto.indexOf(termo) !== -1;
                item.hidden = !mostra;
                if (mostra) visiveis++;
            });
            if (vazio) vazio.hidden = visiveis !== 0;
            if (contagem) contagem.textContent = visiveis + " " + (visiveis === 1 ? singular : plural);
        });
    });

    // ---------------------------------------------------------------
    // Mostrar/ocultar senha
    // ---------------------------------------------------------------
    $$("[data-senha-alternar]").forEach(function (botao) {
        var campo = document.getElementById(botao.getAttribute("aria-controls"));
        if (!campo) return;
        botao.addEventListener("click", function () {
            var mostrar = campo.type === "password";
            campo.type = mostrar ? "text" : "password";
            botao.setAttribute("aria-pressed", mostrar ? "true" : "false");
            botao.setAttribute("aria-label", mostrar ? "Ocultar senha" : "Mostrar senha");
            $$("[data-icone-senha]", botao).forEach(function (i) { i.hidden = i.getAttribute("data-icone-senha") !== (mostrar ? "ocultar" : "mostrar"); });
        });
    });

    // ---------------------------------------------------------------
    // Spinner no botão de envio. Espera a validação do jQuery decidir:
    // se ela cancelou o envio (defaultPrevented), o botão não fica carregando
    // ---------------------------------------------------------------
    document.addEventListener("submit", function (e) {
        var form = e.target;
        if (form.hasAttribute("data-sem-spinner")) return;
        setTimeout(function () {
            if (e.defaultPrevented) return;
            var botao = e.submitter || $("button[type=submit], button:not([type])", form);
            if (!botao) return;
            // Mantém a largura para o botão não "encolher" ao esconder o texto
            botao.style.minWidth = botao.offsetWidth + "px";
            botao.classList.add("is-loading");
            botao.setAttribute("aria-busy", "true");
        }, 0);
    });
    // Ao voltar pelo histórico (bfcache), tira o spinner que ficou
    window.addEventListener("pageshow", function () {
        $$(".btn.is-loading").forEach(function (b) { b.classList.remove("is-loading"); b.removeAttribute("aria-busy"); });
    });

    // ---------------------------------------------------------------
    // PDV (venda e compra): cards de produto alimentam as 8 linhas fixas do formulário.
    // Os campos Linhas[i].ProdutoId / Linhas[i].Quantidade continuam os mesmos;
    // o servidor recalcula tudo ao salvar.
    // ---------------------------------------------------------------
    var pedido = $("#formPedido");
    if (pedido) {
        var linhas = $$("[data-linha]", pedido);
        var cards = $$("[data-pdv-produto]", pedido);
        var total = $("#pedidoTotal");
        var qtdItens = $("[data-carrinho-qtd]", pedido);
        var vazioCarrinho = $("[data-carrinho-vazio]", pedido);
        var botaoEnviar = $("[data-pedido-enviar]", pedido);
        var cardsPorId = {};
        cards.forEach(function (c) { cardsPorId[c.getAttribute("data-id")] = c; });
        // Com JS, os selects ficam escondidos (o produto vem do card): tira do Tab para não focar algo invisível
        linhas.forEach(function (l) { $("[data-produto]", l).setAttribute("tabindex", "-1"); });

        var lerQtd = function (campo) {
            var q = parseInt(campo.value, 10);
            return isNaN(q) || q < 0 ? 0 : q;
        };

        var atualizar = function () {
            var soma = 0, unidades = 0, usadas = 0;
            var noCarrinho = {};

            linhas.forEach(function (linha) {
                var select = $("[data-produto]", linha);
                var campoQtd = $("[data-quantidade]", linha);
                var subtotal = $("[data-subtotal]", linha);
                var opcao = select.options[select.selectedIndex];
                var livre = select.value === "0";
                var preco = opcao && opcao.dataset.preco ? parseFloat(opcao.dataset.preco) : 0;
                var estoque = opcao && opcao.dataset.estoque ? parseInt(opcao.dataset.estoque, 10) : 0;
                var qtd = lerQtd(campoQtd);

                linha.classList.toggle("livre", livre);
                if (livre) { subtotal.textContent = "—"; campoQtd.classList.remove("is-invalid"); return; }

                usadas++;
                // Nome, preço e miniatura da linha vêm do card do produto
                var card = cardsPorId[select.value];
                var nome = $("[data-linha-nome]", linha);
                var precoEl = $("[data-linha-preco]", linha);
                var thumb = $("[data-linha-thumb]", linha);
                if (nome) nome.textContent = card ? card.getAttribute("data-nome") : opcao.textContent.trim();
                if (precoEl) precoEl.textContent = moeda.format(preco) + " · " + estoque + " em estoque";
                if (thumb && card) {
                    thumb.style.setProperty("--h", card.getAttribute("data-matiz"));
                    $("span", thumb).textContent = card.getAttribute("data-iniciais");
                }

                var acima = qtd > estoque;
                campoQtd.classList.toggle("is-invalid", acima);
                campoQtd.setAttribute("aria-invalid", acima ? "true" : "false");
                campoQtd.title = acima ? "Acima do estoque disponível (" + estoque + ")" : "";
                $("[data-qtd-mais]", linha).disabled = qtd >= estoque;

                var valor = preco * qtd;
                subtotal.textContent = qtd > 0 ? moeda.format(valor) : "—";
                soma += valor;
                unidades += qtd;
                noCarrinho[select.value] = (noCarrinho[select.value] || 0) + qtd;
            });

            total.textContent = moeda.format(soma);
            var textoItens = unidades + (unidades === 1 ? " item" : " itens");
            if (qtdItens) qtdItens.textContent = textoItens;
            // Barra flutuante do celular: mesmo total, só aparece com itens no carrinho
            var barra = $("[data-pdv-barra]", pedido);
            if (barra) {
                barra.hidden = unidades === 0;
                $("[data-pdv-barra-total]", barra).textContent = moeda.format(soma);
                $("[data-pdv-barra-qtd]", barra).textContent = textoItens;
            }
            if (vazioCarrinho) vazioCarrinho.hidden = usadas > 0;
            if (botaoEnviar) botaoEnviar.disabled = unidades === 0;

            // Card do produto: selo "no carrinho" e botão desligado quando atinge o estoque
            cards.forEach(function (c) {
                var id = c.getAttribute("data-id");
                var q = noCarrinho[id] || 0;
                var selo = $("[data-pdv-no-carrinho]", c);
                c.classList.toggle("no-carrinho", q > 0);
                if (selo) { selo.hidden = q === 0; selo.textContent = q + " no carrinho"; }
                var add = $("[data-pdv-add]", c);
                if (add) add.disabled = q >= parseInt(c.getAttribute("data-estoque"), 10);
            });
        };

        var linhaDoProduto = function (id) {
            return linhas.filter(function (l) { return $("[data-produto]", l).value === id; })[0];
        };

        // Adicionar pelo card: soma 1 se já está no carrinho; senão ocupa a próxima linha livre
        cards.forEach(function (card) {
            var add = $("[data-pdv-add]", card);
            if (!add) return;
            add.addEventListener("click", function () {
                var id = card.getAttribute("data-id");
                var estoque = parseInt(card.getAttribute("data-estoque"), 10);
                var linha = linhaDoProduto(id);
                if (linha) {
                    var campo = $("[data-quantidade]", linha);
                    campo.value = Math.min(estoque, lerQtd(campo) + 1);
                } else {
                    linha = linhas.filter(function (l) { return $("[data-produto]", l).value === "0"; })[0];
                    if (!linha) {
                        mostrarToast("aviso", "Carrinho cheio", "Cada pedido aceita até " + linhas.length + " produtos diferentes.");
                        return;
                    }
                    $("[data-produto]", linha).value = id;
                    $("[data-quantidade]", linha).value = 1;
                }
                atualizar();
                // Pequeno "pulo" no card para confirmar a ação
                card.classList.remove("adicionado");
                void card.offsetWidth;
                card.classList.add("adicionado");
            });
        });

        // Botões +, − e remover em cada linha do carrinho
        linhas.forEach(function (linha) {
            var select = $("[data-produto]", linha);
            var campo = $("[data-quantidade]", linha);
            var limpar = function () { select.value = "0"; campo.value = ""; atualizar(); };
            $("[data-qtd-mais]", linha).addEventListener("click", function () { campo.value = lerQtd(campo) + 1; atualizar(); });
            $("[data-qtd-menos]", linha).addEventListener("click", function () {
                var q = lerQtd(campo) - 1;
                if (q <= 0) limpar(); else { campo.value = q; atualizar(); }
            });
            $("[data-linha-remover]", linha).addEventListener("click", function () {
                limpar();
                var proximo = $("[data-pdv-busca]", pedido);
                if (proximo) proximo.focus();
            });
        });

        pedido.addEventListener("input", atualizar);
        pedido.addEventListener("change", atualizar);
        atualizar();
    }

    // ---------------------------------------------------------------
    // Gráfico de vendas por dia (Chart.js em wwwroot/lib/chartjs)
    // ---------------------------------------------------------------
    var canvas = $("#graficoVendas");
    if (canvas && window.Chart) {
        var valores = [];
        try { valores = JSON.parse(canvas.getAttribute("data-valores") || "[]"); } catch (e) { valores = []; }
        var mes = canvas.getAttribute("data-mes") || "";
        var rotulos = valores.map(function (_, i) { return String(i + 1); });
        var grafico = null;

        var cor = function (nome) { return getComputedStyle(document.documentElement).getPropertyValue(nome).trim(); };

        var desenhar = function () {
            if (grafico) grafico.destroy();
            // Redesign: barras do mês no verde-petróleo (a cor dos dados) e "hoje" em tinta; cores lidas dos tokens do tema
            var corBarra = cor("--chart-bar") || "#1B6C80";
            var corHoje = cor("--chart-bar-hoje") || "#131416";
            var hoje = valores.length - 1;

            // Redesign: mesma fonte da interface (IBM Plex Sans), com a Inter e a do sistema como reserva
            window.Chart.defaults.font.family = "'IBM Plex Sans', Inter, system-ui, sans-serif";
            window.Chart.defaults.color = cor("--chart-text");

            grafico = new window.Chart(canvas, {
                type: "bar",
                data: {
                    labels: rotulos,
                    datasets: [{
                        label: "Vendas",
                        data: valores,
                        backgroundColor: valores.map(function (_, i) { return i === hoje ? corHoje : corBarra; }),
                        hoverBackgroundColor: cor("--chart-bar-hover") || "#12505F",
                        // Redesign: colunas de canto reto na base e só 2px de raio no topo (a base fica apoiada no zero)
                        borderRadius: 2,
                        borderSkipped: "start",
                        maxBarThickness: 22
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    animation: { duration: 500 },
                    interaction: { mode: "index", intersect: false },
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            // Tooltip lê a cor do token do tema
                            // Redesign: tooltip simples, raio 4, fundo lido do token do tema
                            backgroundColor: cor("--chart-tooltip") || "#131416",
                            padding: 8,
                            cornerRadius: 4,
                            displayColors: false,
                            titleFont: { weight: "600" },
                            callbacks: {
                                title: function (itens) { return "Dia " + itens[0].label + (mes ? "/" + mes : ""); },
                                label: function (item) { return moeda.format(item.parsed.y); }
                            }
                        }
                    },
                    scales: {
                        x: { grid: { display: false }, border: { display: false }, ticks: { maxRotation: 0, autoSkipPadding: 12 } },
                        y: {
                            beginAtZero: true,
                            grid: { color: cor("--chart-grid") },
                            border: { display: false },
                            ticks: {
                                maxTicksLimit: 5,
                                callback: function (v) { return v >= 1000 ? "R$ " + (v / 1000).toLocaleString("pt-BR") + " mil" : "R$ " + v.toLocaleString("pt-BR"); }
                            }
                        }
                    }
                }
            });
        };

        desenhar();
        // Redesenha com as cores do novo tema
        document.addEventListener("sistema:tema", desenhar);
        canvas.parentElement.classList.add("pronto");
    }
})();
