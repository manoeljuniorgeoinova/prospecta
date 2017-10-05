using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.lib.timer
{
    public class Data
    {
        public DateTime date { get; set; }

        public Data(string data)
        {
            this.parse(data.Trim());
        }

        public void parse(string data)
        {
            var d = data.ToLower();            
            var p = data.Split('/');

            this.date = new DateTime(int.Parse(p[2]), int.Parse(p[1]), int.Parse(p[0]), 0, 0, 0);
        }

        public bool isEqual(DateTime date)
        {
            return date.Day == this.date.Day && date.Month == this.date.Month && date.Year == this.date.Year;
        }

    }
}
