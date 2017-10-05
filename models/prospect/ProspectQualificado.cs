using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class ProspectQualificado
    {
        public int id { get; set; }
        public int id_prospect { get; set; }
        public int id_empresa { get; set; }
        public int id_cliente { get; set; }
        public string telefone { get; set; }
        public string nome_contato { get; set; }
        public string email { get; set; }
        public string cnpj { get; set; }
        public bool telefone_pertence_empresa { get; set; }
        public string telefones { get; set; }
    }
}
