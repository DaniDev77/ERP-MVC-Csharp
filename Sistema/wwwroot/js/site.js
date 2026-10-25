// Comportamentos do Sistema. Tudo que a tela precisa rodar sem ida ao servidor fica aqui.
// Sem nada inline no HTML: a política de segurança (CSP) só aceita scripts deste arquivo.
(function () {
    "use strict";

    var moeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

    // Busca de listas: filtra as linhas da tabela pelo texto de data-filtro, sem recarregar a página
    document.querySelectorAll("[data-filtro-alvo]").forEach(function (campo) {
        var tabela = document.querySelector(campo.getAttribute("data-filtro-alvo"));
        if (!tabela) return;
        var vazio = tabela.parentElement.querySelector(".sem-resultado");

        campo.addEventListener("input", function () {
            var termo = campo.value.trim().toLowerCase();
            var visiveis = 0;
            tabela.querySelectorAll("tbody tr").forEach(function (linha) {
                var texto = (linha.getAttribute("data-filtro") || "").toLowerCase();
                var mostra = termo === "" || texto.indexOf(termo) !== -1;
                linha.hidden = !mostra;
                if (mostra) visiveis++;
            });
            if (vazio) vazio.hidden = visiveis !== 0;
        });
    });

    // Pedido (venda e compra): subtotal por linha, total e alerta de quantidade acima do estoque
    var pedido = document.getElementById("formPedido");
    if (pedido) {
        var total = document.getElementById("pedidoTotal");

        var atualizar = function () {
            var soma = 0;
            pedido.querySelectorAll("[data-linha]").forEach(function (linha) {
                var select = linha.querySelector("[data-produto]");
                var quantidadeCampo = linha.querySelector("[data-quantidade]");
                var subtotal = linha.querySelector("[data-subtotal]");
                var opcao = select.options[select.selectedIndex];
                var preco = opcao && opcao.dataset.preco ? parseFloat(opcao.dataset.preco) : 0;
                var estoque = opcao && opcao.dataset.estoque ? parseInt(opcao.dataset.estoque, 10) : 0;
                var quantidade = parseInt(quantidadeCampo.value, 10);
                if (isNaN(quantidade) || quantidade < 0) quantidade = 0;

                var acima = select.value !== "0" && quantidade > estoque;
                quantidadeCampo.setAttribute("aria-invalid", acima ? "true" : "false");
                quantidadeCampo.title = acima ? "Acima do estoque disponível (" + estoque + ")" : "";
                quantidadeCampo.classList.toggle("is-invalid", acima);

                if (select.value === "0" || quantidade === 0) {
                    subtotal.textContent = "—";
                    subtotal.classList.add("vazio-valor");
                } else {
                    var valor = preco * quantidade;
                    subtotal.textContent = moeda.format(valor);
                    subtotal.classList.remove("vazio-valor");
                    soma += valor;
                }
            });
            total.textContent = moeda.format(soma);
        };

        pedido.addEventListener("input", atualizar);
        pedido.addEventListener("change", atualizar);
        atualizar();
    }
})();
