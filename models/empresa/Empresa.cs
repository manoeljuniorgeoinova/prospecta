using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class Empresa
    {
        public int id { get; set; }
        public string cnpj { get; set; }
        public string razao_social { get; set; }
        public string nome_fantasia { get; set; }
        public string uf { get; set; }
        public string municipio { get; set; }
        public string email { get; set; }
        public string cnae_primario_codigo { get; set; }
        public string situacao { get; set; }
        public string[] socio { get; set; }

        public DateTime data_ultima_atualizacao { get; set; }
        public DateTime data_ultima_atualizacao_site { get; set; }

        [PetaPoco.Ignore] public bool wasUpdatedReceita { get; set; }
        [PetaPoco.Ignore] public bool wasUpdatedSite { get; set; }

        [PetaPoco.Ignore] public List<Telefone> telefones { get; set; }
        [PetaPoco.Ignore] public List<Socio> socios { get; set; }
        [PetaPoco.Ignore] public EmpresaContato contato { get; set; }

        public string getNomePrincipalSocio()
        {
            if (this.socios == null) return "Não Informando";
            if (this.socios.Count == 0) return "Não Informando";

            var p = this.socios[0].nome.Replace(" - ", "").Replace("-", "").Replace("EPP", "").Replace("LTDA", "").Split(' ');

            if (p.Length == 0) return "Não Informando";
            if (p.Length == 1) return p[0];

            var firstName = p[0];
            var lastName = p[1];

            if (lastName.ToUpper() == "ME") lastName = "";

            return String.Format("{0} {1}", firstName, lastName);
        }

        public string getRawCnpj()
        {
            return this.cnpj.Replace("-", "").Replace(".", "").Replace("/", "");
        }

        public bool compareTo(Empresa empresa)
        {
            return this.getRawCnpj() == empresa.getRawCnpj();
        }
    }
}
