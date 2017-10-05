using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.lib.timer
{
    public class Intervalo
    {
        public Horario minimo { get; set; }
        public Horario maximo { get; set; }

        public Intervalo(string intervalo)
        {
            this.parse(intervalo);
        }

        public bool isBetween(DateTime time)
        {
            var dtMinimo = new DateTime(1, 1, 1, this.minimo.hora, this.minimo.minutos, 0);
            var dtMaximo = new DateTime(1, 1, 1, this.maximo.hora, this.maximo.minutos, 0);

            var dt = new DateTime(1, 1, 1, time.Hour, time.Minute, 0);

            if (dt.TimeOfDay >= dtMinimo.TimeOfDay && dt.TimeOfDay < dtMaximo.TimeOfDay) return true;
            else return false;
        }

        public void parse(string intervalo)
        {
            var p = intervalo.Split('-');

            this.minimo = new Horario(p[0]);
            this.maximo = new Horario(p[1]);
        }
    }
}
