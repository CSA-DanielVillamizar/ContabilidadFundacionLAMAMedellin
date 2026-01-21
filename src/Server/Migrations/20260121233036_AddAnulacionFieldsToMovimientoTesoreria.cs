using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAnulacionFieldsToMovimientoTesoreria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: TasasCambio.CreatedAt and TasasCambio.EsOficial already exist in database
            // (added by earlier migration). Skipping duplicate AddColumn commands.

            migrationBuilder.CreateTable(
                name: "CategoriasEgreso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsGastoSocial = table.Column<bool>(type: "bit", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasEgreso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuentasFinancieras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Banco = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumeroCuenta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TitularCuenta = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SaldoInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasFinancieras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FuentesIngreso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuentesIngreso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosTesoreria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroMovimiento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    CuentaFinancieraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FuenteIngresoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoriaEgresoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Medio = table.Column<int>(type: "int", nullable: false),
                    ReferenciaTransaccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TerceroId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TerceroNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoporteUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAprobacion = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAnulacion = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReciboId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ImportHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ImportSource = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImportSheet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImportRowNumber = table.Column<int>(type: "int", nullable: true),
                    ImportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImportHasBalanceMismatch = table.Column<bool>(type: "bit", nullable: false),
                    ImportBalanceExpected = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ImportBalanceFound = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosTesoreria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosTesoreria_CategoriasEgreso_CategoriaEgresoId",
                        column: x => x.CategoriaEgresoId,
                        principalTable: "CategoriasEgreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosTesoreria_CuentasFinancieras_CuentaFinancieraId",
                        column: x => x.CuentaFinancieraId,
                        principalTable: "CuentasFinancieras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosTesoreria_FuentesIngreso_FuenteIngresoId",
                        column: x => x.FuenteIngresoId,
                        principalTable: "FuentesIngreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosTesoreria_Recibos_ReciboId",
                        column: x => x.ReciboId,
                        principalTable: "Recibos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AportesMensuales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MiembroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    ValorEsperado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MovimientoTesoreriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AportesMensuales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AportesMensuales_Miembros_MiembroId",
                        column: x => x.MiembroId,
                        principalTable: "Miembros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AportesMensuales_MovimientosTesoreria_MovimientoTesoreriaId",
                        column: x => x.MovimientoTesoreriaId,
                        principalTable: "MovimientosTesoreria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CategoriasEgreso",
                columns: new[] { "Id", "Activa", "Codigo", "CreatedAt", "CreatedBy", "CuentaContableId", "Descripcion", "EsGastoSocial", "Nombre" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), true, "AYUDA-SOCIAL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Proyectos de ayuda social (RTE)", true, "Ayuda Social" },
                    { new Guid("30000000-0000-0000-0000-000000000002"), true, "EVENTO-LOG", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Gastos de organización de eventos", false, "Logística de Eventos" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), true, "COMPRA-MERCH", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Parches, souvenirs, jerseys", false, "Compra Inventario Mercancía" },
                    { new Guid("30000000-0000-0000-0000-000000000004"), true, "COMPRA-CLUB-CAFE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Café, capuchino, etc.", false, "Compra Insumos Casa Club - Café" },
                    { new Guid("30000000-0000-0000-0000-000000000005"), true, "COMPRA-CLUB-CERV", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Cerveza, bebidas alcohólicas", false, "Compra Insumos Casa Club - Cerveza" },
                    { new Guid("30000000-0000-0000-0000-000000000006"), true, "COMPRA-CLUB-COMI", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Alimentos para emparedados, snacks", false, "Compra Insumos Casa Club - Comida" },
                    { new Guid("30000000-0000-0000-0000-000000000007"), true, "COMPRA-CLUB-OTROS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Artículos moteros para venta", false, "Compra Insumos Casa Club - Otros" },
                    { new Guid("30000000-0000-0000-0000-000000000008"), true, "ADMIN-PAPEL", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Papelería, oficina", false, "Gastos Administrativos - Papelería" },
                    { new Guid("30000000-0000-0000-0000-000000000009"), true, "ADMIN-TRANSP", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Transporte, combustible", false, "Gastos Administrativos - Transporte" },
                    { new Guid("30000000-0000-0000-0000-000000000010"), true, "ADMIN-SERVICIOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Internet, telefonía, servicios públicos", false, "Gastos Administrativos - Servicios" },
                    { new Guid("30000000-0000-0000-0000-000000000011"), true, "MANTENIMIENTO", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Mantenimiento de infraestructura", false, "Mantenimiento" },
                    { new Guid("30000000-0000-0000-0000-000000000012"), true, "OTROS-GASTOS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Gastos misceláneos", false, "Otros Gastos" }
                });

            migrationBuilder.InsertData(
                table: "CuentasFinancieras",
                columns: new[] { "Id", "Activa", "Banco", "Codigo", "CreatedAt", "CreatedBy", "FechaApertura", "Nombre", "NumeroCuenta", "Observaciones", "SaldoActual", "SaldoInicial", "Tipo", "TitularCuenta", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), true, "Bancolombia", "BANCO-BCOL-001", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bancolombia Cuenta Corriente Principal", "****5678", null, 0m, 0m, 1, null, null, null });

            migrationBuilder.InsertData(
                table: "FuentesIngreso",
                columns: new[] { "Id", "Activa", "Codigo", "CreatedAt", "CreatedBy", "CuentaContableId", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), true, "APORTE-MEN", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "$20.000 COP recurrente", "Aporte Mensual Miembro" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), true, "VENTA-MERCH", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Souvenirs, jerseys, parches, gorras", "Venta Mercancía" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), true, "VENTA-CLUB-ART", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Artículos moteros vendidos en casa club", "Venta Casa Club - Artículos Moteros" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), true, "VENTA-CLUB-CAFE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Café vendido en casa club", "Venta Casa Club - Café" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), true, "VENTA-CLUB-CERV", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Cerveza vendida en casa club", "Venta Casa Club - Cerveza" },
                    { new Guid("20000000-0000-0000-0000-000000000006"), true, "VENTA-CLUB-COMI", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Emparedados, snacks, comida ligera", "Venta Casa Club - Comida" },
                    { new Guid("20000000-0000-0000-0000-000000000007"), true, "DONACION", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Donaciones recibidas (RTE)", "Donación" },
                    { new Guid("20000000-0000-0000-0000-000000000008"), true, "EVENTO", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Ingresos por eventos organizados", "Evento" },
                    { new Guid("20000000-0000-0000-0000-000000000009"), true, "RENOVACION-MEM", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Renovación anual de membresía", "Renovación Membresía" },
                    { new Guid("20000000-0000-0000-0000-000000000010"), true, "OTROS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "seed", null, "Ingresos misceláneos", "Otros Ingresos" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AportesMensuales_MiembroId_Ano_Mes",
                table: "AportesMensuales",
                columns: new[] { "MiembroId", "Ano", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AportesMensuales_MovimientoTesoreriaId",
                table: "AportesMensuales",
                column: "MovimientoTesoreriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasEgreso_Codigo",
                table: "CategoriasEgreso",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasFinancieras_Codigo",
                table: "CuentasFinancieras",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuentesIngreso_Codigo",
                table: "FuentesIngreso",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_CategoriaEgresoId",
                table: "MovimientosTesoreria",
                column: "CategoriaEgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_CuentaFinancieraId",
                table: "MovimientosTesoreria",
                column: "CuentaFinancieraId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_Fecha_Tipo_CuentaFinancieraId_Estado",
                table: "MovimientosTesoreria",
                columns: new[] { "Fecha", "Tipo", "CuentaFinancieraId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_FuenteIngresoId",
                table: "MovimientosTesoreria",
                column: "FuenteIngresoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_ImportHash",
                table: "MovimientosTesoreria",
                column: "ImportHash");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_NumeroMovimiento",
                table: "MovimientosTesoreria",
                column: "NumeroMovimiento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosTesoreria_ReciboId",
                table: "MovimientosTesoreria",
                column: "ReciboId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AportesMensuales");

            migrationBuilder.DropTable(
                name: "MovimientosTesoreria");

            migrationBuilder.DropTable(
                name: "CategoriasEgreso");

            migrationBuilder.DropTable(
                name: "CuentasFinancieras");

            migrationBuilder.DropTable(
                name: "FuentesIngreso");

            // NOTE: NOT dropping TasasCambio.CreatedAt and TasasCambio.EsOficial
            // as they were added by an earlier migration
        }
    }
}
