using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.lib.timer
{
    public class Week
    {
        private string week { get; set; }

        public Week(string week)
        {
            this.week = week.Trim();
        }

        public bool isEqual(DateTime date)
        {
            if (this.week == "domingo" && date.DayOfWeek == DayOfWeek.Sunday) return true;
            if (this.week == "segunda" && date.DayOfWeek == DayOfWeek.Monday) return true;
            if (this.week == "terça" && date.DayOfWeek == DayOfWeek.Tuesday) return true;
            if (this.week == "quarta" && date.DayOfWeek == DayOfWeek.Wednesday) return true;
            if (this.week == "quinta" && date.DayOfWeek == DayOfWeek.Thursday) return true;
            if (this.week == "sexta" && date.DayOfWeek == DayOfWeek.Friday) return true;
            if (this.week == "sabado" && date.DayOfWeek == DayOfWeek.Saturday) return true;

            return false;
        }
    }
}
