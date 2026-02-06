using CrystalDecisions.CrystalReports.Engine;
using WarehousePacking.PrintService.Models;
using PdfiumViewer;
using System;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Logger = WarehousePacking.PrintService.Logging.Logger;

namespace WarehousePacking.PrintService
{
    public static class PrintManager
    {
        public static void Print(PrintJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            if (job.DataType == "ZPL" || job.DataType == "EPL")
            {
                var bytes = Convert.FromBase64String(job.Content);

                if (job.PrinterName.Contains(":"))
                    NetworkPrint(job.PrinterName, bytes);
                else
                    PrintRaw(job.PrinterName, bytes);
            }
            else if (job.DataType == "PDF")
            {
                var bytes = Convert.FromBase64String(job.Content);
                PrintPdf(bytes, job.PrinterName);
            }
            else if (job.DataType == "CRYSTAL")
            {
                PrintCrystalReport(job);
            }
            else
            {
                throw new Exception($"Unsupported DataType: {job.DataType}");
            }
        }

        private static void PrintRaw(string printerName, byte[] label)
        {
            var printerSettings = new SharpZebra.Printing.PrinterSettings
            {
                PrinterName = printerName
            };

            var printer = new SharpZebra.Printing.SpoolPrinter(printerSettings);
            if (!printer.Print(label).GetValueOrDefault())
                throw new Exception($"Failed to print to {printerName}");
        }

        private static void NetworkPrint(string printerName, byte[] label)
        {
            var parts = printerName.Split(':');

            var settings = new SharpZebra.Printing.PrinterSettings
            {
                PrinterName = parts[0],
                PrinterPort = parts.Length > 1 ? int.Parse(parts[1]) : 9100
            };

            var printer = new SharpZebra.Printing.NetworkPrinter(settings);
            if (!printer.Print(label).GetValueOrDefault())
                throw new Exception($"Failed to print to network printer {printerName}");
        }

        // Copy your existing methods here unchanged
        private static void PrintPdf(byte[] pdfBytes, string printerName)
        {
            try
            {
                using (var stream = new MemoryStream(pdfBytes))
                using (var pdfDocument = PdfDocument.Load(stream))
                {
                    // Fit to full paper instead of only shrinking
                    using (var printDocument = pdfDocument.CreatePrintDocument(PdfPrintMode.CutMargin))
                    {
                        printDocument.PrinterSettings.PrinterName = printerName;
                        printDocument.PrintController = new StandardPrintController();

                        var pdfPageSize = pdfDocument.PageSizes[0];
                        bool isLandscape = pdfPageSize.Width > pdfPageSize.Height;

                        // pick A4 if available, otherwise the largest available size
                        var paperSizes = printDocument.PrinterSettings.PaperSizes.Cast<PaperSize>();
                        var selectedPaper = paperSizes.FirstOrDefault(p => p.Kind == PaperKind.A4)
                                           ?? paperSizes.OrderByDescending(p => p.Width * p.Height).FirstOrDefault();

                        if (selectedPaper != null)
                        {
                            printDocument.DefaultPageSettings.PaperSize = selectedPaper;
                        }

                        printDocument.DefaultPageSettings.Landscape = isLandscape;
                        printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                        printDocument.OriginAtMargins = false;

                        printDocument.QueryPageSettings += (sender, e) =>
                        {
                            if (selectedPaper != null)
                                e.PageSettings.PaperSize = selectedPaper;

                            e.PageSettings.Landscape = isLandscape;
                            e.PageSettings.Margins = new Margins(0, 0, 0, 0);
                        };

                        printDocument.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Błąd podczas drukowania: " + ex.Message);
                throw;
            }
        }

        private static void PrintCrystalReport(PrintJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            var report = new ReportDocument();
            try
            {
                // Load the report
                report.Load(job.Content);

                // Apply database logon for each table
                if (job.Parameters != null &&
                    job.Parameters.TryGetValue("DbUser", out var dbUser) &&
                    job.Parameters.TryGetValue("DbPassword", out var dbPassword) &&
                    job.Parameters.TryGetValue("DbServer", out var dbServer) &&
                    job.Parameters.TryGetValue("DbName", out var dbName))
                {
                    report.SetDatabaseLogon(dbUser, dbPassword, dbServer, dbName);
                }

                // Apply parameters and selection formula
                if (job.Parameters != null)
                {
                    foreach (var kv in job.Parameters)
                    {
                        if (kv.Key.Equals("DbUser", StringComparison.OrdinalIgnoreCase) ||
                            kv.Key.Equals("DbPassword", StringComparison.OrdinalIgnoreCase) ||
                            kv.Key.Equals("DbServer", StringComparison.OrdinalIgnoreCase) ||
                            kv.Key.Equals("DbName", StringComparison.OrdinalIgnoreCase))
                            continue;

                        report.SetParameterValue(kv.Key, kv.Value);
                    }
                }
                // Set printer and print
                report.PrintOptions.PrinterName = string.IsNullOrEmpty(job.PrinterName) ? string.Empty : job.PrinterName;
                //report.ExportToDisk(ExportFormatType.PortableDocFormat, @"D:\a\Test.pdf");
                report.PrintToPrinter(1, true, 0, 0);
            }
            catch (Exception ex)
            {
                Logger.Error("Error printing report: " + ex);
            }
            finally
            {
                // Safe cleanup
                try { report.Close(); } catch { }
                try { report.Dispose(); } catch { }
            }
        }

        private static class NativeMethods
        {
            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibrary(string lpFileName);
        }
    }
}