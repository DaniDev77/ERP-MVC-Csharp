using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema.Data.Migrations
{
    // Separa Pedido x Compra x Venda sem perder dados.
    // Antes: a tabela Pedido guardava compras (Tipo = 0) e vendas (Tipo = 1), com baixa de estoque na hora.
    // Depois:
    //   - Compra/CompraItem e Venda/VendaItem recebem os registros antigos com os MESMOS ids (CompraId/VendaId = PedidoId antigo);
    //   - Pedido/PedidoItem ficam vazios e passam a ser só os pedidos dos clientes (status, decisão do ADM, localização);
    //   - Usuario ganha UF, cidade e código IBGE (localização informada com consentimento).
    // Escrita à mão: o gerado pelo EF renomeava Tipo→Status e VendedorId→DecididoPorId, o que transformaria as vendas
    // antigas em pedidos "Confirmados" falsos. O SQL Server roda a migration numa transação: se a conferência falhar, nada muda.
    public partial class SepararPedidoCompraVenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------- 1. Localização no cadastro do usuário ----------
            migrationBuilder.AddColumn<string>(name: "Uf", table: "Usuario", type: "nvarchar(2)", maxLength: 2, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Cidade", table: "Usuario", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<int>(name: "CodigoMunicipio", table: "Usuario", type: "int", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "LocalizacaoRespondidaEm", table: "Usuario", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "LocalizacaoRecusada", table: "Usuario", type: "bit", nullable: false, defaultValue: false);

            // ---------- 2. Tabelas novas de compra e venda ----------
            migrationBuilder.CreateTable(
                name: "Compra",
                columns: table => new
                {
                    CompraId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    PedidoId = table.Column<int>(type: "int", nullable: true),
                    Uf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodigoMunicipio = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compra", x => x.CompraId);
                    table.ForeignKey(
                        name: "FK_Compra_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedido",
                        principalColumn: "PedidoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compra_Usuario_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Venda",
                columns: table => new
                {
                    VendaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CompradorId = table.Column<int>(type: "int", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    Uf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodigoMunicipio = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venda", x => x.VendaId);
                    table.ForeignKey(
                        name: "FK_Venda_Usuario_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Venda_Usuario_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompraItem",
                columns: table => new
                {
                    CompraItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompraId = table.Column<int>(type: "int", nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompraItem", x => x.CompraItemId);
                    table.ForeignKey(
                        name: "FK_CompraItem_Compra_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compra",
                        principalColumn: "CompraId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompraItem_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "ProdutoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendaItem",
                columns: table => new
                {
                    VendaItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendaId = table.Column<int>(type: "int", nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendaItem", x => x.VendaItemId);
                    table.ForeignKey(
                        name: "FK_VendaItem_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "ProdutoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendaItem_Venda_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Venda",
                        principalColumn: "VendaId",
                        onDelete: ReferentialAction.Cascade);
                });

            // ---------- 3. Cópia dos dados antigos (mesmo id, para rastrear) e conferência ----------
            // Venda sem vendedor não cabe na tabela nova (VendedorId é obrigatório): para tudo antes de copiar
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM Pedido WHERE Tipo = 1 AND VendedorId IS NULL)
    THROW 50001, 'Migração interrompida: há vendas sem vendedor na tabela Pedido. Corrija antes de migrar.', 1;");

            migrationBuilder.Sql(@"
SET IDENTITY_INSERT Compra ON;
INSERT INTO Compra (CompraId, DataHora, Total, CompradorId, PedidoId, Uf, Cidade, CodigoMunicipio)
SELECT PedidoId, DataHora, Total, CompradorId, NULL, NULL, NULL, NULL FROM Pedido WHERE Tipo = 0;
SET IDENTITY_INSERT Compra OFF;

INSERT INTO CompraItem (CompraId, ProdutoId, Quantidade, PrecoUnitario)
SELECT i.PedidoId, i.ProdutoId, i.Quantidade, i.PrecoUnitario
FROM PedidoItem i INNER JOIN Pedido p ON p.PedidoId = i.PedidoId
WHERE p.Tipo = 0;

SET IDENTITY_INSERT Venda ON;
INSERT INTO Venda (VendaId, DataHora, Total, CompradorId, VendedorId, Uf, Cidade, CodigoMunicipio)
SELECT PedidoId, DataHora, Total, CompradorId, VendedorId, NULL, NULL, NULL FROM Pedido WHERE Tipo = 1;
SET IDENTITY_INSERT Venda OFF;

INSERT INTO VendaItem (VendaId, ProdutoId, Quantidade, PrecoUnitario)
SELECT i.PedidoId, i.ProdutoId, i.Quantidade, i.PrecoUnitario
FROM PedidoItem i INNER JOIN Pedido p ON p.PedidoId = i.PedidoId
WHERE p.Tipo = 1;");

            // Conferência: quantidade de registros, de itens e soma dos totais têm que bater; senão a transação é desfeita
            migrationBuilder.Sql(@"
IF (SELECT COUNT(*) FROM Pedido WHERE Tipo NOT IN (0, 1)) > 0
    THROW 50002, 'Migração interrompida: há registros na tabela Pedido com Tipo desconhecido.', 1;
IF (SELECT COUNT(*) FROM Pedido) <> (SELECT COUNT(*) FROM Compra) + (SELECT COUNT(*) FROM Venda)
    THROW 50003, 'Migração interrompida: a quantidade de compras + vendas copiadas não bate com a tabela Pedido.', 1;
IF (SELECT COUNT(*) FROM PedidoItem) <> (SELECT COUNT(*) FROM CompraItem) + (SELECT COUNT(*) FROM VendaItem)
    THROW 50004, 'Migração interrompida: a quantidade de itens copiados não bate com a tabela PedidoItem.', 1;
IF (SELECT ISNULL(SUM(Total), 0) FROM Pedido) <> (SELECT ISNULL(SUM(Total), 0) FROM Compra) + (SELECT ISNULL(SUM(Total), 0) FROM Venda)
    THROW 50005, 'Migração interrompida: a soma dos totais copiados não bate com a tabela Pedido.', 1;");

            // Os registros antigos já estão em Compra/Venda: Pedido fica vazio para o novo fluxo de pedidos
            migrationBuilder.Sql("DELETE FROM PedidoItem; DELETE FROM Pedido;");

            // ---------- 4. Pedido passa a ser só o pedido do cliente ----------
            migrationBuilder.DropForeignKey(name: "FK_Pedido_Usuario_VendedorId", table: "Pedido");
            migrationBuilder.DropIndex(name: "IX_Pedido_VendedorId", table: "Pedido");
            migrationBuilder.DropColumn(name: "VendedorId", table: "Pedido");
            migrationBuilder.DropColumn(name: "Tipo", table: "Pedido");

            migrationBuilder.AddColumn<int>(name: "Status", table: "Pedido", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<string>(name: "Uf", table: "Pedido", type: "nvarchar(2)", maxLength: 2, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Cidade", table: "Pedido", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<int>(name: "CodigoMunicipio", table: "Pedido", type: "int", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "DataDecisao", table: "Pedido", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<int>(name: "DecididoPorId", table: "Pedido", type: "int", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Motivo", table: "Pedido", type: "nvarchar(300)", maxLength: 300, nullable: true);
            migrationBuilder.AddColumn<byte[]>(name: "RowVersion", table: "Pedido", type: "rowversion", rowVersion: true, nullable: true);

            migrationBuilder.CreateIndex(name: "IX_Pedido_DecididoPorId", table: "Pedido", column: "DecididoPorId");
            migrationBuilder.CreateIndex(name: "IX_Pedido_Status_DataHora", table: "Pedido", columns: new[] { "Status", "DataHora" });
            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Usuario_DecididoPorId",
                table: "Pedido",
                column: "DecididoPorId",
                principalTable: "Usuario",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Restrict);

            // ---------- 5. Índices das tabelas novas ----------
            migrationBuilder.CreateIndex(name: "IX_Compra_CompradorId", table: "Compra", column: "CompradorId");
            migrationBuilder.CreateIndex(name: "IX_Compra_DataHora", table: "Compra", column: "DataHora");
            migrationBuilder.CreateIndex(name: "IX_Compra_PedidoId", table: "Compra", column: "PedidoId", unique: true, filter: "[PedidoId] IS NOT NULL");
            migrationBuilder.CreateIndex(name: "IX_CompraItem_CompraId", table: "CompraItem", column: "CompraId");
            migrationBuilder.CreateIndex(name: "IX_CompraItem_ProdutoId", table: "CompraItem", column: "ProdutoId");
            migrationBuilder.CreateIndex(name: "IX_Venda_CompradorId", table: "Venda", column: "CompradorId");
            migrationBuilder.CreateIndex(name: "IX_Venda_DataHora", table: "Venda", column: "DataHora");
            migrationBuilder.CreateIndex(name: "IX_Venda_VendedorId", table: "Venda", column: "VendedorId");
            migrationBuilder.CreateIndex(name: "IX_VendaItem_ProdutoId", table: "VendaItem", column: "ProdutoId");
            migrationBuilder.CreateIndex(name: "IX_VendaItem_VendaId", table: "VendaItem", column: "VendaId");
        }

        /// <inheritdoc />
        // Volta ao modelo antigo: compras e vendas voltam para Pedido (Tipo 0/1, com ids novos).
        // ATENÇÃO: pedidos pendentes, recusados e cancelados do fluxo novo não existem no modelo antigo e são descartados;
        // a localização dos usuários também é apagada
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Compra_Pedido_PedidoId", table: "Compra");
            migrationBuilder.DropForeignKey(name: "FK_Pedido_Usuario_DecididoPorId", table: "Pedido");
            migrationBuilder.Sql("DELETE FROM PedidoItem; DELETE FROM Pedido;");

            migrationBuilder.DropIndex(name: "IX_Pedido_Status_DataHora", table: "Pedido");
            migrationBuilder.DropIndex(name: "IX_Pedido_DecididoPorId", table: "Pedido");
            migrationBuilder.DropColumn(name: "Status", table: "Pedido");
            migrationBuilder.DropColumn(name: "Uf", table: "Pedido");
            migrationBuilder.DropColumn(name: "Cidade", table: "Pedido");
            migrationBuilder.DropColumn(name: "CodigoMunicipio", table: "Pedido");
            migrationBuilder.DropColumn(name: "DataDecisao", table: "Pedido");
            migrationBuilder.DropColumn(name: "DecididoPorId", table: "Pedido");
            migrationBuilder.DropColumn(name: "Motivo", table: "Pedido");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Pedido");

            migrationBuilder.AddColumn<int>(name: "Tipo", table: "Pedido", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "VendedorId", table: "Pedido", type: "int", nullable: true);

            // Copia compras e vendas de volta; o MERGE devolve o id novo de cada uma para ligar os itens
            migrationBuilder.Sql(@"
DECLARE @mapa TABLE (Origem int NOT NULL, Tipo int NOT NULL, Novo int NOT NULL);

MERGE Pedido AS alvo
USING (
    SELECT CompraId AS Origem, 0 AS Tipo, DataHora, Total, CompradorId, CAST(NULL AS int) AS VendedorId FROM Compra
    UNION ALL
    SELECT VendaId, 1, DataHora, Total, CompradorId, VendedorId FROM Venda
) AS origem ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (Tipo, DataHora, Total, CompradorId, VendedorId)
    VALUES (origem.Tipo, origem.DataHora, origem.Total, origem.CompradorId, origem.VendedorId)
OUTPUT origem.Origem, origem.Tipo, inserted.PedidoId INTO @mapa (Origem, Tipo, Novo);

INSERT INTO PedidoItem (PedidoId, ProdutoId, Quantidade, PrecoUnitario)
SELECT m.Novo, i.ProdutoId, i.Quantidade, i.PrecoUnitario FROM CompraItem i INNER JOIN @mapa m ON m.Origem = i.CompraId AND m.Tipo = 0;

INSERT INTO PedidoItem (PedidoId, ProdutoId, Quantidade, PrecoUnitario)
SELECT m.Novo, i.ProdutoId, i.Quantidade, i.PrecoUnitario FROM VendaItem i INNER JOIN @mapa m ON m.Origem = i.VendaId AND m.Tipo = 1;");

            migrationBuilder.CreateIndex(name: "IX_Pedido_VendedorId", table: "Pedido", column: "VendedorId");
            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Usuario_VendedorId",
                table: "Pedido",
                column: "VendedorId",
                principalTable: "Usuario",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(name: "CompraItem");
            migrationBuilder.DropTable(name: "VendaItem");
            migrationBuilder.DropTable(name: "Compra");
            migrationBuilder.DropTable(name: "Venda");

            migrationBuilder.DropColumn(name: "Uf", table: "Usuario");
            migrationBuilder.DropColumn(name: "Cidade", table: "Usuario");
            migrationBuilder.DropColumn(name: "CodigoMunicipio", table: "Usuario");
            migrationBuilder.DropColumn(name: "LocalizacaoRespondidaEm", table: "Usuario");
            migrationBuilder.DropColumn(name: "LocalizacaoRecusada", table: "Usuario");
        }
    }
}
