using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema.Data.Migrations
{
    /// <inheritdoc />
    public partial class AcessoPorFuncaoESeguranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Funcao_FuncaoId",
                table: "Usuario");

            migrationBuilder.AlterColumn<int>(
                name: "FuncaoId",
                table: "Usuario",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Produto",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_AppUserId",
                table: "Usuario",
                column: "AppUserId",
                unique: true,
                filter: "[AppUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Funcao_FuncaoId",
                table: "Usuario",
                column: "FuncaoId",
                principalTable: "Funcao",
                principalColumn: "FuncaoId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Funcao_FuncaoId",
                table: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_AppUserId",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Produto");

            migrationBuilder.AlterColumn<int>(
                name: "FuncaoId",
                table: "Usuario",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Funcao_FuncaoId",
                table: "Usuario",
                column: "FuncaoId",
                principalTable: "Funcao",
                principalColumn: "FuncaoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
