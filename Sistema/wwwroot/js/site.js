// Comportamentos de interface do Sistema. Nada inline no HTML: a CSP só aceita scripts de arquivo.
// Organizado em módulos pequenos; cada um só age se encontrar seus elementos na página.
(function () {
    "use strict";

    var moeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
    var $ = function (sel, raiz) { return (raiz || document).querySelector(sel); };
    var $$ = function (sel, raiz) { return Array.prototype.slice.call((raiz || document).querySelectorAll(sel)); };

    // ---------------------------------------------------------------
    // Toasts. Alterado (direção D, RD5): sucesso e aviso somem em 6s (pausa com o mouse ou o foco do teclado);
    // o erro fica na tela até ser fechado no X, com role="alert" (inclusive o criado pelo mostrarToast)
    // ---------------------------------------------------------------
    var pilhaToasts = $("#toasts");

    function fecharToast(toast) {
        if (!toast || toast.classList.contains("saindo")) return;
        toast.classList.add("saindo");
        setTimeout(function () { toast.remove(); }, 200);
    }

    // Alterado (direção D, RD5): o tempo do JS (6s) é o mesmo da barra .toast-x::after do components.css
    function prepararToast(toast) {
        var fixo = toast.classList.contains("toast-erro");
        if (fixo) toast.setAttribute("role", "alert");
        var timer = null;
        var agendar = function (ms) { if (!fixo) { clearTimeout(timer); timer = setTimeout(function () { fecharToast(toast); }, ms); } };
        agendar(6000);
        ["mouseenter", "focusin"].forEach(function (ev) { toast.addEventListener(ev, function () { clearTimeout(timer); }); });
        ["mouseleave", "focusout"].forEach(function (ev) { toast.addEventListener(ev, function () { agendar(2500); }); });
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
    // Sidebar como drawer abaixo de 1100px: abre por qualquer [data-menu-abrir], fecha no fundo, no X ou com Esc
    // Alterado (direção D, RD3): vários abridores (busca e conta na barra do topo, aba "Menu" no rodapé).
    // Com a gaveta aberta, um segundo clique fecha em vez de criar outro fundo, e o foco volta para quem abriu
    // ---------------------------------------------------------------
    var botoesMenu = $$("[data-menu-abrir]");
    var sidebar = $("#menu");
    if (botoesMenu.length && sidebar) {
        var fundo = null, quemAbriu = null;
        var marcar = function (aberto) { botoesMenu.forEach(function (b) { b.setAttribute("aria-expanded", aberto ? "true" : "false"); }); };
        var fecharMenu = function (devolverFoco) {
            document.body.classList.remove("menu-aberto");
            marcar(false);
            if (fundo) { fundo.remove(); fundo = null; }
            if (devolverFoco && quemAbriu) quemAbriu.focus();
            quemAbriu = null;
        };
        botoesMenu.forEach(function (botao) {
            botao.addEventListener("click", function () {
                // Já aberta: fecha, em vez de criar um segundo .drawer-fundo (o primeiro ficaria órfão cobrindo a página)
                if (document.body.classList.contains("menu-aberto")) { fecharMenu(true); return; }
                quemAbriu = botao;
                document.body.classList.add("menu-aberto");
                marcar(true);
                fundo = document.createElement("div");
                fundo.className = "drawer-fundo";
                fundo.addEventListener("click", function () { fecharMenu(true); });
                document.body.appendChild(fundo);
                // data-menu-foco="busca": o botão de busca da barra do topo abre a gaveta já no campo de busca (RD4)
                var alvo = botao.getAttribute("data-menu-foco") === "busca" ? $("[data-busca-global]") : $(".sidebar-nav a", sidebar);
                if (alvo) alvo.focus();
            });
        });
        $$("[data-menu-fechar]").forEach(function (b) { b.addEventListener("click", function () { fecharMenu(true); }); });
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && document.body.classList.contains("menu-aberto")) fecharMenu(true);
        });
        // Se a janela crescer com o menu aberto, desfaz o estado de drawer
        // Alterado (redesign B): breakpoint de 992 para 1100px, o mesmo do layout.css
        window.addEventListener("resize", function () {
            if (window.innerWidth >= 1100 && document.body.classList.contains("menu-aberto")) fecharMenu(false);
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
    // Novo (redesign B, D5 e parecer R2): o clique no botão vira escolha (true mesmo com o localStorage bloqueado)
    var escolhaNestaPagina = false;
    function temEscolha() {
        if (escolhaNestaPagina) return true;
        try { var t = window.localStorage.getItem("sistema-tema"); return t === "dark" || t === "light"; } catch (e) { return false; }
    }
    $$("[data-tema-alternar]").forEach(function (b) {
        b.addEventListener("click", function () {
            var novo = temaAtual() === "dark" ? "light" : "dark";
            escolhaNestaPagina = true;
            document.documentElement.setAttribute("data-theme", novo);
            try { window.localStorage.setItem("sistema-tema", novo); } catch (e) { /* armazenamento bloqueado: vale só nesta página */ }
            atualizarBotoesTema();
            document.dispatchEvent(new CustomEvent("sistema:tema"));
        });
    });
    atualizarBotoesTema();
    // Novo (redesign B, D5 e parecer R2): sem escolha salva, acompanha a troca de tema do aparelho com a página aberta
    var temaAparelho = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)");
    if (temaAparelho) {
        var aoMudarAparelho = function (e) {
            if (temEscolha()) return;
            document.documentElement.setAttribute("data-theme", e.matches ? "dark" : "light");
            atualizarBotoesTema();
            // O gráfico se redesenha com as cores do tema novo
            document.dispatchEvent(new CustomEvent("sistema:tema"));
        };
        if (temaAparelho.addEventListener) temaAparelho.addEventListener("change", aoMudarAparelho);
        else if (temaAparelho.addListener) temaAparelho.addListener(aoMudarAparelho);
    }

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
        // Alterado (direção D): abaixo de 1100px a busca fica na gaveta fechada (fora do Tab); o atalho abre a gaveta pelo
        // botão de busca da barra do topo, que já leva o foco ao campo (RD3 e RD4)
        document.addEventListener("keydown", function (e) {
            var alvo = e.target.tagName;
            // Com a folha da venda aberta (diálogo do PDV), o atalho não abre a gaveta por baixo dela
            if (e.key === "/" && alvo !== "INPUT" && alvo !== "TEXTAREA" && alvo !== "SELECT" && !document.body.classList.contains("folha-venda-aberta")) {
                e.preventDefault();
                var abridorBusca = $("[data-menu-abrir][data-menu-foco='busca']");
                if (window.innerWidth < 1100 && abridorBusca && !document.body.classList.contains("menu-aberto")) abridorBusca.click();
                else buscaGlobal.focus();
            }
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
    // Novo (direção D, RD8): motivos rápidos da recusa (Pedido/Index) e contador "N de 300".
    // Sem JS o grupo continua escondido (hidden) e a dica fica estática. O campo é achado pelo próprio form (não por id),
    // o texto vem do data-motivo-texto (sem innerHTML), é cortado no maxLength (o maxlength não limita valor posto por
    // script) e o evento input atualiza o contador. O servidor continua aceitando qualquer texto de até 300 (HF-006)
    // ---------------------------------------------------------------
    $$("[data-motivos]").forEach(function (grupo) {
        var form = grupo.closest("form");
        var campo = form ? form.elements.namedItem("motivo") : null;
        if (!campo) return;
        var contador = $("[data-motivo-contador]", form);
        var limite = campo.maxLength > 0 ? campo.maxLength : 300;
        var contar = function () { if (contador) contador.textContent = campo.value.length + " de " + limite + " caracteres"; };
        grupo.hidden = false;
        $$("[data-motivo-texto]", grupo).forEach(function (b) {
            b.addEventListener("click", function () {
                campo.value = (b.getAttribute("data-motivo-texto") || "").slice(0, limite);
                campo.dispatchEvent(new Event("input", { bubbles: true }));
                campo.focus();
            });
        });
        campo.addEventListener("input", contar);
        contar();
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
        // Corrigido (revisão D1/D2, achado 2): Enter na busca de produtos (ou "Ir" no teclado do celular) enviava o
        // #formPedido e registrava a venda ou o pedido sem revisão. Na busca, Enter só filtra
        var campoBuscaPdv = $("[data-pdv-busca]", pedido);
        if (campoBuscaPdv) {
            campoBuscaPdv.addEventListener("keydown", function (e) {
                if (e.key === "Enter") e.preventDefault();
            });
        }

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
            // Novo (direção D): com o total zerado, o número fica cinza e sem a etiqueta amarela (nada em aberto ainda)
            total.classList.toggle("zerado", soma === 0);
            var textoItens = unidades + (unidades === 1 ? " item" : " itens");
            if (qtdItens) qtdItens.textContent = textoItens;
            // Novo (direção D, RD11): "N de 8 produtos" sai das linhas do formulário, nunca de um 8 escrito à mão
            var limite = $("[data-pdv-limite]", pedido);
            if (limite) limite.textContent = usadas + " de " + linhas.length + " produtos";
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

        // -----------------------------------------------------------
        // Novo (direção D, RD11): no celular (abaixo de 880px) a barra fixa abre a venda numa folha que sobe do rodapé.
        // A folha é o MESMO #carrinho, dentro do #formPedido, só mudado por classe: nada é clonado, então os 8
        // Linhas[i].ProdutoId/Quantidade e o Comprador continuam um só no form. Sem JS, o carrinho fica no fluxo.
        // role="dialog" e aria-modal só enquanto a folha está aberta, com o foco preso dentro, Esc e o foco de volta na barra
        // -----------------------------------------------------------
        var carrinho = $("#carrinho", pedido);
        var barraFolha = $("[data-pdv-barra]", pedido);
        var fecharFolhaBtn = $("[data-pdv-folha-fechar]", pedido);
        var estreito = window.matchMedia("(max-width: 879.98px)");
        if (carrinho && barraFolha) {
            // Erro do servidor dentro do carrinho (ex.: cliente não encontrado): o carrinho fica no fluxo, à vista, sem folha
            if ($(".field-validation-error, .input-validation-error", carrinho)) pedido.classList.add("pdv-com-erro");
            var fundoFolha = null;
            var focaveis = function () {
                return $$("a[href], button, input, select, textarea, [tabindex]", carrinho).filter(function (el) {
                    return el.tabIndex >= 0 && !el.disabled && el.type !== "hidden" && el.getClientRects().length > 0;
                });
            };
            var fecharFolha = function (devolverFoco) {
                if (!carrinho.classList.contains("folha-aberta")) return;
                carrinho.classList.remove("folha-aberta");
                carrinho.removeAttribute("role");
                carrinho.removeAttribute("aria-modal");
                document.body.classList.remove("folha-venda-aberta");
                barraFolha.setAttribute("aria-expanded", "false");
                if (fecharFolhaBtn) fecharFolhaBtn.hidden = true;
                if (fundoFolha) { fundoFolha.remove(); fundoFolha = null; }
                if (devolverFoco) {
                    // Se a venda ficou vazia, a barra some: o foco vai para a busca de produtos
                    var volta = barraFolha.hidden ? $("[data-pdv-busca]", pedido) : barraFolha;
                    if (volta) volta.focus();
                }
            };
            var abrirFolha = function () {
                carrinho.classList.add("folha-aberta");
                carrinho.setAttribute("role", "dialog");
                carrinho.setAttribute("aria-modal", "true");
                document.body.classList.add("folha-venda-aberta");
                barraFolha.setAttribute("aria-expanded", "true");
                if (fecharFolhaBtn) fecharFolhaBtn.hidden = false;
                fundoFolha = document.createElement("div");
                fundoFolha.className = "folha-fundo";
                fundoFolha.addEventListener("click", function () { fecharFolha(true); });
                document.body.appendChild(fundoFolha);
                (fecharFolhaBtn || carrinho).focus();
            };
            barraFolha.addEventListener("click", function () {
                if (estreito.matches && !pedido.classList.contains("pdv-com-erro")) abrirFolha();
                else carrinho.focus();
            });
            if (fecharFolhaBtn) fecharFolhaBtn.addEventListener("click", function () { fecharFolha(true); });
            carrinho.addEventListener("keydown", function (e) {
                if (!carrinho.classList.contains("folha-aberta")) return;
                if (e.key === "Escape") { e.preventDefault(); fecharFolha(true); return; }
                if (e.key !== "Tab") return;
                // Foco preso na folha: do último volta ao primeiro, e vice-versa
                var lista = focaveis();
                if (!lista.length) return;
                var primeiro = lista[0], ultimo = lista[lista.length - 1];
                if (e.shiftKey && (document.activeElement === primeiro || document.activeElement === carrinho)) { e.preventDefault(); ultimo.focus(); }
                else if (!e.shiftKey && document.activeElement === ultimo) { e.preventDefault(); primeiro.focus(); }
            });
            // Janela larga de novo (girar o tablet): a folha vira a coluna da direita
            var aoMudarLargura = function () { if (!estreito.matches) fecharFolha(false); };
            if (estreito.addEventListener) estreito.addEventListener("change", aoMudarLargura); else estreito.addListener(aoMudarLargura);
        }
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
            // Alterado (redesign B): barras do mês no azul-caneta (a cor dos dados) e "hoje" em tinta; cores lidas dos tokens do tema
            var corBarra = cor("--chart-bar") || "#1D5FBF";
            var corHoje = cor("--chart-bar-hoje") || "#131416";
            var hoje = valores.length - 1;

            // Alterado (redesign B, parecer R3): a mesma família do @font-face do base.css ("Geist"), com a do sistema como reserva
            window.Chart.defaults.font.family = "Geist, system-ui, sans-serif";
            window.Chart.defaults.color = cor("--chart-text");

            grafico = new window.Chart(canvas, {
                type: "bar",
                data: {
                    labels: rotulos,
                    datasets: [{
                        label: "Vendas",
                        data: valores,
                        backgroundColor: valores.map(function (_, i) { return i === hoje ? corHoje : corBarra; }),
                        hoverBackgroundColor: cor("--chart-bar-hover") || "#164A94",
                        // Redesign: colunas de canto reto na base e só 2px de raio no topo (a base fica apoiada no zero)
                        borderRadius: 2,
                        borderSkipped: "start",
                        maxBarThickness: 22
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    // Alterado (redesign B, parecer 7.1-4): sem animação para quem pediu menos movimento no sistema
                    animation: window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches ? false : { duration: 500 },
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

    // ---------------------------------------------------------------
    // Novo (HF-002): localização do cliente. Ao trocar a UF, busca as cidades em GET Perfil/Cidades?uf=XX
    // (mesmo site, permitido pela CSP) e preenche o select de cidade. Sem JS, o formulário funciona em dois envios.
    // ---------------------------------------------------------------
    $$("[data-loc-uf]").forEach(function (selUf) {
        var form = selUf.form;
        var selCidade = form && $("[data-loc-cidade]", form);
        var url = selUf.getAttribute("data-cidades-url");
        if (!selCidade || !url) return;

        function opcao(valor, texto) {
            var o = document.createElement("option");
            o.value = valor;
            o.textContent = texto;
            return o;
        }

        selUf.addEventListener("change", function () {
            var uf = selUf.value;
            selCidade.replaceChildren(opcao("", uf ? "Carregando cidades..." : "Escolha a UF primeiro"));
            selCidade.disabled = !!uf;
            if (!uf) { selCidade.disabled = false; return; }

            fetch(url + "?uf=" + encodeURIComponent(uf), { headers: { "Accept": "application/json" }, credentials: "same-origin" })
                .then(function (r) { if (!r.ok) throw new Error(r.status); return r.json(); })
                .then(function (cidades) {
                    // Resposta atrasada de uma UF anterior não sobrescreve a escolha atual
                    if (selUf.value !== uf) return;
                    selCidade.replaceChildren(opcao("", "Escolha"));
                    cidades.forEach(function (c) { selCidade.appendChild(opcao(c.codigo, c.nome)); });
                    selCidade.required = true;
                })
                .catch(function () {
                    if (selUf.value !== uf) return;
                    selCidade.replaceChildren(opcao("", "Não foi possível carregar. Clique em Salvar para tentar de novo."));
                    // Corrigido (QA B4): sem a lista, a cidade deixa de ser obrigatória no navegador, para o Salvar enviar
                    // só a UF e o servidor devolver a página já com as cidades (o mesmo caminho de quem está sem JS)
                    selCidade.required = false;
                })
                .finally(function () {
                    // Corrigido (QA B4): só reabilita se ainda for a UF escolhida (resposta atrasada não destrava outra carga)
                    if (selUf.value === uf) selCidade.disabled = false;
                });
        });

        // Com JS, a cidade passa a ser obrigatória no navegador (o servidor confere de qualquer forma)
        selCidade.required = true;
    });
})();
