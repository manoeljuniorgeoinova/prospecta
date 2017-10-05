using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.models
{
    public class Empresa
    {
        public string nome { get; set; }

        public Empresa(string nome)
        {
            this.nome = nome;
        }

        public string raw()
        {
            return this.clear(this.nome);
        }
    }
}
