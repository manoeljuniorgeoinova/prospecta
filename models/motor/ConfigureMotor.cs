using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.models
{
    public class ConfigureMotor
    {
        public int id { get; set; }
        public int id_projeto { get; set; }

        public bool step_receita { get; set; }
        public bool step_site { get; set; }
        public bool step_telefone { get; set; }
        public bool confirma_telefone { get; set; }

        public int validade_receita { get; set; }
        public int validate_site { get; set; }

        public bool base_cnpj { get; set; }
        public int taxa_busca_prospects { get; set; }
        public int taxa_prospeccao { get; set; }
    }
}
