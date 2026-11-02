using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// ExecuteUpdateAsync: apaga a localização das operações antigas (QA B2)
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Models;
using Sistema.Services;

// Perfil de quem está logado: mostra os dados da própria conta (qualquer usuário logado vê só o seu).
// Alterado: agora também guarda a localização (UF e cidade) informada pelo próprio usuário, com consentimento (LGPD)
[Authorize]
public class PerfilController(UsuarioLogadoService usuarioLogado, MunicipioService municipios, ApplicationDbContext context) : Controller
{
    // GET: PERFIL
    // Usuario é null quando a conta ainda não completou o cadastro; a view mostra o convite para completar
    public async Task<IActionResult> Index()
    {
        var usuario = await usuarioLogado.ObterAsync(User);
        // Novo: dados do formulário de localização (com as cidades da UF já salva, para funcionar sem JavaScript)
        if (usuario != null)
            ViewBag.Localizacao = MontarLocalizacao(usuario.Uf, usuario.CodigoMunicipio, usuario, Url.Action(nameof(Index)));
        return View(usuario);
    }

    // Novo — POST: PERFIL/Localizacao
    // Salva UF e cidade escolhidas. A cidade precisa ser da lista do IBGE e pertencer à UF
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Localizacao(LocalizacaoViewModel model)
    {
        var usuario = await usuarioLogado.ObterAsync(User);
        if (usuario == null)
            return RedirectToAction(nameof(Index));

        var voltar = UrlLocalOuPerfil(model.ReturnUrl);
        var uf = model.Uf?.Trim().ToUpperInvariant();

        if (!MunicipioService.UfValida(uf))
        {
            TempData["Erro"] = "Escolha o estado (UF).";
            return Redirect(voltar);
        }

        var municipio = municipios.Buscar(uf, model.CodigoMunicipio);
        if (municipio == null)
        {
            // Sem JavaScript, o primeiro envio só troca a UF: volta ao perfil já com as cidades dessa UF na lista
            ViewBag.Localizacao = MontarLocalizacao(uf, null, usuario, model.ReturnUrl);
            ViewBag.LocalizacaoErro = "Escolha a cidade.";
            return View(nameof(Index), usuario);
        }

        usuario.Uf = municipio.Uf;
        usuario.Cidade = municipio.Nome;
        usuario.CodigoMunicipio = municipio.Codigo;
        usuario.LocalizacaoRecusada = false;
        usuario.LocalizacaoRespondidaEm = DateTime.Now;
        await context.SaveChangesAsync();

        TempData["Sucesso"] = $"Localização salva: {municipio.Nome} - {municipio.Uf}.";
        return Redirect(voltar);
    }

    // Novo — POST: PERFIL/RecusarLocalizacao
    // "Prefiro não informar": apaga a localização guardada e não pergunta mais na compra
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecusarLocalizacao(string? returnUrl)
    {
        var usuario = await usuarioLogado.ObterAsync(User);
        if (usuario == null)
            return RedirectToAction(nameof(Index));

        // Corrigido (QA B2, decisão do dono: LGPD art. 18): apagar a localização também apaga as cópias dela nos pedidos,
        // compras e vendas do cliente. Antes só o perfil era limpo e o painel do ADM continuava mostrando a cidade.
        // Tudo numa transação: ou some de todos os lugares, ou de nenhum
        await using var transacao = await context.Database.BeginTransactionAsync();

        // Diferencia "apagar" (já tinha cidade) de "prefiro não informar" (nunca informou), só para a mensagem
        var tinhaLocalizacao = usuario.Uf != null;
        usuario.Uf = null;
        usuario.Cidade = null;
        usuario.CodigoMunicipio = null;
        usuario.LocalizacaoRecusada = true;
        usuario.LocalizacaoRespondidaEm = DateTime.Now;
        await context.SaveChangesAsync();

        var id = usuario.UsuarioId;
        await context.Pedidos.Where(p => p.CompradorId == id && p.Uf != null)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Uf, (string?)null).SetProperty(p => p.Cidade, (string?)null).SetProperty(p => p.CodigoMunicipio, (int?)null));
        await context.Compras.Where(c => c.CompradorId == id && c.Uf != null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Uf, (string?)null).SetProperty(c => c.Cidade, (string?)null).SetProperty(c => c.CodigoMunicipio, (int?)null));
        await context.Vendas.Where(v => v.CompradorId == id && v.Uf != null)
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.Uf, (string?)null).SetProperty(v => v.Cidade, (string?)null).SetProperty(v => v.CodigoMunicipio, (int?)null));

        await transacao.CommitAsync();

        TempData["Sucesso"] = tinhaLocalizacao
            ? "Sua localização foi apagada do perfil e das compras anteriores. Você pode informar de novo em Meu perfil."
            : "Tudo bem: não vamos usar sua localização. Você pode informar depois em Meu perfil.";
        return Redirect(UrlLocalOuPerfil(returnUrl));
    }

    // Novo — GET: PERFIL/Cidades?uf=SP
    // Lista de municípios da UF em JSON, para o select de cidade ser preenchido sem recarregar a página
    [HttpGet]
    public IActionResult Cidades(string? uf)
    {
        var lista = municipios.Cidades(uf?.Trim()).Select(m => new { codigo = m.Codigo, nome = m.Nome });
        return Json(lista);
    }

    // Monta o formulário de localização: UF/cidade atuais e as cidades da UF mostrada
    private LocalizacaoViewModel MontarLocalizacao(string? uf, int? codigo, Usuario usuario, string? returnUrl)
    {
        return new LocalizacaoViewModel
        {
            Uf = uf,
            CodigoMunicipio = codigo,
            ReturnUrl = returnUrl,
            CidadeAtual = usuario.Cidade == null ? null : $"{usuario.Cidade} - {usuario.Uf}",
            Recusada = usuario.LocalizacaoRecusada,
            Respondida = usuario.LocalizacaoRespondidaEm != null,
            Ufs = MunicipioService.Ufs,
            Cidades = municipios.Cidades(uf)
        };
    }

    // Só volta para endereços do próprio sistema (evita redirecionamento aberto para outro site)
    private string UrlLocalOuPerfil(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Index))!;
    }
}
