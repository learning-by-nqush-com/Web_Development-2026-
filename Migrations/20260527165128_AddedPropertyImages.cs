using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staybnb.Migrations
{
    /// <inheritdoc />
    public partial class AddedPropertyImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PropertyId",
                table: "PropertyImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_PropertyId",
                table: "PropertyImages",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyImages_Properties_PropertyId",
                table: "PropertyImages",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyImages_Properties_PropertyId",
                table: "PropertyImages");

            migrationBuilder.DropIndex(
                name: "IX_PropertyImages_PropertyId",
                table: "PropertyImages");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                table: "PropertyImages");
        }
    }
}
