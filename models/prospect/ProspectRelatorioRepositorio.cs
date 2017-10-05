using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class ProspectRelatorioRepositorio
    {
        public static void insert(ProspectRelatorio relatorio)
        {
            Repositorio.insert("prospect.prospect_relatorio", "id", relatorio);
        }
    }
}
