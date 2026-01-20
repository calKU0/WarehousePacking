using BusinessObjects.ThirdParty.OOC.FSSL;
using CrystalDecisions.CrystalReports.Engine;
using KontrolaPakowania.PrintService.Logging;
using KontrolaPakowania.PrintService.Models;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Logger = KontrolaPakowania.PrintService.Logging.Logger;

namespace KontrolaPakowania.PrintService
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
                    // Używamy ShrinkToMargin - jeśli PDF jest większy od obszaru druku, zostanie pomniejszony.
                    // Jeśli jest mniejszy, zostanie wydrukowany w rozmiarze oryginalnym.
                    using (var printDocument = pdfDocument.CreatePrintDocument(PdfPrintMode.ShrinkToMargin))
                    {
                        printDocument.PrinterSettings.PrinterName = printerName;
                        printDocument.PrintController = new StandardPrintController();

                        // Pobierz rozmiar pierwszej strony w punktach (1/72 cala)
                        var pdfPageSize = pdfDocument.PageSizes[0];
                        bool isLandscape = pdfPageSize.Width > pdfPageSize.Height;

                        // Ustawienie orientacji
                        printDocument.DefaultPageSettings.Landscape = isLandscape;

                        // WYEROWANIE MARGINESÓW - to jest klucz do "dużego" wydruku
                        printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                        printDocument.OriginAtMargins = false; // (0,0) to fizyczna krawędź kartki

                        // Ręczne ustawienie rozmiaru papieru, aby pasował do PDF (opcjonalne, ale pomaga)
                        // .NET używa jednostek 1/100 cala, PDF używa 1/72 cala (punktów)
                        int widthHundredths = (int)(pdfPageSize.Width / 72.0 * 100.0);
                        int heightHundredths = (int)(pdfPageSize.Height / 72.0 * 100.0);

                        // Próba znalezienia i ustawienia pasującego rozmiaru papieru w drukarce
                        foreach (PaperSize size in printDocument.PrinterSettings.PaperSizes)
                        {
                            // Szukamy rozmiaru zbliżonego (np. A4) lub ustawiamy Custom
                            if (Math.Abs(size.Width - (isLandscape ? heightHundredths : widthHundredths)) < 10)
                            {
                                printDocument.DefaultPageSettings.PaperSize = size;
                                break;
                            }
                        }

                        printDocument.QueryPageSettings += (sender, e) =>
                        {
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