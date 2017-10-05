using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.lib.timer
{

    public class Timer
    {
        public List<Intervalo> intervalos { get; set; }
        public List<Data> datas { get; set; }
        public List<Week> daysOfWeek { get; set; }

        public void wait()
        {
            try
            {
                this.parse();

                while (true)
                {
                    if (this.isDiaBloqueado())
                    {
                        System.Threading.Thread.Sleep(1000);
                        continue;
                    }

                    if (this.isDataBloquadas())
                    {
                        System.Threading.Thread.Sleep(1000);
                        continue;
                    }

                    if (this.isHorarioDisponivel()) return;

                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch { }
        }

        private bool isDataBloquadas()
        {
            foreach (var data in this.datas)
            {
                if (data.isEqual(DateTime.Now)) return true;
            }

            return false;
        }

        private bool isHorarioDisponivel()
        {
            foreach (var interval in this.intervalos)
            {
                if (interval.isBetween(DateTime.Now)) return true;
            }

            return false;
        }

        private bool isDiaBloqueado()
        {
            foreach (var day in this.daysOfWeek)
            {
                if (day.isEqual(DateTime.Now)) return true;
            }

            return false;
        }
        
        private void parse()
        {
            var horarioDisponivel = Config.read("Timer", "horarios_disponiveis");
            var datasIndisponiveis = Config.read("Timer", "datas_bloqueadas");
            var daysOfWeek = Config.read("Timer", "dias_bloqueados");

            var intervalos = horarioDisponivel.Split(',');
            var datas = datasIndisponiveis.Split(',');
            var weeks = daysOfWeek.Split(',');

            this.intervalos = new List<Intervalo>();
            this.datas = new List<Data>();
            this.daysOfWeek = new List<Week>();

            foreach (var week in weeks)
            {
                this.daysOfWeek.Add(new Week(week));
            }

            foreach (var intervalo in intervalos)
            {
                this.intervalos.Add(new Intervalo(intervalo));
            }

            foreach (var data in datas)
            {
                this.datas.Add(new Data(data));
            }
        }
    }
}
