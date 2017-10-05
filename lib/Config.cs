using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pillar.Util;

namespace resultys.prospecta.lib
{
    public class Config
    {
        private static IniFile inifile;

        private static IniFile getInstance()
        {
            if (inifile == null)
            {
                inifile = new IniFile(String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), "config.ini"));
            }

            return inifile;
        }

        public static string read(string section, string setting)
        {
            var inifile = new IniFile(String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), "config.ini"));

            return inifile.Read(section, setting);
        }

    }
}
