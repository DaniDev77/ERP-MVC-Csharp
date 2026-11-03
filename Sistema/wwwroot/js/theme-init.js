// Roda no <head>, antes de pintar a página: aplica o tema (claro/escuro) para não piscar.
// Fica em arquivo próprio porque a CSP não aceita script inline.
(function () {
    "use strict";
    var raiz = document.documentElement;
    // Marca que há JavaScript: o CSS só esconde coisas "progressivas" (ex.: linhas livres do carrinho) com JS ativo
    raiz.classList.add("js");
    var tema = null;
    try { tema = window.localStorage.getItem("sistema-tema"); } catch (e) { tema = null; }
    // Alterado (redesign B, D5 e parecer R2): a escolha salva vale; sem escolha (ou valor inválido), segue o tema do aparelho
    if (tema !== "dark" && tema !== "light") {
        tema = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }
    raiz.setAttribute("data-theme", tema);
})();
