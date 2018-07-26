using InstallerUI.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallerUI.Services
{
    [Export(typeof(IMySQLService))]
    public class MySQLService : IMySQLService
    {
        public void InitServer(int port)
        {
            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    RedirectStandardInput = false,
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = Environment.Is64BitOperatingSystem ?
                                String.Format(@"/C """"C:\Program Files (x86)\MySQL\MySQL Installer for Windows\MySQLInstallerConsole.exe"" community install server;5.7.22;X64:*:servertype=Server;servicename=MySqlSTATSports;port={0};datadir=""C:\mysql\statsports\data"";passwd=admin -silent"" ", port.ToString())
                                :
                                String.Format(@"/C """"C:\Program Files\MySQL\MySQL Installer for Windows\MySQLInstallerConsole.exe"" community install server;5.7.22;X86:*:servertype=Server;servicename=MySqlSTATSports;port={0};datadir=""C:\mysql\statsports\data"";passwd=admin -silent"" ", port.ToString())
                }
            };
            p.Start();
            p.WaitForExit();
        }

    }
}
