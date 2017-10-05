using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class ProspectDescartado
    {
        public int id { get; set; }
        public string cnpj { get; set; }
        public int id_projeto { get; set; }
        public DateTime create_at { get; set; }
    }
}
