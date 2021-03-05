//    Keepass Vault Sync Plugin
//    Copyright (C) 2018 Orange Business Services
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
//    USA
//

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
