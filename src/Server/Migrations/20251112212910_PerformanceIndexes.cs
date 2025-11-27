using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Índices para Recibos - mejorar filtrado por fecha y estado
            migrationBuilder.CreateIndex(
                name: "IX_Recibos_FechaEmision",
                table: "Recibos",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_Estado",
                table: "Recibos",
                column: "Estado");

            // Índice compuesto para consultas comunes de recibos por rango de fechas y estado
            migrationBuilder.CreateIndex(
                name: "IX_Recibos_FechaEmision_Estado",
                table: "Recibos",
                columns: new[] { "FechaEmision", "Estado" });

            // Índices para Egresos - mejorar filtrado por fecha y categoría
            migrationBuilder.CreateIndex(
                name: "IX_Egresos_Fecha",
                table: "Egresos",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Egresos_Categoria",
                table: "Egresos",
                column: "Categoria");

            // Índice compuesto para reportes de egresos por período y categoría
            migrationBuilder.CreateIndex(
                name: "IX_Egresos_Fecha_Categoria",
                table: "Egresos",
                columns: new[] { "Fecha", "Categoria" });

            // Índice para Miembros - filtrado por estado
            migrationBuilder.CreateIndex(
                name: "IX_Miembros_Estado",
                table: "Miembros",
                column: "Estado");

            // Índices para VentasProductos - filtrado por estado y fecha
            migrationBuilder.CreateIndex(
                name: "IX_VentasProductos_Estado",
                table: "VentasProductos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_VentasProductos_FechaVenta",
                table: "VentasProductos",
                column: "FechaVenta");

            // Índices para ComprasProductos - filtrado por estado y fecha
            migrationBuilder.CreateIndex(
                name: "IX_ComprasProductos_Estado",
                table: "ComprasProductos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasProductos_FechaCompra",
                table: "ComprasProductos",
                column: "FechaCompra");

            // Índice adicional para Productos omitido (ya existe índice único por 'Codigo')

            // Índice para MovimientosInventario - filtrado por tipo y fecha
            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_Tipo",
                table: "MovimientosInventario",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_FechaMovimiento",
                table: "MovimientosInventario",
                column: "FechaMovimiento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar índices en orden inverso
            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_FechaMovimiento",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_Tipo",
                table: "MovimientosInventario");

            // No se creó índice adicional para Productos

            migrationBuilder.DropIndex(
                name: "IX_ComprasProductos_FechaCompra",
                table: "ComprasProductos");

            migrationBuilder.DropIndex(
                name: "IX_ComprasProductos_Estado",
                table: "ComprasProductos");

            migrationBuilder.DropIndex(
                name: "IX_VentasProductos_FechaVenta",
                table: "VentasProductos");

            migrationBuilder.DropIndex(
                name: "IX_VentasProductos_Estado",
                table: "VentasProductos");

            migrationBuilder.DropIndex(
                name: "IX_Miembros_Estado",
                table: "Miembros");

            // Nota: No se creó índice para NumeroIdentificacion porque la columna no existe en el esquema actual

            migrationBuilder.DropIndex(
                name: "IX_Egresos_Fecha_Categoria",
                table: "Egresos");

            migrationBuilder.DropIndex(
                name: "IX_Egresos_Categoria",
                table: "Egresos");

            migrationBuilder.DropIndex(
                name: "IX_Egresos_Fecha",
                table: "Egresos");

            migrationBuilder.DropIndex(
                name: "IX_Recibos_FechaEmision_Estado",
                table: "Recibos");

            migrationBuilder.DropIndex(
                name: "IX_Recibos_Estado",
                table: "Recibos");

            migrationBuilder.DropIndex(
                name: "IX_Recibos_FechaEmision",
                table: "Recibos");
        }
    }
}
