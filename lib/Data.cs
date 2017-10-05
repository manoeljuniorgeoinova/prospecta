using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.lib
{
    public class Data
    {
        private static DateTime Null = new DateTime(1, 1, 1, 0, 0, 0);

        public static bool isNotNull(DateTime data)
        {
            return !isNull(data);
        }

        public static bool isNull(DateTime data)
        {
            return data.Year == Null.Year && data.Month == Null.Month && data.Day == Null.Day;
        }
    }
}
