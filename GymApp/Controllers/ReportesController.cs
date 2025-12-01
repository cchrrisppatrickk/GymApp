using ClosedXML.Excel;
using GymApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GymApp.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        public IActionResult Index()
        {
            // Por defecto muestra el mes actual
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerIngresos(int mes, int anio)
        {
            var data = await _reporteService.ObtenerReporteMensualAsync(mes, anio);
            return Json(new { success = true, data = data });
        }



        [HttpGet]
        public async Task<IActionResult> ExportarExcel(int mes, int anio)
        {
            // 1. Obtener los datos usando tu servicio existente
            var datos = await _reporteService.ObtenerReporteMensualAsync(mes, anio);
            string nombreMes = new DateTime(anio, mes, 1).ToString("MMMM yyyy").ToUpper();

            // 2. Crear el libro de Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Control Ingresos");

                // ==========================================
                // CABECERAS (Dibujando la estructura)
                // ==========================================

                // Fila 1: Título General
                worksheet.Cell("A1").Value = $"CONTROL DE INGRESOS GENERAL - {nombreMes}";
                worksheet.Range("A1:S1").Merge().Style
                    .Font.SetBold().Font.SetFontSize(14)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.White);

                // Fila 2: Encabezados Principales (Categorías)
                // DIA
                worksheet.Cell("A2").Value = "DIA";
                worksheet.Range("A2:A4").Merge().Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                // BEBIDAS (Columnas B-E)
                worksheet.Cell("B2").Value = "BEBIDAS";
                worksheet.Range("B2:E2").Merge().Style.Fill.SetBackgroundColor(XLColor.LightGreen);

                // SUB BEBIDAS (Total)
                worksheet.Cell("F2").Value = "SUB";
                worksheet.Range("F2:F3").Merge().Style.Fill.SetBackgroundColor(XLColor.LightGray);

                // LIBRES (Columnas G-J)
                worksheet.Cell("G2").Value = "LIBRES";
                worksheet.Range("G2:J2").Merge().Style.Fill.SetBackgroundColor(XLColor.LightYellow);

                // SUB LIBRES (Total)
                worksheet.Cell("K2").Value = "SUB";
                worksheet.Range("K2:K3").Merge().Style.Fill.SetBackgroundColor(XLColor.LightGray);

                // XB (Columnas L-O)
                worksheet.Cell("L2").Value = "XB";
                worksheet.Range("L2:O2").Merge().Style.Fill.SetBackgroundColor(XLColor.LightSalmon);

                // SUB XB (Total)
                worksheet.Cell("P2").Value = "SUB";
                worksheet.Range("P2:P3").Merge().Style.Fill.SetBackgroundColor(XLColor.LightGray);

                // TOTALES FINALES (Columnas Q-S)
                worksheet.Cell("Q2").Value = "TOT. EFEC";
                worksheet.Range("Q2:Q4").Merge().Style.Fill.SetBackgroundColor(XLColor.SeaGreen).Font.SetFontColor(XLColor.White);

                worksheet.Cell("R2").Value = "TOT. YAPE";
                worksheet.Range("R2:R4").Merge().Style.Fill.SetBackgroundColor(XLColor.SkyBlue);

                worksheet.Cell("S2").Value = "TOTAL GRAL";
                worksheet.Range("S2:S4").Merge().Style.Fill.SetBackgroundColor(XLColor.Black).Font.SetFontColor(XLColor.White);

                // Fila 3: Turnos (Mañana / Tarde)
                worksheet.Cell("B3").Value = "MAÑANA"; worksheet.Range("B3:C3").Merge();
                worksheet.Cell("D3").Value = "TARDE"; worksheet.Range("D3:E3").Merge();

                worksheet.Cell("G3").Value = "MAÑANA"; worksheet.Range("G3:H3").Merge();
                worksheet.Cell("J3").Value = "TARDE"; worksheet.Range("I3:J3").Merge(); // Ojo con índices

                worksheet.Cell("L3").Value = "MAÑANA"; worksheet.Range("L3:M3").Merge();
                worksheet.Cell("N3").Value = "TARDE"; worksheet.Range("N3:O3").Merge();

                // Fila 4: Métodos de Pago
                var headersPago = new[] { "Efec", "Yape", "Efec", "Yape", "S/.", "Efec", "Yape", "Efec", "Yape", "S/.", "Efec", "Yape", "Efec", "Yape", "S/." };
                for (int i = 0; i < headersPago.Length; i++)
                {
                    worksheet.Cell(4, i + 2).Value = headersPago[i];
                }

                // Estilo general de cabeceras
                worksheet.Range("A2:S4").Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Font.SetBold()
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                // ==========================================
                // CUERPO (Datos)
                // ==========================================
                int fila = 5;
                foreach (var item in datos)
                {
                    worksheet.Cell(fila, 1).Value = item.Dia;

                    // Bebidas
                    worksheet.Cell(fila, 2).Value = item.BebidasMananaEfectivo;
                    worksheet.Cell(fila, 3).Value = item.BebidasMananaYape;
                    worksheet.Cell(fila, 4).Value = item.BebidasTardeEfectivo;
                    worksheet.Cell(fila, 5).Value = item.BebidasTardeYape;
                    worksheet.Cell(fila, 6).Value = item.TotalBebidas; // Subtotal

                    // Libres
                    worksheet.Cell(fila, 7).Value = item.LibresMananaEfectivo;
                    worksheet.Cell(fila, 8).Value = item.LibresMananaYape;
                    worksheet.Cell(fila, 9).Value = item.LibresTardeEfectivo;
                    worksheet.Cell(fila, 10).Value = item.LibresTardeYape;
                    worksheet.Cell(fila, 11).Value = item.TotalLibres; // Subtotal

                    // XB
                    worksheet.Cell(fila, 12).Value = item.XBMananaEfectivo;
                    worksheet.Cell(fila, 13).Value = item.XBMananaYape;
                    worksheet.Cell(fila, 14).Value = item.XBTardeEfectivo;
                    worksheet.Cell(fila, 15).Value = item.XBTardeYape;
                    worksheet.Cell(fila, 16).Value = item.TotalXB; // Subtotal

                    // Totales Finales
                    worksheet.Cell(fila, 17).Value = item.TotalDiaEfectivo;
                    worksheet.Cell(fila, 18).Value = item.TotalDiaYape;
                    worksheet.Cell(fila, 19).Value = item.TotalGeneralDia;

                    fila++;
                }

                // ==========================================
                // PIE DE PÁGINA (Sumatorias)
                // ==========================================
                worksheet.Cell(fila, 1).Value = "TOTAL MES";
                worksheet.Cell(fila, 1).Style.Font.SetBold();

                // Fórmula automática: SUM(B5:B[fila-1])
                for (int col = 2; col <= 19; col++)
                {
                    var colLetter = XLHelper.GetColumnLetterFromNumber(col);
                    worksheet.Cell(fila, col).FormulaA1 = $"SUM({colLetter}5:{colLetter}{fila - 1})";
                    worksheet.Cell(fila, col).Style.Font.SetBold();
                }

                // Colores de los totales finales en el footer
                worksheet.Cell(fila, 17).Style.Fill.SetBackgroundColor(XLColor.SeaGreen).Font.SetFontColor(XLColor.White);
                worksheet.Cell(fila, 18).Style.Fill.SetBackgroundColor(XLColor.SkyBlue);
                worksheet.Cell(fila, 19).Style.Fill.SetBackgroundColor(XLColor.Black).Font.SetFontColor(XLColor.White);

                // ==========================================
                // FORMATO FINAL
                // ==========================================

                // Bordes a toda la tabla
                var rangoDatos = worksheet.Range(2, 1, fila, 19);
                rangoDatos.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                rangoDatos.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

                // Formato moneda a todas las celdas numéricas
                worksheet.Range(5, 2, fila, 19).Style.NumberFormat.Format = "0.00";

                // Ajustar ancho de columnas automáticamente
                worksheet.Columns().AdjustToContents();

                // 3. Devolver el archivo
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Reporte_Ingresos_{mes}_{anio}.xlsx");
                }
            }
        }


    }
}