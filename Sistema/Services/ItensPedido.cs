namespace Sistema.Services
{
    // Novo: validação dos itens comum a pedido e venda (antes ficava dentro do PedidoService.RegistrarAsync)
    public static class ItensPedido
    {
        // Novo (QA B3): teto por produto num pedido ou venda. Bem acima de qualquer estoque real e longe do limite do int
        public const int QuantidadeMaxima = 1_000_000;

        // Confere a lista e soma as linhas repetidas do mesmo produto. Devolve o erro, ou null se estiver tudo certo
        public static (List<(int ProdutoId, int Quantidade)> Itens, string? Erro) Preparar(IReadOnlyList<(int ProdutoId, int Quantidade)> itens)
        {
            // Pedido sem produto não faz sentido
            if (itens.Count == 0)
                return (new(), "Selecione pelo menos um produto.");

            // Quantidade zero ou negativa não pode dar baixa no estoque
            if (itens.Any(i => i.Quantidade <= 0))
                return (new(), "A quantidade deve ser maior que zero.");

            // Se o mesmo produto aparecer em duas linhas, as quantidades são somadas.
            // Corrigido (QA B3): a soma era feita em int e estourava (OverflowException, erro 500) com quantidades enormes
            // repetidas no mesmo produto. Agora soma em long e recusa o que passar do limite
            var somados = itens
                .GroupBy(i => i.ProdutoId)
                .Select(g => (ProdutoId: g.Key, Quantidade: g.Sum(x => (long)x.Quantidade)))
                .ToList();
            if (somados.Any(i => i.Quantidade > QuantidadeMaxima))
                return (new(), $"A quantidade de cada produto pode ser no máximo {QuantidadeMaxima:N0}.");

            return (somados.Select(i => (i.ProdutoId, (int)i.Quantidade)).ToList(), null);
        }
    }
}
