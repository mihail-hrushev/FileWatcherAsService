namespace PentahoRunOnFileChangeService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.process = new System.ServiceProcess.ServiceProcessInstaller();

            this.process.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.process.Password = null;
            this.process.Username = null;

            this.serviceAdmin = new System.ServiceProcess.ServiceInstaller();
            this.serviceAdmin.StartType = System.ServiceProcess.ServiceStartMode.Manual;
            this.serviceAdmin.ServiceName = "PentahoRunOnFileChangeService";
            this.serviceAdmin.DisplayName = "Pentaho File Watch Run";

            // 
            // serviceProcessInstaller1
            // 

            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.process,
            this.serviceAdmin});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller process;
        private System.ServiceProcess.ServiceInstaller serviceAdmin;
    }
}