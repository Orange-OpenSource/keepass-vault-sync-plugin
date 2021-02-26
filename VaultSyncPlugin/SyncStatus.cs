using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultSyncPlugin
{
    public class SyncStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Started;

        public event EventHandler Ended;

        private string logs = string.Empty;

        public string Logs { 
            get { return this.logs; }
            private set
            {
                this.logs = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Logs"));
                }
            }
        }

        public void StartSync()
        {
            if (this.Started != null)
            {
                this.Started(this, new EventArgs());
            }
        }

        public void StopSync()
        {
            if (this.Ended != null)
            {
                this.Ended(this, new EventArgs());
            }
        }

        public void AddLog(string log)
        {
            this.Logs = this.Logs + Environment.NewLine + log;
        }
    }
}
