using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace unofficial_pdrive_http_bridge.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecretsCacheGroups",
                columns: table => new
                {
                    Context_HasValue = table.Column<bool>(type: "INTEGER", nullable: false),
                    Context_Name = table.Column<string>(type: "TEXT", nullable: false),
                    Context_Id = table.Column<string>(type: "TEXT", nullable: false),
                    ValueHolderName = table.Column<string>(type: "TEXT", nullable: false),
                    ValueHolderId = table.Column<string>(type: "TEXT", nullable: false),
                    ValueName = table.Column<string>(type: "TEXT", nullable: false),
                    Secret_Context_HasValue = table.Column<bool>(type: "INTEGER", nullable: false),
                    Secret_Context_Name = table.Column<string>(type: "TEXT", nullable: false),
                    Secret_Context_Id = table.Column<string>(type: "TEXT", nullable: false),
                    Secret_ValueHolderName = table.Column<string>(type: "TEXT", nullable: false),
                    Secret_ValueHolderId = table.Column<string>(type: "TEXT", nullable: false),
                    Secret_ValueName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecretsCacheGroups", x => new { x.Context_HasValue, x.Context_Name, x.Context_Id, x.ValueHolderName, x.ValueHolderId, x.ValueName, x.Secret_Context_HasValue, x.Secret_Context_Name, x.Secret_Context_Id, x.Secret_ValueHolderName, x.Secret_ValueHolderId, x.Secret_ValueName });
                });

            migrationBuilder.CreateTable(
                name: "SecretsCacheSecrets",
                columns: table => new
                {
                    Context_HasValue = table.Column<bool>(type: "INTEGER", nullable: false),
                    Context_Name = table.Column<string>(type: "TEXT", nullable: false),
                    Context_Id = table.Column<string>(type: "TEXT", nullable: false),
                    ValueHolderName = table.Column<string>(type: "TEXT", nullable: false),
                    ValueHolderId = table.Column<string>(type: "TEXT", nullable: false),
                    ValueName = table.Column<string>(type: "TEXT", nullable: false),
                    SecretBytes = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Flags = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecretsCacheSecrets", x => new { x.Context_HasValue, x.Context_Name, x.Context_Id, x.ValueHolderName, x.ValueHolderId, x.ValueName });
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    SessionId = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    AccessToken = table.Column<string>(type: "TEXT", nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                    IsWaitingForSecondFactorCode = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordMode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "SessionScopes",
                columns: table => new
                {
                    Scope = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionScopes", x => x.Scope);
                });

            migrationBuilder.CreateTable(
                name: "TrackedVolumes",
                columns: table => new
                {
                    VolumeId = table.Column<string>(type: "TEXT", nullable: false),
                    LatestEventId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedVolumes", x => x.VolumeId);
                });

            migrationBuilder.CreateTable(
                name: "WebUiPasswords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Password = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebUiPasswords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackedFolders",
                columns: table => new
                {
                    VolumeId = table.Column<string>(type: "TEXT", nullable: false),
                    NodeId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedFolders", x => new { x.VolumeId, x.NodeId });
                    table.ForeignKey(
                        name: "FK_TrackedFolders_TrackedVolumes_VolumeId",
                        column: x => x.VolumeId,
                        principalTable: "TrackedVolumes",
                        principalColumn: "VolumeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodeMetadata",
                columns: table => new
                {
                    VolumeId = table.Column<string>(type: "TEXT", nullable: false),
                    NodeId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ParentNodeId = table.Column<string>(type: "TEXT", nullable: false),
                    IsFile = table.Column<bool>(type: "INTEGER", nullable: false),
                    MediaType = table.Column<string>(type: "TEXT", nullable: true),
                    ActiveRevisionId = table.Column<string>(type: "TEXT", nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: true),
                    ModificationTime = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeMetadata", x => new { x.VolumeId, x.NodeId });
                    table.ForeignKey(
                        name: "FK_NodeMetadata_TrackedFolders_VolumeId_ParentNodeId",
                        columns: x => new { x.VolumeId, x.ParentNodeId },
                        principalTable: "TrackedFolders",
                        principalColumns: new[] { "VolumeId", "NodeId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeMetadata_VolumeId_ParentNodeId",
                table: "NodeMetadata",
                columns: new[] { "VolumeId", "ParentNodeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodeMetadata");

            migrationBuilder.DropTable(
                name: "SecretsCacheGroups");

            migrationBuilder.DropTable(
                name: "SecretsCacheSecrets");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "SessionScopes");

            migrationBuilder.DropTable(
                name: "WebUiPasswords");

            migrationBuilder.DropTable(
                name: "TrackedFolders");

            migrationBuilder.DropTable(
                name: "TrackedVolumes");
        }
    }
}
