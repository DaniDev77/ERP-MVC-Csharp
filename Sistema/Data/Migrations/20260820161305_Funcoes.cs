using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema.Data.Migrations
{
    /// <inheritdoc />
    public partial class Funcoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Funcao_TempId",
                table: "Funcao");

            migrationBuilder.RenameColumn(
                name: "TempId",
                table: "Funcao",
                newName: "FuncaoId");

            migrationBuilder.AlterColumn<int>(
                name: "FuncaoId",
                table: "Funcao",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Funcao",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Funcao",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Funcao",
                table: "Funcao",
                column: "FuncaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Funcao",
                table: "Funcao");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Funcao");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Funcao");

            migrationBuilder.RenameColumn(
                name: "FuncaoId",
                table: "Funcao",
                newName: "TempId");

            migrationBuilder.AlterColumn<int>(
                name: "TempId",
                table: "Funcao",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Funcao_TempId",
                table: "Funcao",
                column: "TempId");
        }
    }
}
