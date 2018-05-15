//    Keepass Vault Sync Plugin
//    Copyright (C) 2018 Orange Applications for Business
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


namespace VaultSyncPlugin.IntegrationTests
{
    /// <summary>
    /// Test settings
    /// </summary>
    internal class TestSettings
    {
        /// <summary>
        /// Vault URL. Base URL of the backend, without any path
        /// </summary>
        public string VaultUrl { get; set; }

        /// <summary>
        /// Username for auth
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Password for auth
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Path for auth. May be 'username' for Vault auth, or the LDAP name for LDAP auth
        /// </summary>
        public string AuthPath { get; set; }

        /// <summary>
        /// The path to get secrets structure from.
        /// </summary>
        public string Path { get; set; }
    }
}