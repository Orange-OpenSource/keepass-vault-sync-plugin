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


using System.Collections.Generic;

namespace VaultSyncPlugin
{
    /// <summary>
    /// Does it really need comments? Self explained POCO.
    /// </summary>
    public class Secret
    {
        public Secret(string name, string user, string password, Dictionary<string, string> content)
        {
            this.Name = name;
            this.User = user;
            this.Password = password;
            this.Content = content;
        }

        public string Name { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }
        public Dictionary<string, string> Content { get; private set; }

        public string Url { get; set; }
    }
}