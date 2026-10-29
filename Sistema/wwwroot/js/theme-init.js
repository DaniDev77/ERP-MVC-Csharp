// Roda no <head>, antes de pintar a página: aplica o tema salvo (claro/escuro) para não piscar.
// Fica em arquivo próprio porque a CSP não aceita script inline.
(function () {
    "use strict";
    var raiz = document.documentElement;
    // Marca que há JavaScript: o CSS só esconde coisas "progressivas" (ex.: linhas livres do carrinho) com JS ativo
    raiz.classList.add("js");
    var tema = null;
    try { tema = window.localStorage.getItem("sistema-tema"); } catch (e) { tema = null; }
    // Padrão é o claro; o escuro é opcional, escolhido no botão da topbar
    raiz.setAttribute("data-theme", tema === "dark" ? "dark" : "light");
})();
