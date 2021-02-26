using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VaultSyncPlugin
{
    public partial class SyncStatusForm : Form
    {
        private SyncStatus syncStatus;

        public SyncStatusForm(SyncStatus syncStatus)
        {
            this.syncStatus = syncStatus;
            InitializeComponent();

            this.logBox.DataBindings.Add("Text",
                            this.syncStatus,
                            "Logs",
                            false,
                            DataSourceUpdateMode.OnPropertyChanged);
            this.syncStatus.Started += SyncStatus_Started;
            this.syncStatus.Ended += SyncStatus_Ended;
        }

        private void SyncStatus_Ended(object sender, EventArgs e)
        {
            this.progressBar.Invoke((MethodInvoker)delegate
            {
                this.progressBar.Style = ProgressBarStyle.Continuous;
            });
        }

        private void SyncStatus_Started(object sender, EventArgs e)
        {
            this.progressBar.Invoke((MethodInvoker)delegate
            {
                this.progressBar.Style = ProgressBarStyle.Marquee;
            });
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
