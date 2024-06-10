using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InspiraHub.Migrations
{
    /// <inheritdoc />
    public partial class CreateContentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                 name: "сontents",
                 columns: table => new
                 {
                     Id = table.Column<long>(type: "bigint", nullable: false)
                     .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                     UserId = table.Column<long>(type: "bigint", nullable: false),
                     Preview = table.Column<string>(type: "text", nullable: true),
                     Title = table.Column<string>(type: "text", nullable: true),
                     Description = table.Column<string>(type: "text", nullable: true),
                     CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                     ContentType = table.Column<string>(type: "text", nullable: true)
                 },
                 constraints: table =>
                 {
                     table.PrimaryKey("PK_Contents", x => x.Id);
                     table.ForeignKey(
                         name: "FK_Contents_User_UserId",
                         column: x => x.UserId,
                         principalTable: "User",
                         principalColumn: "Id",
                         onDelete: ReferentialAction.Cascade);
                 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contents");
        }
    }
}
