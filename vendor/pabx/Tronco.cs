using System.Collections.Generic;
using System.Linq;
using System.Threading;

using resultys.prospecta.models;

namespace resultys.prospecta.vendor.pabx
{
    public class Tronco
    {
        private List<Chamada> chamadas { get; set; }

        private static object objlock = new object();
        private static int totalCanais = int.Parse(resultys.prospecta.lib.Config.read("CentralTelefonica", "quantidade_ramais"));
        private static int _totalDisponivel = int.Parse(resultys.prospecta.lib.Config.read("CentralTelefonica", "quantidade_ramais"));
        public static int totalDisponivel {
            get {
                lock (objlock)
                {
                    return _totalDisponivel;
                }
            }
            set
            {
                lock (objlock)
                {
                    _totalDisponivel = value;
                }
            }
        }

        public Tronco()
        {
            this.chamadas = new List<Chamada>();
        }

        public void add(Chamada chamada)
        {
            this.chamadas.Add(chamada);
        }

        public void clear()
        {
            this.chamadas.Clear();
        }

        public void ligar()
        {
            var callLimit = int.Parse(resultys.prospecta.lib.Config.read("CentralTelefonica", "quantidade_ramais"));
            var total = (int)this.chamadas.Count / callLimit;
            var telefones = this.chamadas.ToList();

            for (int pageCounter = 0; pageCounter < total; pageCounter++) {
                start(telefones, callLimit);
            }

            start(telefones, telefones.Count);
        }

        private void start(List<Chamada> telefones, int length)
        {
            var threads = new List<Thread>();
            for (int i = 0; i < length; i++)
            {
                var thread = new Thread(call);

                threads.Add(thread);
            }

            for (int i = 0; i < length; i++)
            {
                threads[i].Start(telefones[i]);
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            for (int i = 0; i < length; i++)
            {
                telefones.Remove(telefones[0]);
            }
        }

        private static void call(object parameter)
        {
            var timer = new resultys.prospecta.lib.timer.Timer();
            timer.wait();

            var validaTelefone = parameter as Chamada;
            validaTelefone.call();
        }
    }
}
