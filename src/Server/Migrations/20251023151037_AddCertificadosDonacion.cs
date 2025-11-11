using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificadosDonacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificadosDonacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    Consecutivo = table.Column<int>(type: "int", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaDonacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoIdentificacionDonante = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IdentificacionDonante = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreDonante = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DireccionDonante = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CiudadDonante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TelefonoDonante = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmailDonante = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DescripcionDonacion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ValorDonacionCOP = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FormaDonacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DestinacionDonacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReciboId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NitEntidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreEntidad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EntidadRTE = table.Column<bool>(type: "bit", nullable: false),
                    ResolucionRTE = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaResolucionRTE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NombreRepresentanteLegal = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IdentificacionRepresentante = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CargoRepresentante = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NombreContador = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TarjetaProfesionalContador = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NombreRevisorFiscal = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TarjetaProfesionalRevisorFiscal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    RazonAnulacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificadosDonacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificadosDonacion_Recibos_ReciboId",
                        column: x => x.ReciboId,
                        principalTable: "Recibos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificadosDonacion_Ano_Consecutivo",
                table: "CertificadosDonacion",
                columns: new[] { "Ano", "Consecutivo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificadosDonacion_ReciboId",
                table: "CertificadosDonacion",
                column: "ReciboId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificadosDonacion");
        }
    }
}
