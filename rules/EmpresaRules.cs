using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.models;

namespace resultys.prospecta.rules
{
    public class EmpresaRules
    {

        public List<Empresa> buscarEmpresas(Projeto projeto)
        {
            if (projeto.config.base_cnpj) return EmpresaRepositorio.searchByCnpjQualificado(projeto.id);

            return EmpresaRepositorio.search(projeto.parametroPesquisa, projeto.parametroPesquisa.totalEmpresaRestantes * projeto.config.taxa_busca_prospects);
        }

    }
}
