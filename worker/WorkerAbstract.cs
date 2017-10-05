using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using resultys.prospecta.lib;
using resultys.prospecta.models;

namespace resultys.prospecta.worker
{
    public delegate void ProjectDelegate(Projeto projeto);
    public delegate void ProjectEmptyDelegate(Fila fila);

    public class WorkerAbstract
    {
        protected Thread thread { get; set; }
        protected Projeto current { get; set; }

        public ProjectEmptyDelegate onEmpty { get; set; }
        public ProjectDelegate onSuccess { get; set; }
        public ProjectDelegate onWork { get; set; }
        public Fila fila { get; set; }

        public WorkerAbstract()
        {
            this.fila = new Fila();
        }

        public void start()
        {
            this.thread = new Thread(() =>
            {
                while (true)
                {
                    while (this.fila.count() == 0)
                    {
                        if (this.onEmpty != null) this.onEmpty(this.fila);
                        System.Threading.Thread.Sleep(5000);
                    }

                    this.current = this.fila.shift();
                    this.onWork(this.current);
                    this.current = null;
                }
            });

            this.thread.Start();
        }

        public bool existInWork(Projeto projeto)
        {
            if (this.fila.exist(projeto)) return true;
            if (this.current != null && this.current.id == projeto.id) return true;

            return false;
        }
    }
}
