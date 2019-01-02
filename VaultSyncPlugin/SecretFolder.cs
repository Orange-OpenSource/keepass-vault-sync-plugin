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


using System.Collections;
using System.Collections.Generic;

namespace VaultSyncPlugin
{
    /// <summary>
    /// Does it really need comments? Self explained POCO.
    /// </summary>
    public class SecretFolder
    {
        private List<SecretFolder> folders = new List<SecretFolder>();
        private List<Secret> secrets = new List<Secret>();

        public SecretFolder(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public IEnumerable<SecretFolder> Folders
        {
            get { return this.folders; }
        }

        public IEnumerable<Secret> Secrets
        {
            get { return this.secrets; }
        }

        /// <summary>
        /// Adds a sub folder in the folder
        /// </summary>
        /// <param name="folder">The folder</param>
        public void AddFolder(SecretFolder folder)
        {
            this.folders.Add(folder);
        }

        /// <summary>
        /// Adds a secret in the folder
        /// </summary>
        /// <param name="secret">The secret</param>
        public void AddSecret(Secret secret)
        {
            this.secrets.Add(secret);
        }

        /// <summary>
        /// Returns true if the path is a folder and not a secret.
        /// Vault API seems to return "/" at the end of the path 
        /// in case of folder.
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>True if is a folder</returns>
        public static bool IsFolder(string path)
        {
            return path.EndsWith("/");
        }
    }
}