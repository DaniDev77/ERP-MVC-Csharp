using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fonecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fonecedor",
                columns: table => new
                {
                    FonecedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FonecedorNome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FonecedorDescricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CNPJ = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FonecedorTelefone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FonecedorEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fonecedor", x => x.FonecedorId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fonecedor");
        }
    }
}
