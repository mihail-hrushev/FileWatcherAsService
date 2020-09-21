using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace FileWatcherPentahoRun
{

    class myFileSystemWatcher : IDisposable
    {
        private FileSystemWatcher watcher;
        private FileSystemEventHandler eventhandler;
        private FolderAndCommand fold;
        public Action eventAction { get; set; }

        public myFileSystemWatcher(FileSystemWatcher w, FolderAndCommand fold)
        {
            this.watcher = w;
            this.fold = fold;
            timer = new System.Timers.Timer();
            timerHandler = new System.Timers.ElapsedEventHandler(OnTimedEvent);
            timer.Elapsed += timerHandler;
        }

        public myFileSystemWatcher setEvent(FileSystemEventHandler e)
        {
            this.eventhandler = e;
            StartWatch();
            return this;
        }

        public void Stopwatch()
        {
            watcher.Changed -= eventhandler;
        }

        public void StartWatch()
        {
            watcher.Changed += eventhandler;
        }


        private System.Timers.Timer timer;
        private System.Timers.ElapsedEventHandler timerHandler; 

        public void PospondeToAvoidUnreasonableEventsCalls(double delay = 1000)
        {
            Stopwatch();
            timer.Interval = delay;
            timer.Start();
        }

        private void OnTimedEvent(Object source, EventArgs e)
        {
            timer.Stop();
            timer.Enabled = false;
            StartWatch();
        }

        public void Dispose()
        {
            watcher.Changed -= eventhandler;
            eventhandler = null;
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;
        }

    }

    class PentahoFileWatcher
    {
        
    #region FIELDS
        private System.Timers.Timer timer;
        
        private bool isReadyForNewTaskExecute = true;

        private List<myFileSystemWatcher> myWatch;
        private List<FolderAndCommand> executionQueue; 
        private List<FolderAndCommand> _itemList;

        private ILogger _logger;

        public PentahoFileWatcher setLogger(ILogger logger)
        {
            this._logger = logger;
            return this;
        }

        private string _itemListPath; 


        #endregion

     #region EVENTS 
        //If app already doing something, then add item to queue, else, add it to queue, execute and lau
        public void RunCommandEvent(object sender, EventArgs e, myFileSystemWatcher watcher, FolderAndCommand comm)
        {
            //if task already exist in queue - return
            this._logger.WriteToFile($"Try to add event in execution queue - {comm.folderName}");
            if (this.executionQueue.Contains(comm)) return;

            //if not - add to queu
            this._logger.WriteToFile($"Add to excution queue new Item - {comm.folderName}");
            this.executionQueue.Add(comm);
            watcher.PospondeToAvoidUnreasonableEventsCalls();
            //if queue already executer - return
            if (!isReadyForNewTaskExecute) return;
            //start queu execution. 
            this._logger.WriteToFile($"Run task -  {comm.folderName}");
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
            if (executionQueue.Count == 0) return;

            isReadyForNewTaskExecute = false;
            timer.Enabled = true;
            timer.Start();
            this.ExecuteCommand(executionQueue[0]);
            executionQueue.RemoveAt(0);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
     #endregion
        
    #region CONSTRUCTOR

        public PentahoFileWatcher(string _itemListPath, double delay)
        {
            this._itemListPath = _itemListPath; 
            myWatch = new List<myFileSystemWatcher>();
            _itemList = new List<FolderAndCommand>();
            executionQueue = new List<FolderAndCommand>();
            timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            timer.Interval = delay;
            ListFileWatch(); 
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

        #endregion

        private void ListFileWatch ()
            {
                FileSystemWatcher Watcher = new FileSystemWatcher();
                Watcher.Path = Path.GetDirectoryName(this._itemListPath);
                Watcher.Filter = "Command.json";
                Watcher.NotifyFilter = NotifyFilters.LastWrite;
                Watcher.Changed += new FileSystemEventHandler(OnChanged);
                Watcher.EnableRaisingEvents = true;
            }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            this.LoadItems();
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
            //var filterExt = "*.xls";
            watcher.Filter = filterExt;





            //watcher.Changed += new FileSystemEventHandler(this.OnChanged);
            //watcher.Created += new FileSystemEventHandler(this.OnChanged);
            //watcher.Deleted += new FileSystemEventHandler(this.OnChanged);
            //watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            

            var m = new myFileSystemWatcher(watcher, foldComm);
            var ev = new FileSystemEventHandler((sender, e) => RunCommandEvent(sender, e, m, foldComm));
            m.setEvent(ev);
            

            this.myWatch.Add(m);
        }

        public void LoadItems()
        {
            try
            {
                this._itemList = FolderAndCommand.LoadItems(this._itemListPath);
            } catch (Exception e )
            {
                this._logger.WriteToFile(e.Message);
                return;
            }

            this.myWatch.ForEach(i =>
            {
                i.Dispose(); 
            });
            this.myWatch.Clear();
            
            this._itemList.ForEach(i =>
            {
                this.CreateFileWatcher(i);
            });

        }

        public void SaveItems(string path)
        {
            FolderAndCommand.SaveItems(path, this._itemList);
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
        }
    }
}
