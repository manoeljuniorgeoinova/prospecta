using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.worker;
using resultys.prospecta.models;

namespace resultys.prospecta.lib
{
    public class Fila
    {
        protected object obj { get; set; }
        protected List<Projeto> projetos { get; set; }

        public Fila()
        {
            this.obj = new object();
            this.projetos = new List<Projeto>();
        }

        public void each(ProjectDelegate lambda)
        {
            lock (this.obj)
            {
                for (int i = 0; i < this.projetos.Count; i++)
                {
                    lambda(this.projetos[i]);
                }
            }
        }

        public void exclusive(ProjectDelegate lambda)
        {
            lock (this.obj)
            {
                lambda(null);
            }
        }

        public void add(Projeto projeto)
        {
            lock (this.obj)
            {
                this.projetos.Add(projeto);
            }
        }

        public Projeto shift()
        {
            Projeto p = null;

            lock (this.obj)
            {
                if (this.projetos.Count > 0)
                {
                    p = this.projetos[0];
                    this.projetos.RemoveAt(0);
                }
            }

            return p;
        }

        public bool exist(Projeto projeto)
        {
            var b = false;

            lock (this.obj)
            {
                b = this.projetos.Exists(p => p.id == projeto.id);
            }

            return b;
        }

        public int count()
        {
            var count = 0;
        
            lock (this.obj)
            {
                count = this.projetos.Count;
            }

            return count;
        }
    }
}
