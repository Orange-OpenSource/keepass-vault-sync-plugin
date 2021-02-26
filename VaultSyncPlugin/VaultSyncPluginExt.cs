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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Security;
using Vault;
using Vault.Models;

namespace VaultSyncPlugin
{
    public sealed class VaultSyncPluginExt : Plugin
    {
        private IPluginHost host = null;
        private ToolStripSeparator separator;
        private ToolStripMenuItem menuItem;
        private SyncStatus syncStatus;
        private SyncStatusForm syncStatusForm;

        /// <summary>
        /// Override UpdateUrl for update checking
        /// </summary>
        public override string UpdateUrl
        {
            get
            {
                return "https://raw.githubusercontent.com/Orange-OpenSource/keepass-vault-sync-plugin/master/KeepassPluginVersion.txt";
            }
        }

        /// <summary>
        /// Input method for the keepass plugin.
        /// </summary>
        /// <param name="host">The keepass instance.</param>
        /// <returns>True is correctly initialized.</returns>
        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            this.host = host;

            // Hook on file opened to check entries in it.
            // Disabled because it takes too much time.
            // But could be re-enabled since the work is done asynchronously now.
            // Could be a setting
            // this.host.MainWindow.FileOpened += this.OnFileOpened;

            var menuItemCollection = this.host.MainWindow.ToolsMenu.DropDownItems;
            this.separator = new ToolStripSeparator();
            menuItemCollection.Add(this.separator);

            // Add menu item
            this.menuItem = new ToolStripMenuItem();
            this.menuItem.Text = "Synchronize Vault entries";
            this.menuItem.Click += this.OnMenuItemClick;
            menuItemCollection.Add(this.menuItem);

            this.syncStatus = new SyncStatus();

            return true;
        }

        /// <summary>
        /// Free resources at closing time.
        /// </summary>
        public override void Terminate()
        {
            // Remove menu items
            ToolStripItemCollection menuItemCollection = this.host.MainWindow.ToolsMenu.DropDownItems;
            this.menuItem.Click -= this.OnMenuItemClick;
            menuItemCollection.Remove(this.menuItem);
            menuItemCollection.Remove(this.separator);

            // Unsubscribe from file opened event
            this.host.MainWindow.FileOpened -= this.OnFileOpened;

            base.Terminate();
        }

        /// <summary>
        /// Called when menu item is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args</param>
        private void OnMenuItemClick(object sender, EventArgs e)
        {
            this.DoTheStuffAsync();
        }

        /// <summary>
        /// Called when file is opened so we can check vault entries in it.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args</param>
        private void OnFileOpened(object sender, FileOpenedEventArgs e)
        {
            this.DoTheStuffAsync();
        }

        /// <summary>
        /// Do the stuff needed. Single entry point for the plugin to do its work.
        /// </summary>
        private void DoTheStuffAsync()
        {
            if (this.syncStatusForm == null)
            {
                this.syncStatusForm = new SyncStatusForm(this.syncStatus);
            }
            
            this.syncStatusForm.Show();

            Task.Run(() =>
            {
                this.syncStatus.StartSync();

                // We synchronize vault entries
                this.SynchronizeVaultEntries(this.host.Database.RootGroup);

                // Then we merge modified data, and refresh the UI. Standard way to do it, but barely documented.
                this.host.Database.MergeIn(this.host.Database, PwMergeMethod.Synchronize);

                this.ExecuteInGuiThread(new Action(() => { this.host.MainWindow.UpdateUI(false, null, true, this.host.Database.RootGroup, true, null, true); }));

                // NOTE: We don't automatically save the database

                this.syncStatus.StopSync();
            });
        }

        /// <summary>
        /// Look for vault entries, recursively, and synchronize them.
        /// </summary>
        /// <param name="group">The group to synchronize (root group for the first call).</param>
        private void SynchronizeVaultEntries(PwGroup group)
        {
            foreach (var entry in group.Entries)
            {
                // If entry starts with "vault" it is considered vault entry to synchronize
                var name = this.GetKeepassEntryProperty(entry, PwDefs.TitleField);
                if (this.IsVaultEntry(name))
                {
                    this.syncStatus.AddLog(string.Format("Found vault entry {0}", name));
                    this.SynchronizeVaultEntry(entry);
                }
            }

            // Recursive call, avoiding scanning vault synchronized groups
            foreach (var subGroup in group.Groups)
            {
                if (!this.IsVaultEntry(subGroup.Name))
                {
                    this.SynchronizeVaultEntries(subGroup);
                }
            }
        }

        /// <summary>
        /// Synchronize this vault entry.
        /// 1. Does NOT delete previous folder
        /// 2. Creates new folder
        /// 3. Populates this folder with Vault data
        /// </summary>
        /// <param name="entry">The entry to synchronize.</param>
        private void SynchronizeVaultEntry(PwEntry entry)
        {
            var group = entry.ParentGroup;
            var entryName = this.GetKeepassEntryProperty(entry, PwDefs.TitleField);

            var vaultLogin = this.GetKeepassEntryPropertyDereferenced(entry, PwDefs.UserNameField);
            var vaultPassword = this.GetKeepassEntryPropertyDereferenced(entry, PwDefs.PasswordField);
            var vaultUrl = this.GetKeepassEntryPropertyDereferenced(entry, PwDefs.UrlField);
            var vaultAuthPath = this.GetKeepassEntryProperty(entry, "auth");
            var vaultPath = this.GetKeepassEntryProperty(entry, "path");

            if (!string.IsNullOrEmpty(vaultUrl) &&
                !string.IsNullOrEmpty(vaultLogin) &&
                !string.IsNullOrEmpty(vaultPassword) &&
                !string.IsNullOrEmpty(vaultPath) &&
                !string.IsNullOrEmpty(vaultAuthPath))
            {
                // Download secrets
                var secrets = this.DownloadSecrets(this.GetSyncGroupName(entryName), vaultUrl, vaultAuthPath, vaultLogin, vaultPassword, vaultPath);

                this.syncStatus.AddLog("Secrets fetched, will now inject them in the database");

                // Create new sync group to synchronize data.
                var newGroup = this.CreateGroup(this.GetSyncGroupName(entryName), secrets, group.IconId);

                group.AddGroup(newGroup, true);

                this.syncStatus.AddLog("Done.");
            }
        }

        /// <summary>
        /// Creates (recursively) a group from a downloaded folder
        /// </summary>
        /// <param name="groupName">The group name of the created group.</param>
        /// <param name="secrets">The downloaded secret folder.</param>
        /// <param name="icon">The icon</param>
        /// <returns>The created keepass group</returns>
        private PwGroup CreateGroup(string groupName, SecretFolder secrets, PwIcon icon)
        {
            var group = new PwGroup(true, true, groupName, icon);
            group.CreationTime = DateTime.Now;

            // Create subfolder to recreate tree structure
            foreach (var subFolder in secrets.Folders)
            {
                // Create a subfolder only if there's something in it
                // Avoids having a lot of empty folders we don't have access to in Vault
                if (subFolder.Folders.Count() > 0 || subFolder.Secrets.Count() > 0)
                {
                    this.syncStatus.AddLog(string.Format("Create group {0} in {1}", subFolder.Name, groupName));
                    var subGroup = this.CreateGroup(subFolder.Name, subFolder, icon);
                    group.AddGroup(subGroup, true);
                }
            }

            // Create entries at this level
            foreach (var secret in secrets.Secrets)
            {
                this.syncStatus.AddLog(string.Format("Create entry {0} in {1}", secret.Name, groupName));
                var entry = this.CreateEntry(secret);
                group.AddEntry(entry, true);
            }

            return group;
        }

        /// <summary>
        /// Creates an entry from a downloaded secret
        /// </summary>
        /// <param name="secret">The downloaded secret</param>
        /// <returns>The keepass entry</returns>
        private PwEntry CreateEntry(Secret secret)
        {
            var entry = new PwEntry(true, true);
            entry.Strings.Set(PwDefs.TitleField, new ProtectedString(false, secret.Name));
            if (!string.IsNullOrEmpty(secret.User))
            {
                entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(false, secret.User));
            }
            if (!string.IsNullOrEmpty(secret.Password))
            {
                entry.Strings.Set(PwDefs.PasswordField, new ProtectedString(false, secret.Password));
            }
            if (!string.IsNullOrEmpty(secret.Url))
            {
                entry.Strings.Set(PwDefs.UrlField, new ProtectedString(false, secret.Url));
            }
            foreach (var item in secret.Content)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    entry.Strings.Set(item.Key, new ProtectedString(false, item.Value));
                }
            }
            return entry;
        }

        /// <summary>
        /// Download the secrets available in a specific Vault, with credentials, and a specific path
        /// </summary>
        /// <param name="groupName">The group name to give to the created group</param>
        /// <param name="vaultUrl">The Vault URL</param>
        /// <param name="vaultAuthPath">The Vault auth path. Can be a LDAP path, or "userpass" for Vault standard authentication</param>
        /// <param name="vaultUsername">The Vault username</param>
        /// <param name="vaultPassword">The Vault password</param>
        /// <param name="vaultPath">The Vault path</param>
        /// <returns></returns>
        private SecretFolder DownloadSecrets(
            string groupName,
            string vaultUrl,
            string vaultAuthPath,
            string vaultUsername,
            string vaultPassword,
            string vaultPath)
        {
            var client = new SynchronousVaultClient(new Uri(vaultUrl), vaultAuthPath, vaultUsername, vaultPassword, this.syncStatus);
            var secretFolder = client.GetSecrets(vaultPath).Result;
            return secretFolder;
        }

        /// <summary>
        /// Helper method to get keepass entry property as string, following references
        /// </summary>
        /// <param name="entry">The keepass entry</param>
        /// <param name="field">The field</param>
        /// <returns>The dereferenced value for this field.</returns>
        private string GetKeepassEntryPropertyDereferenced(PwEntry entry, string field)
        {
            var value = this.GetKeepassEntryProperty(entry, field);
            this.ExecuteInGuiThread(new Action(() => { value = SprEngine.Compile(value, new SprContext(entry, this.host.Database, SprCompileFlags.All)); }));
            return value;
        }

        /// <summary>
        /// Helper method to get keepass entry property as string
        /// </summary>
        /// <param name="entry">The keepass entry</param>
        /// <param name="field">The field</param>
        /// <returns>The value for this field.</returns>
        private string GetKeepassEntryProperty(PwEntry entry, string field)
        {
            return entry.Strings.GetSafe(field).ReadString();
        }

        /// <summary>
        /// Gets a standardized group name from an entry name, to create a group for synchronizing data for the entry
        /// </summary>
        /// <param name="entryName">The entry containing Vault data to synchronize.</param>
        /// <returns>The group name.</returns>
        private string GetSyncGroupName(string entryName)
        {
            return string.Format("{0}-{1}", entryName, DateTime.Now.ToString("yyyyMMdd-HHmm"));
        }

        /// <summary>
        /// Execute a delegate in the GUI thread when manipulating GUI objects
        /// </summary>
        /// <param name="delegate">The delegate to execute</param>
        private void ExecuteInGuiThread(Delegate @delegate)
        {
            if (this.host.MainWindow.InvokeRequired)
            {
                this.host.MainWindow.Invoke(@delegate);
            }
            else
            {
                @delegate.DynamicInvoke();
            }
        }

        /// <summary>
        /// Returns true if the entry is a vault entry to synchronize.
        /// </summary>
        /// <param name="entryName">entry name</param>
        /// <returns>true if this is a vault entry</returns>
        private bool IsVaultEntry(string entryName)
        {
            return entryName.StartsWith("vault");
        }
    }
}