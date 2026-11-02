// Debug.Assert: confere o mapa de regiões (HF-007)
using System.Diagnostics;
using System.Text.Json;

namespace Sistema.Services
{
    // Novo: lista oficial de UFs e municípios do IBGE, embutida no sistema (Data/Ibge/municipios.json).
    // Nada é consultado fora do servidor: a localização do cliente não sai do sistema (LGPD).
    // Registrado como singleton: o arquivo é lido uma vez só.
    public class MunicipioService
    {
        public record Municipio(int Codigo, string Nome, string Uf);

        // As 27 unidades da federação, em ordem alfabética de sigla
        public static readonly IReadOnlyDictionary<string, string> Ufs = new SortedDictionary<string, string>
        {
            ["AC"] = "Acre", ["AL"] = "Alagoas", ["AM"] = "Amazonas", ["AP"] = "Amapá", ["BA"] = "Bahia",
            ["CE"] = "Ceará", ["DF"] = "Distrito Federal", ["ES"] = "Espírito Santo", ["GO"] = "Goiás",
            ["MA"] = "Maranhão", ["MG"] = "Minas Gerais", ["MS"] = "Mato Grosso do Sul", ["MT"] = "Mato Grosso",
            ["PA"] = "Pará", ["PB"] = "Paraíba", ["PE"] = "Pernambuco", ["PI"] = "Piauí", ["PR"] = "Paraná",
            ["RJ"] = "Rio de Janeiro", ["RN"] = "Rio Grande do Norte", ["RO"] = "Rondônia", ["RR"] = "Roraima",
            ["RS"] = "Rio Grande do Sul", ["SC"] = "Santa Catarina", ["SE"] = "Sergipe", ["SP"] = "São Paulo",
            ["TO"] = "Tocantins"
        };

        // Novo (HF-007): regiões na ordem dos códigos do IBGE (1 N, 2 NE, 3 SE, 4 S, 5 CO).
        // Fica DEPOIS de Ufs: inicializadores estáticos rodam na ordem do arquivo
        public static readonly IReadOnlyList<(string Chave, string Nome, IReadOnlyList<string> Ufs)> Regioes =
        [
            ("norte", "Norte", ["AC", "AM", "AP", "PA", "RO", "RR", "TO"]),
            ("nordeste", "Nordeste", ["AL", "BA", "CE", "MA", "PB", "PE", "PI", "RN", "SE"]),
            ("sudeste", "Sudeste", ["ES", "MG", "RJ", "SP"]),
            ("sul", "Sul", ["PR", "RS", "SC"]),
            ("centro-oeste", "Centro-Oeste", ["DF", "GO", "MS", "MT"]),
        ];

        // Novo (HF-007): UF → região; o ToDictionary já falha se uma UF aparecer em duas regiões
        private static readonly Dictionary<string, string> RegiaoPorUf =
            Regioes.SelectMany(r => r.Ufs, (r, uf) => (Uf: uf, r.Chave)).ToDictionary(x => x.Uf, x => x.Chave);

        private static readonly Dictionary<string, string> NomePorRegiao = Regioes.ToDictionary(r => r.Chave, r => r.Nome);

        // Novo (HF-007): garante as 27 UFs, cada uma numa região (só em Debug; o projeto de testes está sem fontes)
        static MunicipioService() => Debug.Assert(RegiaoPorUf.Count == Ufs.Count && Ufs.Keys.All(RegiaoPorUf.ContainsKey));

        private readonly Dictionary<int, Municipio> _porCodigo;
        private readonly Dictionary<string, List<Municipio>> _porUf;

        public MunicipioService()
        {
            // O JSON vai como recurso embutido no .dll (Sistema.csproj), então não depende da pasta de publicação
            using var fluxo = typeof(MunicipioService).Assembly.GetManifestResourceStream("Sistema.Data.Ibge.municipios.json")
                ?? throw new InvalidOperationException("Lista de municípios do IBGE não encontrada (Data/Ibge/municipios.json).");
            var itens = JsonSerializer.Deserialize<List<ItemArquivo>>(fluxo) ?? new();

            var lista = itens.Select(i => new Municipio(i.c, i.n, i.u)).ToList();
            _porCodigo = lista.ToDictionary(m => m.Codigo);
            _porUf = lista.GroupBy(m => m.Uf).ToDictionary(g => g.Key, g => g.OrderBy(m => m.Nome, StringComparer.Create(new System.Globalization.CultureInfo("pt-BR"), true)).ToList());
        }

        public static bool UfValida(string? uf) => uf != null && Ufs.ContainsKey(uf);

        public static string NomeUf(string? uf) => uf != null && Ufs.TryGetValue(uf, out var nome) ? nome : "Não informado";

        // Novo (HF-007): consultas de região
        public static bool RegiaoValida(string? chave) => chave != null && NomePorRegiao.ContainsKey(chave);

        public static string? RegiaoDaUf(string? uf) => uf != null && RegiaoPorUf.TryGetValue(uf, out var r) ? r : null;

        public static string NomeRegiao(string? chave) => chave != null && NomePorRegiao.TryGetValue(chave, out var n) ? n : "Não informado";

        // Municípios de uma UF, em ordem alfabética; vazio se a UF não existir
        public IReadOnlyList<Municipio> Cidades(string? uf)
        {
            return uf != null && _porUf.TryGetValue(uf.ToUpperInvariant(), out var lista) ? lista : Array.Empty<Municipio>();
        }

        // Acha o município pelo código e confere se ele é da UF informada (impede UF e cidade trocadas no formulário)
        public Municipio? Buscar(string? uf, int? codigo)
        {
            if (codigo == null || !_porCodigo.TryGetValue(codigo.Value, out var municipio))
                return null;
            return string.Equals(municipio.Uf, uf, StringComparison.OrdinalIgnoreCase) ? municipio : null;
        }

        // Formato compacto do arquivo: c = código IBGE, n = nome, u = UF
        private sealed record ItemArquivo(int c, string n, string u);
    }
}
