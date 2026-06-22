using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddAnuncioImg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagemUrl",
                table: "Anuncios",
                newName: "Transmissao");

            migrationBuilder.AddColumn<string>(
                name: "Cor",
                table: "Anuncios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Potencia",
                table: "Anuncios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnuncioImagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnuncioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnuncioImagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnuncioImagens_Anuncios_AnuncioId",
                        column: x => x.AnuncioId,
                        principalTable: "Anuncios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnuncioImagens_AnuncioId",
                table: "AnuncioImagens",
                column: "AnuncioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnuncioImagens");

            migrationBuilder.DropColumn(
                name: "Cor",
                table: "Anuncios");

            migrationBuilder.DropColumn(
                name: "Potencia",
                table: "Anuncios");

            migrationBuilder.RenameColumn(
                name: "Transmissao",
                table: "Anuncios",
                newName: "ImagemUrl");
        }
    }
}
