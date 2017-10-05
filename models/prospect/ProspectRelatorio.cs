using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class ProspectRelatorio
    {
        public int id { get; set; }
        public int id_prospect { get; set; }

        public int total_prospectado { get; set; }
        public int total_qualificado { get; set; }
        public int total_descartado { get; set; }
    }
}
