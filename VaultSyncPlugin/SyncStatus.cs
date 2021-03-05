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
