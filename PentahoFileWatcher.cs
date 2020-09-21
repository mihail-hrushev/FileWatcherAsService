using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newton =  Newtonsoft.Json; 

namespace FileWatcherPentahoRun
{

    public class FolderAndCommand: IEquatable<FolderAndCommand>
    {
        public string folderName;
        public string commandPath; 

        public FolderAndCommand( string folder, string command)
        {
            folderName = folder;
            commandPath = command; 
        }

        public bool Equals(FolderAndCommand other)
        {
            if ((this.folderName == other.folderName) && (this.commandPath == other.commandPath))
            {
                return true;
            } else {
                return false;
            }
        }
    }

    class PentahoFileWatcher
    {
        
    #region FIELDS
        private System.Timers.Timer timer;
        
        private bool isReadyForNewTaskExecute = true;
        private bool rerunafter = false;

        private List<FileSystemWatcher> myWatch;
        private List<FolderAndCommand> executionQueue; 
        private List<FolderAndCommand> _itemList;

        #endregion

    #region EVENTS 
        //If app already doing something, then add item to queue, else, add it to queue, execute and lau
        public void RunCommandEvent(object sender, EventArgs e, FolderAndCommand comm)
        {
            //if task already exist in queue - return
            if (this.executionQueue.Contains(comm)) return;
            //if not - add to queu
            this.executionQueue.Add(comm);
            //if queue already executer - return
            if (!isReadyForNewTaskExecute) return;
            //start queu execution. 
            RunNextTaskFromQueue();
        }

        //
        private void OnTimedEvent(Object source, EventArgs e)
        {
            timer.Stop();
            timer.Enabled = false;
            RunNextTaskFromQueue();
        }

        /// <summary>
        /// Each time after delay - execute next command in queue, if queu is over - stop. 
        /// </summary>
        private void RunNextTaskFromQueue () {
            isReadyForNewTaskExecute = true;
            //if something in queue - reset timer, execute command.
            if (executionQueue.Count > 0)
            {
                isReadyForNewTaskExecute = false;
                timer.Enabled = true;
                timer.Start();
                this.ExecuteCommand(executionQueue[0]);
                executionQueue.RemoveAt(0);
                return; 
            }
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
        #endregion
        
    #region CONST

        public PentahoFileWatcher()
        {
            myWatch = new List<FileSystemWatcher>();
            _itemList = new List<FolderAndCommand>();
            executionQueue = new List<FolderAndCommand>();
            timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            //timer.Interval = 140000;
            timer.Interval = 140000;
        }

        #endregion

    #region Public 

        public void RemoveItem(FolderAndCommand item)
        {
            this._itemList.Remove(item);
        }

        public bool AddItem(string folderName, string commandName)
        {
            var item = new FolderAndCommand(folderName, commandName);

            if (_itemList.Contains(item)) { return false; }

            _itemList.Add(item);

            return true;
        }
        
        public void LoadItems(string filePath)
        {
            string json = File.ReadAllText(filePath);
            _itemList = Newton.JsonConvert.DeserializeObject<List<FolderAndCommand>>(json);

            _itemList.ForEach(item =>
           { if (item.commandPath!=null && item.commandPath!="" &&item.folderName!=null&& item.folderName!="")
               CreateFileWatcher(item);
           });
        }
        
        public void SaveItems(string filePath)
        {
            File.WriteAllText(filePath, Newton.JsonConvert.SerializeObject(_itemList));
        }

        #endregion

        private ILogger _logger; 

        public PentahoFileWatcher setLogger(ILogger logger)
        {
            this._logger = logger;
            return this; 
        }

        private void CreateFileWatcher(FolderAndCommand foldComm)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            this._logger.WriteToFile($"Watch At: {foldComm.folderName} ");
            this._logger.WriteToFile($"Execute: {foldComm.commandPath} ");
            watcher.Path = System.IO.Path.GetDirectoryName(foldComm.folderName);

            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.

            var filterExt = "*" + Path.GetExtension(foldComm.folderName);

            watcher.Filter = filterExt;

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler((sender, e) => RunCommandEvent(sender, e, foldComm));
            //watcher.Changed += new FileSystemEventHandler(this.OnChanged);
            //watcher.Created += new FileSystemEventHandler(this.OnChanged);
            //watcher.Deleted += new FileSystemEventHandler(this.OnChanged);
            //watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            this.myWatch.Add(watcher);
        }

        private void RunScript(string processFileName)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + processFileName,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                //startInfo.EnvironmentVariables.Add("CATALINA_HOME", @"c:\server");

                var process = new Process();
                process.StartInfo = startInfo;
                process.Start();
            }
            catch(Exception e)
            {
                this._logger.WriteToFile(e.Message);
            }
        }


        private void ExecuteCommand(FolderAndCommand command)
        {
            this._logger.WriteToFile( $" ExecuteCommand: {command.commandPath} ");

            this.RunScript(command.commandPath);

            //System.Diagnostics.Process.Start(command.commandPath);

            //var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            //processInfo.CreateNoWindow = true;
            //processInfo.UseShellExecute = false;
            //processInfo.RedirectStandardError = true;
            //processInfo.RedirectStandardOutput = true;

            //var process = Process.Start(processInfo);

            //process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            //    Console.WriteLine("output>>" + e.Data);
            //process.BeginOutputReadLine();

            //process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            //    Console.WriteLine("error>>" + e.Data);
            //process.BeginErrorReadLine();

            //process.WaitForExit();

            //Console.WriteLine("ExitCode: {0}", process.ExitCode);
            //process.Close();
        }

    }
}
