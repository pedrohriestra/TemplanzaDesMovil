using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Templanza.Api.Migrations
{
    /// <inheritdoc />
    public partial class SimplifySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blends_Usuarios_CreatedById",
                table: "Blends");

            migrationBuilder.DropTable(
                name: "BlendPlantas");

            migrationBuilder.DropTable(
                name: "Plantas");

            migrationBuilder.DropIndex(
                name: "IX_Blends_CreatedById",
                table: "Blends");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Blends");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Blends");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "Blends",
                newName: "Stock");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Blends",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Blends",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "Blends",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Blends",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Blends");

            migrationBuilder.DropColumn(
                name: "Precio",
                table: "Blends");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Blends");

            migrationBuilder.RenameColumn(
                name: "Stock",
                table: "Blends",
                newName: "CreatedById");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Blends",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Blends",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Blends",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Plantas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Propiedades = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plantas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlendPlantas",
                columns: table => new
                {
                    BlendId = table.Column<int>(type: "int", nullable: false),
                    PlantaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlendPlantas", x => new { x.BlendId, x.PlantaId });
                    table.ForeignKey(
                        name: "FK_BlendPlantas_Blends_BlendId",
                        column: x => x.BlendId,
                        principalTable: "Blends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlendPlantas_Plantas_PlantaId",
                        column: x => x.PlantaId,
                        principalTable: "Plantas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blends_CreatedById",
                table: "Blends",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BlendPlantas_PlantaId",
                table: "BlendPlantas",
                column: "PlantaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blends_Usuarios_CreatedById",
                table: "Blends",
                column: "CreatedById",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
