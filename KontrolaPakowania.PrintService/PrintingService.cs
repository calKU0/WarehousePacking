using KontrolaPakowania.PrintService.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KontrolaPakowania.PrintService
{
    public partial class PrintingService : ServiceBase
    {
        private Thread listenerThread;

        public PrintingService()
        {
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            PrintingListener.Running = true;

            listenerThread = new Thread(() =>
            {
                try
                {
                    PrintingListener.Start();
                }
                catch (Exception ex)
                {
                    Logger.Error("PrintingListener crashed:");
                    Logger.Error(ex.ToString());
                }
            })
            {
                IsBackground = true
            };

            listenerThread.Start();
        }

        protected override void OnStop()
        {
            PrintingListener.Running = false;
            PrintingListener.Stop();

            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Join(3000);
            }
        }
    }
}