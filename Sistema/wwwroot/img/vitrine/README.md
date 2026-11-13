# Fotos da home pública (`wwwroot/img/vitrine`)

Procedência das imagens usadas em `Views/Home/_Vitrine.cshtml`. Mesma regra do traçado do IBGE
documentado no `Views/Desempenho/_MapaBrasil.cshtml`: **arquivo local, fonte, URL, licença e data**.

Regras desta pasta:

- **Só fonte de uso livre comercial.** Hoje são todas **domínio público** (Public Domain Mark 1.0),
  via Wikimedia Commons. Sem licença *share-alike* (CC BY-SA) nesta pasta: a página é pública e não
  deve herdar obrigação de relicenciamento.
- **Sem procedência conferida, a imagem não entra.** Se não der para abrir a página de origem e ler a
  licença, não use o arquivo.
- **Teto de 300 KB por arquivo** e largura útil máxima de 1600 px. Toda foto entra com `width` e
  `height` no HTML (sem salto de layout) e, fora da primeira dobra, com `loading="lazy"` e
  `decoding="async"`.
- Todas são em **preto e branco / sépia**: é um tratamento só para a página inteira e funciona nos
  dois temas (no escuro ainda passam pelo filtro do token `--vitrine-foto-filtro`).
- O crédito aparece na própria página, no rodapé (`.v-creditos`).

| Arquivo | Dimensões | Peso | Onde aparece |
| --- | --- | --- | --- |
| `loja-rua-da-carioca.jpg` | 960 × 1267 | 246 KB | Capa (palco fixo do primeiro capítulo) |
| `tendinha-mangueira.jpg` | 900 × 1200 | 124 KB | Catálogo (palco fixo) |
| `armazem-mury.jpg` | 1600 × 900 | 186 KB | "O balcão continua" (palco fixo) |
| `rua-da-quitanda.jpg` | 1500 × 844 | 252 KB | Rodapé (foto de sangria inteira) |

---

## `loja-rua-da-carioca.jpg`

- **O que é:** fachada da Loja de calçados Casa Atlas, Rua da Carioca, Rio de Janeiro, com a vitrine
  cheia e pedestres na calçada.
- **Autor:** campo "Author" do Commons: **Núcleo de digitalização / IMS**; acervo do **Instituto
  Moreira Salles**. (A página tem a categoria oculta "Photographs by Augusto Malta", mas Augusto Malta
  **não** está no campo de autor — por isso o crédito da página cita o acervo, e não o fotógrafo, ao
  contrário do que dizia o comentário da versão antiga da vitrine.)
- **Fonte:** Wikimedia Commons.
- **URL:** https://commons.wikimedia.org/wiki/File:Rua_da_Carioca,_com_destaque_para_a_Loja_de_cal%C3%A7ados_Casa_Atlas,_Acervo_do_Instituto_Moreira_Salles.jpg
- **Licença:** **domínio público** — etiqueta `PD-1923` ("public domain in the United States",
  publicada antes de 1931). Atribuição sugerida pela própria página: "Public domain / Instituto
  Moreira Salles Collection". Não é Creative Commons.
- **Data da foto:** entre c. 1910 e c. 1920.
- **Licença conferida em:** 2026-11-13, lendo a página do Commons (a versão antiga do `_Vitrine.cshtml`
  dizia "Public Domain Mark 1.0", que é a etiqueta da outra foto, não desta).
- **Baixada em:** 2026-10-31 (já estava versionada no git desde o commit `d04544d`).
- **Edição:** nenhuma nesta rodada; o arquivo continua byte a byte igual ao que estava no repositório.

## `armazem-mury.jpg`

- **O que é:** interior do armazém de secos e molhados da família Mury, Nova Friburgo (RJ), começo do
  século XX: prateleiras de garrafas e caixas, balcão de madeira com balança e o dono sentado à mesa
  com os papéis, ao lado de um menino. **É a foto do caderno em cima do balcão** — a imagem que
  sustenta a frase "o balcão continua, o caderno não".
- **Autor:** fotógrafo desconhecido; acervo particular da família Mury (divulgação por Janaína Botelho,
  blog "História e Memória de Nova Friburgo").
- **Fonte:** Wikimedia Commons.
- **URL:** https://commons.wikimedia.org/wiki/File:Armaz%C3%A9m_da_Fam%C3%ADlia_Mury_em_1916_-_Nova_Friburgo.jpg
- **Licença:** domínio público — **Public Domain Mark 1.0** (https://creativecommons.org/publicdomain/mark/1.0/),
  mais as etiquetas `PD-Brazil` e `PD-1923`.
- **Data da foto:** **em conflito na própria fonte** — o nome do arquivo e os metadados dizem 1916; o
  campo "Date" do resumo diz "por volta de 1930?". Por isso a legenda na página escreve só
  "começo do século XX", e não um ano.
- **Licença conferida em:** 2026-11-13, lendo a página do Commons.
- **Baixada em:** 2026-10-31 (já estava versionada no git desde o commit `d04544d`).
- **Edição:** nenhuma nesta rodada; arquivo igual ao do repositório. O recorte em retrato do palco é
  feito no CSS (`object-fit: cover` + `object-position`), não no arquivo.

## `tendinha-mangueira.jpg`

- **O que é:** interior da Tendinha, armazém de secos e molhados na Mangueira, Rio de Janeiro, 1926:
  prateleiras lotadas de mercadoria, caixa registradora e fregueses no balcão.
- **Autor:** Raymundo de Castro.
- **Fonte:** Wikimedia Commons.
- **URL:** https://commons.wikimedia.org/wiki/File:Tendinha_Armaz%C3%A9m_Secos_e_Molhados_na_Mangueira.jpg
- **Licença:** domínio público (Public Domain Mark 1.0) — https://creativecommons.org/publicdomain/mark/1.0/
- **Baixada em:** 2026-11-13.
- **Edição (reproduzível):** do original de 1920 × 1280, recorte `x=540 y=0 w=960 h=1280` (retrato 3:4,
  centrado no balcão), redimensionado para 900 × 1200 e salvo em JPEG qualidade 74 → 124 KB.

## `rua-da-quitanda.jpg`

- **O que é:** rua da Quitanda, Rio de Janeiro: fileira de lojas com o letreiro pintado
  "AO ESPELHO DE VENEZA — SOLIANI FERMO & C." e as portas de enrolar. Chapa de vidro digitalizada.
- **Autor:** Núcleo de digitalização / Instituto Moreira Salles (IMS).
- **Fonte:** Wikimedia Commons.
- **URL:** https://commons.wikimedia.org/wiki/File:Rua_da_Quitanda_-_3.jpg
- **Licença:** domínio público (Public Domain Mark 1.0) — https://creativecommons.org/publicdomain/mark/1.0/
- **Baixada em:** 2026-11-13.
- **Edição (reproduzível):** do original de 1920 × 2571, recorte `x=0 y=1020 w=1920 h=1080` (faixa
  central, onde está o letreiro — o rodapé escreve por cima das bordas de cima e de baixo),
  redimensionado para 1500 × 844 e salvo em JPEG qualidade 66 → 252 KB (q=72 dava 318 KB, acima do teto).
