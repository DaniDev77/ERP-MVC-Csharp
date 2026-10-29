namespace Sistema.Models.Ui
{
    // Dados dos partials visuais (_PageHeader, _EmptyState, _KpiCard, _ProdutoThumb).
    // Só apresentação: nenhuma regra de negócio passa por aqui.

    // Cabeçalho de página: título, descrição, ícone e até duas ações
    public class PageHeader
    {
        public string Titulo { get; init; } = string.Empty;
        public string? Descricao { get; init; }
        // Nome do ícone (Helpers/Icones) mostrado numa bolha ao lado do título
        public string? Icone { get; init; }
        // Ação principal (botão com gradiente)
        public string? AcaoTexto { get; init; }
        public string? AcaoUrl { get; init; }
        public string AcaoIcone { get; init; } = "mais";
        // Ação secundária (botão branco com borda)
        public string? SecundariaTexto { get; init; }
        public string? SecundariaUrl { get; init; }
        public string? SecundariaIcone { get; init; }
        // Link "voltar" acima do título
        public string? VoltarTexto { get; init; }
        public string? VoltarUrl { get; init; }
    }

    // Estado vazio: ilustração, texto e chamada para ação
    public class EmptyState
    {
        // Caminho da ilustração em wwwroot/img (ex.: "~/img/vazio-caixas.svg")
        public string Imagem { get; init; } = "~/img/vazio-caixas.svg";
        public string Titulo { get; init; } = string.Empty;
        public string? Texto { get; init; }
        public string? AcaoTexto { get; init; }
        public string? AcaoUrl { get; init; }
        public string AcaoIcone { get; init; } = "mais";
        public string? SecundariaTexto { get; init; }
        public string? SecundariaUrl { get; init; }
        public bool Compacto { get; init; }
    }

    // Card de KPI: rótulo, número grande, ícone em bolha colorida e rodapé
    public class KpiCard
    {
        public string Rotulo { get; init; } = string.Empty;
        public string Valor { get; init; } = string.Empty;
        public string Icone { get; init; } = "moeda";
        // Tom da bolha: brand, violet, success, warning, danger, info
        public string Tom { get; init; } = "brand";
        public string? Rodape { get; init; }
        // Selo ao lado do rodapé (ex.: "12 vendas"), com o tom do chip
        public string? Selo { get; init; }
        public string SeloTom { get; init; } = "brand";
        // Card inteiro clicável quando tem link
        public string? Url { get; init; }
        public bool Alerta { get; init; }
    }

    // Miniatura de produto (placeholder com gradiente e iniciais)
    public class ProdutoThumb
    {
        public string Nome { get; init; } = string.Empty;
        // Classe de tamanho: "thumb-sm", "thumb-lg", "thumb-card" ou vazio (44px)
        public string Tamanho { get; init; } = string.Empty;
    }
}
