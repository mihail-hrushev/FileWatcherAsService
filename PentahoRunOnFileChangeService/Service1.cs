using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using FileWatcherPentahoRun;

namespace PentahoRunOnFileChangeService
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer(); // name space(using System.Timers;)  
        PentahoFileWatcher pentaho;
        private ILogger _logger; 

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            this._logger = new Logger(path);
            pentaho = new PentahoFileWatcher()
                            .setLogger(this._logger);
            pentaho.LoadItems(path+@"\Command.json");

            this._logger.WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 300000; //number in milisecinds  
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            this._logger.WriteToFile("Service is stopped at " + DateTime.Now);
        }
  

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            this._logger.WriteToFile("Service is recall at " + DateTime.Now);
        }
    }
}
