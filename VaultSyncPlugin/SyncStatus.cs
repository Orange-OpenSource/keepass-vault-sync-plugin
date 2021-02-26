using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultSyncPlugin
{
    public class SyncStatus
    {
        public event EventHandler<string> NewLog;

        public event EventHandler Started;

        public event EventHandler Ended;

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
            if (this.NewLog != null)
            {
                this.NewLog(this, log);
            }
        }
    }
}
