using BusinessObjects.ThirdParty.OOC.FSSL;
using KontrolaPakowania.PrintService.Logging;
using KontrolaPakowania.PrintService.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Logger = KontrolaPakowania.PrintService.Logging.Logger;

namespace KontrolaPakowania.PrintService
{
    public static class PrintingListener
    {
        private static HttpListener _listener;
        public static volatile bool Running;

        public static void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:54321/print/");
            _listener.Start();

            Logger.Info("Started on http://localhost:54321/print/");

            while (Running)
            {
                HttpListenerContext context = null;

                try
                {
                    context = _listener.GetContext();
                }
                catch (HttpListenerException)
                {
                    if (!Running)
                        break;
                    throw;
                }

                var request = context.Request;
                var response = context.Response;

                try
                {
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    response.AddHeader("Access-Control-Allow-Methods", "POST, OPTIONS");
                    response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

                    if (request.HttpMethod == "OPTIONS")
                    {
                        response.StatusCode = 200;
                        var buffer = Encoding.UTF8.GetBytes("OK");

                        try
                        {
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                        catch { }
                    }
                    else if (request.HttpMethod == "POST")
                    {
                        using (var reader = new StreamReader(request.InputStream))
                        {
                            var body = reader.ReadToEnd();

                            var job = JsonSerializer.Deserialize<PrintJob>(
                                body,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (job?.DataType == "PING")
                            {
                                var buffer = Encoding.UTF8.GetBytes("PONG");
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                            }
                            else
                            {
                                // Fire-and-forget: odpowiedź od razu
                                response.StatusCode = 202;
                                response.Close();

                                // Druk w tle
                                Task.Run(() => PrintManager.Print(job));

                                continue; // idź do kolejnego requestu
                            }
                        }
                    }
                    else
                    {
                        response.StatusCode = 405;
                        var buffer = Encoding.UTF8.GetBytes("Method Not Allowed");
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                    response.StatusCode = 500;

                    try
                    {
                        var buffer = Encoding.UTF8.GetBytes(ex.Message);
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    catch { }
                }
                finally
                {
                    try
                    {
                        response.Close();
                    }
                    catch (HttpListenerException)
                    {
                        // klient zerwał połączenie – NORMALNE
                    }
                    catch (ObjectDisposedException)
                    {
                        // response już zamknięty – OK
                    }
                }
            }

            _listener.Close();
        }

        public static void Stop()
        {
            Running = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch { }
        }
    }
}