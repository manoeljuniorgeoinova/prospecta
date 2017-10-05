using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class Prospect
    {
        public int id { get; set; }
        public int id_projeto { get; set; }
        public string nome { get; set; }
        public DateTime data_geracao { get; set; }
        public int quantidade { get; set; }
        public string status { get; set; }
    }
}
