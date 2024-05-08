using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecAll.Contrib.MaksedTestList.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "maskedtestlist_hilo",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "maskedtestlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MaskedId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaskedContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserIdentityGuid = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maskedtestlists", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maskedtestlists_MaskedId",
                table: "maskedtestlists",
                column: "MaskedId",
                unique: true,
                filter: "[MaskedId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_maskedtestlists_UserIdentityGuid",
                table: "maskedtestlists",
                column: "UserIdentityGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maskedtestlists");

            migrationBuilder.DropSequence(
                name: "maskedtestlist_hilo");
        }
    }
}
