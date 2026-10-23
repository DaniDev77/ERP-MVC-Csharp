using Microsoft.EntityFrameworkCore;
using Sistema.Authorization;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.Services
{
    // Acha o usuário padrão pelo CPF ou pelo código (UsuarioId)
    public class BuscaCompradorService(ApplicationDbContext context)
    {
        public async Task<Usuario?> BuscarAsync(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return null;

            var termo = texto.Trim();

            // Primeiro tenta o CPF exatamente como digitado
            var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.CPF == termo);
            if (usuario != null)
                return usuario;

            // Depois tenta o CPF só com números (ex.: digitado com pontos e traço)
            var somenteDigitos = new string(termo.Where(char.IsDigit).ToArray());
            usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.CPF == somenteDigitos);
            if (usuario != null)
                return usuario;

            // Por último, trata como código do usuário; o CPF tem prioridade para não confundir um CPF com um código
            if (int.TryParse(termo, out var codigo))
                return await context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == codigo);

            return null;
        }

        // Só usuário com a função Padrão pode receber um produto numa venda
        public static bool EhPadrao(Usuario usuario)
        {
            return usuario.FuncaoId == Politicas.FuncaoPadraoId;
        }
    }
}
