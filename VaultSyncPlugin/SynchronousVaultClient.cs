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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vault;
using Vault.Models;
using Vault.Models.Auth.UserPass;

namespace VaultSyncPlugin
{
    /// <summary>
    ///  Synchronous wrapper vault client.
    /// </summary>
    public class SynchronousVaultClient
    {
        VaultClient client;
        private string authPath;
        private string vaultLogin;
        private string vaultPassword;
        private string token;

        /// <summary>
        ///  Creates a client from credentials
        /// </summary>
        /// <param name="vaultUrl">The Vault URL</param>
        /// <param name="authPath">The Vault auth path. Can be userpass for standard auth method, or the LDAP path in case of LDAP auth.</param>
        /// <param name="vaultLogin">The login</param>
        /// <param name="vaultPassword">The password</param>
        public SynchronousVaultClient(Uri vaultUrl, string authPath, string vaultLogin, string vaultPassword)
        {
            // Disable SSL checks, for self-signed certificates.
            // TODO could be a plugin setting
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            this.authPath = authPath;
            this.vaultLogin = vaultLogin;
            this.vaultPassword = vaultPassword;
            this.client = new VaultClient(vaultUrl);
        }

        /// <summary>
        /// Download the secrets from a path (following the '.../v1/secret/')
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The secret tree structure.</returns>
        public async Task<SecretFolder> GetSecrets(string path)
        {
            var folder = new SecretFolder(this.GetLastFolder(path));

            // Gets the list of secrets for this path
            IEnumerable<string> keyList = new List<string>(); ;
            try
            {
                keyList = await this.GetSecretList(path);
            }
            catch (VaultRequestException ex)
            {
                // May be connection issue, or forbidden. We just continue.
            }

            foreach (var key in keyList)
            {
                if (SecretFolder.IsFolder(key))
                {
                    // Resursive call
                    folder.AddFolder(await this.GetSecrets(path + "/" + key));
                }
                else
                {
                    // Adds secret
                    folder.AddSecret(this.GetSecret(path + "/" + key));
                }
            }

            return folder;
        }

        /// <summary>
        /// Gets the secret list in a path. Contains folders and items. Only difference is folder is ending with a trailing '/'.
        /// </summary>
        /// <param name="path">The path (after .../v1/secret/')</param>
        /// <returns>The secret list.</returns>
        private async Task<List<string>> GetSecretList(string path)
        {
            try
            {
                this.CheckToken();
                return await Task.FromResult(client.Secret.List(path).Result.Data.Keys);
            }
            catch (Exception ex)
            {
                // May be unauthorized access
                return new List<string>();
            }
        }

        /// <summary>
        /// Checks if the token is downloaded, or else download it.
        /// </summary>
        private void CheckToken()
        {
            if (string.IsNullOrEmpty(this.token))
            {
                this.token = this.GetToken(this.authPath, this.vaultLogin, this.vaultPassword).Result;
                this.client.Token = this.token;
            }

        }

        /// <summary>
        ///  Downloads the token
        /// </summary>
        /// <param name="authPath">The auth path</param>
        /// <param name="user">The username</param>
        /// <param name="password">The password</param>
        /// <returns></returns>
        private async Task<string> GetToken(string authPath, string user, string password)
        {
            
            var loginRequest = new LoginRequest
            {
                Password = password
            };

            return await Task.FromResult<string>(client.Auth.Write<LoginRequest, NoData>(authPath + "/login/" + user, loginRequest).Result.Auth.ClientToken);
        }

        /// <summary>
        /// Download a secret from a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Secret GetSecret(string path)
        {
            Secret secret;

            try
            {
                var data = Task.FromResult(client.Secret.Read<Dictionary<string, string>>(path).Result.Data).Result;
                secret = this.SecretFromDictionary(this.GetLastFolder(path), data);
            }
            catch (Exception ex)
            {
                // Creates the entry with the exception message. Could help analyzing when failing.
                secret = new Secret(this.GetLastFolder(path), ex.Message, string.Empty, new Dictionary<string, string>());
            }

            return secret;
        }

        /// <summary>
        /// Creates a secret from a dictionary. Since Vault secrets are not structured, it can fail. In this case there's a message in the field.
        /// </summary>
        /// <param name="name">The secret name</param>
        /// <param name="data">The data to create the secret from.</param>
        /// <returns></returns>
        private Secret SecretFromDictionary(string name, Dictionary<string, string> data)
        {
            var user = this.UserFromDictionary(data);
            var password = this.PasswordFromDictionary(data);
            var url = this.UrlFromDictionary(data);
            var secret = new Secret(name, user, password, data);
            secret.Url = url;
            return secret;
        }

        /// <summary>
        /// Yes. That's the only thing we can do about that.
        /// </summary>
        /// <param name="data">The data to look up.</param>
        /// <returns>The value, or error message if not found</returns>
        private string PasswordFromDictionary(Dictionary<string, string> data)
        {
            string[] keyList = { "password", "pass", "passwd" };
            string value;

            if (this.TryGetValue(keyList, data, out value))
            {
                return value;
            }

            return "Password field not found";
        }

        /// <summary>
        /// Yes. That's the only thing we can do about that.
        /// </summary>
        /// <param name="data">The data to look up.</param>
        /// <returns>The value, or error message if not found</returns>
        private string UserFromDictionary(Dictionary<string, string> data)
        {
            string[] keyList = { "login", "user", "username" };
            string value;

            if (this.TryGetValue(keyList, data, out value))
            {
                return value;
            }

            return "User field not found";
        }

        /// <summary>
        /// Yes. That's the only thing we can do about that.
        /// </summary>
        /// <param name="data">The data to look up.</param>
        /// <returns>The value, or error message if not found</returns>
        private string UrlFromDictionary(Dictionary<string, string> data)
        {
            string[] keyList = { "url" };
            string value;

            if (this.TryGetValue(keyList, data, out value))
            {
                return value;
            }

            return string.Empty;
        }

        /// <summary>
        /// Helper method. Try to find a value from a list of potential keys. Case insensitive.
        /// </summary>
        /// <param name="keyList">The list of keys to use</param>
        /// <param name="data">The dictionary to look up</param>
        /// <param name="value">The value, if found.</param>
        /// <returns>true if the value is found.</returns>
        private bool TryGetValue(string[] keyList, Dictionary<string, string> data, out string value)
        {
            foreach (var key in keyList)
            {
                var item = data.FirstOrDefault(i => i.Key.ToLower().Equals(key));
                if (!string.IsNullOrEmpty(item.Key))
                {
                    value = item.Value;
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }

        /// <summary>
        /// Get the last folder of a path
        /// </summary>
        /// <param name="fullpath">The path</param>
        /// <returns>The last folder</returns>
        private string GetLastFolder(string fullpath)
        {
            var name = fullpath;
            if (SecretFolder.IsFolder(name))
            {
                // Remove the trailing /
                name = name.Substring(0, name.Length - 1);
            }

            return name.Substring(name.LastIndexOf("/") + 1);
        }
    }
}
