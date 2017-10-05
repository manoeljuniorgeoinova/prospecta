using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.lib.timer
{
    public class Horario
    {
        public int hora { get; set; }
        public int minutos { get; set; }

        public Horario(string horario)
        {
            this.parse(horario);
        }

        public void parse(string horario)
        {
            var p = horario.Split(':');

            this.hora = int.Parse(p[0]);
            this.minutos = int.Parse(p[1]);
        }
    }
}
