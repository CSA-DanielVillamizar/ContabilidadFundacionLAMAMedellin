using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.Controllers;
using Server.Services.Reportes;
using Xunit;

namespace UnitTests
{
    public class StubReportesService : IReportesService
    {
        public Task<byte[]> GenerarReporteMensualExcelAsync(int anio, int mes, CancellationToken ct = default)
        {
            var data = new byte[256];
            for (int i = 0; i < data.Length; i++) data[i] = (byte)(i % 255);
            return Task.FromResult(data);
        }

        public Task<byte[]> GenerarReporteMensualPdfAsync(int anio, int mes, CancellationToken ct = default)
        {
            var data = new byte[512];
            for (int i = 0; i < data.Length; i++) data[i] = (byte)((i * 7) % 255);
            return Task.FromResult(data);
        }

        public Task<TesoreriaMesResult> GenerarReporteMensualAsync(int anio, int mes, CancellationToken ct = default)
        {
            return Task.FromResult(new TesoreriaMesResult(System.DateTime.UtcNow, anio, mes, 0m, 0m, 0m, 0m));
        }
    }

    public class ReportesEndpointsTests
    {
        [Fact]
        public async Task TesoreriaPdf_DevuelveFileConHeaders()
        {
            var svc = new StubReportesService();
            var controller = new ReportsController(svc);
            var result = await controller.TesoreriaPdf(2025, 10);
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", file.ContentType);
            Assert.Equal("reporte-tesoreria-2025-10.pdf", file.FileDownloadName);
            Assert.True(file.FileContents.Length > 0);
        }

        [Fact]
        public async Task TesoreriaExcel_DevuelveFileConHeaders()
        {
            var svc = new StubReportesService();
            var controller = new ReportsController(svc);
            var result = await controller.TesoreriaExcel(2025, 10);
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
            Assert.Equal("reporte-tesoreria-2025-10.xlsx", file.FileDownloadName);
            Assert.True(file.FileContents.Length > 0);
        }
    }
}
