# License

Developped at Orange Applications for Business under LGPL-2.1. See [LICENSE](LICENSE).

# How to use

1. Download the [latest PLGX file](https://github.com/Orange-OpenSource/keepass-vault-sync-plugin/releases) and copy it in the Keepass installation folder, in plugins directory
2. Open your database
3. Create an entry with name starting with `vault`. For example: `vault-personal-folder`
    * Username is the username used to authenticate on Vault
    * Password is the password used to authenticate on Vault
    * URL is the Vault Backend URL (port included). For example: `https://local-vault:8200`
    * In Advanced tab add the following String fields:
      * `auth` field contains the auth path. For basic Vault authentication, it should be `username`. For LDAP authentication, it should be the LDAP name.
      * `path` field contains the path to synchronize. Any secret in this path will be synchronized, recursively.
4. Click on *Tools -> Synchronize Vault entries*. Synchronization may take a while, since Vault API is really not designed for this kind of use case.
5. A folder named with your entry name followed by the date and time timestamp will be created. If the entry was previously synchronized, the previous folder won't be deleted.
6. You can save your database. The plugin won't do it for you.
7. For now, there is no error message in case of issue. Only the lack of synchronization will be a symptom of issue. It may be improved in future versions. If needed.

# Why these release names?

* Because release themes are cheap but are a small pleasure in release process
* Because it helps structuring releases
* Because why not?
* Because [Vault](https://en.wikipedia.org/wiki/Vault_\(comics\)), so [Release Theme](https://fr.wikipedia.org/wiki/Cat%C3%A9gorie:Super-vilain_Marvel)

# How to build

1. Get the dependencies listed [here](external/Readme.md)
1. Modify the version in both AssemblyInfo and KeepassPluginVersion.txt
1. Build the solution, targetting `Release PLGX`
1. The file is generated in VaultSyncPlugin/bin/ReleasePlgx/VaultSyncPlugin.plgx

# How to test

This part could be improved. For now, there are is one integration test, with minimal assertion.
On first run, a `secrets.json` file will be generated, containing the needed values to be modified for the test to run.
Since it contains sensitive data, this file is gitignored. But you should check regularly that it's not committed.

# Library used

* Keepass for plugin API
* [Vault.NET](https://github.com/Chatham/Vault.NET) for Vault API C# wrapping
* [PlgxTool](https://github.com/dlech/KeePassPluginDevTools) for PLGX generation
