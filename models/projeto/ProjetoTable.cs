using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.models
{
    public class ProjetoTable
    {
        public int id { get; set; }
        public int id_usuario { get; set; }
        public int id_cliente { get; set; }
        public string nome_perfil { get; set; }        
        public int quantidade_prospect { get; set; }
        public DateTime data_inicio { get; set; }
        public DateTime data_fim { get; set; }        
        public string status { get; set; }
        public bool forcar_prospeccao { get; set; }
        public string[] segmento { get; set; }
        public string[] estado { get; set; }
        public string[] municipio { get; set; }
        public string[] porte_empresa { get; set; }
        public string[] quantidade_funcionario { get; set; }
        public string[] regime_tributario { get; set; }
        public string[] palavra_chave_in { get; set; }
        public string[] palavra_chave_out { get; set; }
    }
}
