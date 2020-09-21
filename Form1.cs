using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Permissions;
using System.Diagnostics;

using Newtonsoft.Json;

namespace FileWatcherPentahoRun
{
    public partial class Form1 : Form
    {

        private PentahoFileWatcher pentaho; 

        public Form1()
        {
            InitializeComponent();
            this.pentaho = new PentahoFileWatcher(@"C:\_DEV\C#\FileWatcherPentahoRunGit\PentahoRunOnFileChangeService\bin\Debug\Command.json", 20000); 
        }

        
        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".xls")
                    listBox1.Items.Add(file);
                    //this.pentaho.CreateFileWatcher(file);
            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }



        private void listBox2_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".bat")
                    listBox2.Items.Add(file);
            }

        }

        private void listBox2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }


        //get filefolder by dialog
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Title = "Browse XLS Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "xls",
                Filter = "xls files (*.xls;*.xlsx)|*.xls;*.xlsx",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var file = openFileDialog1.FileName;
                if (Path.GetExtension(file) == ".xls" || Path.GetExtension(file) == ".xlsx") 
                    listBox1.Items.Add(file);
                    //this.pentaho.CreateFileWatcher(file);
            }
        }

        //get command action by dialog
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Title = "Browse Command Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "bat",
                Filter = "bat files (*.bat)|*.bat",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                listBox2.Items.Add(openFileDialog1.FileName);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("asdasdasd");
        }


        private ILogger _logger;
        //get json file
        private void button1_Click_1(object sender, EventArgs e)
        {


            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }

            dynamic account = JsonConvert.DeserializeObject(fileContent);
            

        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.pentaho.AddItem(@"C:\_Dev", @"C:\_Dev\run.bat");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.pentaho.SaveItems(@"C:\_Dev\Command.json");
        }

        private void button4_Click(object sender, EventArgs e)
        {

            this._logger = new Logger(AppDomain.CurrentDomain.BaseDirectory);
            this.pentaho.setLogger(this._logger);
            this.pentaho.LoadItems();
        }
    }
}
