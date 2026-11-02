namespace Sistema.Models
{
    // Novo: tela de gerenciamento de pedidos do ADM (Pedido/Index)
    public class PedidosAdminViewModel
    {
        // Filtro de status: "pendente" (padrão), "confirmado", "recusado", "cancelado" ou "todos"
        public string Status { get; set; } = "pendente";

        // Filtro de período (Helpers/Periodos): semana, mes, bimestre, semestre, ano ou todos
        public string Periodo { get; set; } = "todos";

        public DateTime Inicio { get; set; }

        public DateTime Fim { get; set; }

        // Quantos pedidos de cada status existem no período (os números dos filtros)
        public Dictionary<StatusPedido, int> Contagem { get; set; } = new();

        // Corrigido (B-5): pedidos do filtro (até 200), com comprador, quem decidiu, itens e produtos.
        // Pendentes: do mais antigo para o mais recente (fila). Demais status: mais recentes primeiro
        public List<Pedido> Pedidos { get; set; } = new();
    }

    // Novo: formulário de localização do usuário (perfil e aviso na tela de compra)
    public class LocalizacaoViewModel
    {
        // Sigla da UF escolhida (ex.: "SP")
        public string? Uf { get; set; }

        // Código IBGE do município escolhido
        public int? CodigoMunicipio { get; set; }

        // Para onde voltar depois de salvar (só URLs locais são aceitas)
        public string? ReturnUrl { get; set; }

        // Só leitura (preenchidos pelo controller para a tela)
        public string? CidadeAtual { get; set; }
        public bool Recusada { get; set; }
        public bool Respondida { get; set; }
        public IReadOnlyDictionary<string, string> Ufs { get; set; } = new Dictionary<string, string>();
        public IReadOnlyList<Sistema.Services.MunicipioService.Municipio> Cidades { get; set; } = Array.Empty<Sistema.Services.MunicipioService.Municipio>();
    }

    // Novo: painel de compras por localidade (Desempenho/Localidades, só ADM)
    public class LocalidadesViewModel
    {
        public string Periodo { get; set; } = "mes";

        public DateTime Inicio { get; set; }

        public DateTime Fim { get; set; }

        // "compras" (padrão), "vendas" ou "todos"
        public string Tipo { get; set; } = "compras";

        // Novo (HF-007): região escolhida (norte, nordeste, sudeste, sul, centro-oeste); nulo = Brasil todo.
        // Filtra PorUf e, sem UF escolhida, PorCidade
        public string? Regiao { get; set; }

        // UF selecionada para ver o ranking de cidades; nulo = Brasil todo (sozinha, não filtra PorUf)
        public string? Uf { get; set; }

        // Novo (HF-007): gráfico em destaque na tela: "regiao", "uf" (padrão) ou "cidade". Só apresentação
        public string Ver { get; set; } = "uf";

        public int QtdTotal { get; set; }

        public decimal ValorTotal { get; set; }

        // Alterado (HF-007): operações sem localização gravada na própria operação (não informada na época
        // ou apagada pelo cliente, B2). Não usa mais a localização atual do cliente
        public int QtdSemLocal { get; set; }

        public decimal ValorSemLocal { get; set; }

        // Novo (HF-007): uma linha por região com movimento, do maior valor para o menor (Brasil todo, sem filtro)
        public List<LinhaLocalidade> PorRegiao { get; set; } = new();

        // Uma linha por UF com movimento (sem "Não informado"), do maior valor para o menor; só da Regiao, se houver
        public List<LinhaLocalidade> PorUf { get; set; } = new();

        // Cidades da UF selecionada; sem UF, da Regiao; sem as duas, do Brasil todo (até 20), do maior valor para o menor
        public List<LinhaLocalidade> PorCidade { get; set; } = new();
    }

    // Uma linha do painel por localidade
    public class LinhaLocalidade
    {
        // Sigla da UF (linha de UF), código IBGE do município (linha de cidade) ou chave da região (linha de região)
        public string Chave { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;

        // Vazio nas linhas de região
        public string Uf { get; set; } = string.Empty;

        // Novo (HF-007): chave da região da linha (norte, nordeste, sudeste, sul, centro-oeste), para os links de detalhamento
        public string Regiao { get; set; } = string.Empty;

        public int Quantidade { get; set; }

        public decimal Valor { get; set; }

        // Fatia do valor total com localização, sempre sobre o Brasil todo (0 a 100), nas três listas.
        // PorRegiao soma 100; PorUf filtrado por região soma a fatia da região
        public decimal Percentual { get; set; }
    }

    // Novo: uma linha de "Últimos pedidos" do início do ADM (junta compras confirmadas e vendas)
    public class Movimentacao
    {
        public bool EhVenda { get; set; }

        public DateTime DataHora { get; set; }

        public string Cliente { get; set; } = string.Empty;

        // Só nas vendas
        public string? Vendedor { get; set; }

        public decimal Total { get; set; }
    }
}
