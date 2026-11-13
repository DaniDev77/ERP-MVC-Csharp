namespace Sistema.Models
{
    // Números do painel inicial: o ADM vê a loja toda; vendedor e padrão veem só o próprio mês
    public class InicioViewModel
    {
        public bool Logado { get; set; }

        public bool EhAdmin { get; set; }

        public bool EhVendedor { get; set; }

        public bool EhPadrao { get; set; }

        // Conta sem função ainda (aguardando o ADM liberar)
        public bool Pendente { get; set; }

        public string NomeUsuario { get; set; } = string.Empty;

        // Primeiro dia do mês exibido
        public DateTime Mes { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        // Totais do mês (ADM: a loja; vendedor: as próprias vendas; padrão: as próprias compras)
        public decimal ValorVendasMes { get; set; }

        public int QtdVendasMes { get; set; }

        public decimal ValorComprasMes { get; set; }

        public int QtdComprasMes { get; set; }

        // Só ADM: estoque
        public int TotalEstoqueBaixo { get; set; }

        public decimal ValorEmEstoque { get; set; }

        // Só ADM: quantos produtos existem e quantos estão zerados (o resumo no pé do cartão de reposição)
        public int TotalProdutos { get; set; }

        public int TotalSemEstoque { get; set; }

        public List<Produto> EstoqueBaixo { get; set; } = new();

        // Alterado: antes era List<Pedido> (compras e vendas na mesma tabela). Agora junta as compras
        // confirmadas e as vendas, já com os nomes prontos
        public List<Movimentacao> UltimosPedidos { get; set; } = new();

        // Novo: ADM = pedidos de clientes aguardando confirmação na loja toda; Padrão = os próprios pedidos pendentes
        public int PedidosPendentes { get; set; }

        // Gráfico de vendas por dia do mês, até hoje: pontos já prontos para o <polyline> do SVG
        public string PontosVendasPorDia { get; set; } = string.Empty;

        public decimal MaiorVendaDoDia { get; set; }

        // Vendas somadas de cada dia do mês, do dia 1 até hoje (o gráfico de colunas do início)
        public decimal[] VendasPorDia { get; set; } = Array.Empty<decimal>();

        // Novo (direção D, B7): os pendentes mais antigos para a fila do Início do ADM (só leitura)
        public List<PedidoNaFila> FilaPendentes { get; set; } = new();

        // Novo (lote B10, HF-015): a vitrine de produtos, que passou a ser a home do visitante.
        // Preenchidos SÓ no ramo de quem não entrou; para quem entrou ficam no valor inicial.
        // IReadOnlyList porque a lista vem do cache compartilhado do VitrineService e ninguém pode
        // alterá-la (RV3). Nunca nula: com a vitrine desligada, para quem entrou e com catálogo vazio,
        // fica a lista vazia, então a view não precisa de verificação de nulo
        public IReadOnlyList<ProdutoVitrine> Vitrine { get; set; } = Array.Empty<ProdutoVitrine>();

        // Quantos produtos estão à venda hoje (mesmo recorte da lista; a lista é cortada no teto de 60)
        public int VitrineTotal { get; set; }

        // Novo (lote B10, RV10): a chave Sistema:MostrarVitrine passou da view para o controller, para
        // existir uma fonte só e para o controller não consultar o catálogo com a vitrine desligada
        public bool MostrarVitrine { get; set; }
    }

    // Novo (lote B10, HF-015): uma linha do catálogo público. SÓ o que a página mostra a um visitante sem
    // login. Nada de Estoque exato, EstoqueMinimo, Fornecedor, custo, ProdutoId, RowVersion, Descricao
    // (RV6) ou dado de pessoa: o que não está aqui não existe para o anônimo.
    // record: o instantâneo fica no cache em memória e é servido a muitos visitantes ao mesmo tempo,
    // então nenhuma parte dele pode ser alterada depois de montada (RV3)
    public sealed record ProdutoVitrine(string Nome, string? Tipo, decimal Preco, bool Disponivel);

    // Novo (lote B10, HF-015): o instantâneo inteiro do catálogo (lista + contagem) num objeto só, para
    // entrar e sair do cache numa chave única e sempre coerente consigo mesmo
    public sealed record VitrineCatalogo(IReadOnlyList<ProdutoVitrine> Itens, int Total);

    // Novo (direção D, B7): uma linha da fila do Início do ADM, só com as colunas que a fila mostra
    public class PedidoNaFila
    {
        public int PedidoId { get; set; }
        public DateTime DataHora { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public int Unidades { get; set; }
        public decimal Total { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
    }
}
