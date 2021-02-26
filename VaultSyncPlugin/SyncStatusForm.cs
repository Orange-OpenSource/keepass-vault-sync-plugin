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

            this.syncStatus.NewLog += SyncStatus_NewLog;
            this.syncStatus.Started += SyncStatus_Started;
            this.syncStatus.Ended += SyncStatus_Ended;

            this.progressBar.MarqueeAnimationSpeed = 30;
        }

        private void SyncStatus_NewLog(object sender, string log)
        {
            this.logBox.AppendText(log);
            this.logBox.AppendText(Environment.NewLine);
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
