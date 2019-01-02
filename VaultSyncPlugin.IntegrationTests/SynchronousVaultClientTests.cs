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


using Newtonsoft.Json;
using NFluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace VaultSyncPlugin.IntegrationTests
{
    [TestFixture]
    public class SynchronousVaultClientTests
    {
        private TestSettings settings;

        [SetUp]
        public void Setup()
        {
            var file = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\secrets.json");
            
            if (File.Exists(file))
            {
                var configuration = File.ReadAllText(file);
                this.settings = JsonConvert.DeserializeObject<TestSettings>(configuration);
            }
            else
            {
                var defaultSettings = new TestSettings
                {
                    VaultUrl = "http://localhost:8200",
                    AuthPath = "userpass",
                    Path = "my-secrets/",
                    User = "user",
                    Password = "password",
                };

                var content = JsonConvert.SerializeObject(defaultSettings);
                File.WriteAllText(file, content);

                Assert.Fail(@"You need to update your Vault data in secrets.json. It WON'T be commited to git (but you should check anyway).");
            }
        }

        [Test]
        public void Top_level_access_should_be_OK_by_credentials()
        {
            var client = new SynchronousVaultClient(new Uri(this.settings.VaultUrl), this.settings.AuthPath, this.settings.User, this.settings.Password);
            SecretFolder structure;
            try
            {
                structure = client.GetSecrets(this.settings.Path).Result;
                Check.That(structure.Folders.Count()).IsStrictlyGreaterThan(0);
                Check.That(structure.Secrets.Count()).IsStrictlyGreaterThan(0);
            }
            catch (Exception ex)
            {
                Assert.Fail(@"Access failed. You should check that your credentials are valid.");
            }
        }
    }
}
