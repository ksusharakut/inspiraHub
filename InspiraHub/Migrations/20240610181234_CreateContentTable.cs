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
                name: "content", // Исправлено имя таблицы
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                   .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    preview = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    create_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content", x => x.id); // Исправлено имя таблицы
                    table.ForeignKey(
                        name: "FK_Contents_User_userId",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content"); // Исправлено имя таблицы
        }
    }
}
