using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;
using resultys.prospecta.models;

namespace resultys.prospecta.worker
{
    public class SiteWorker : WorkerAbstract
    {        
        public SiteWorker() : base() {
            this.onWork += new ProjectDelegate(work);
        }

        public void work(Projeto projeto)
        {
            if (!projeto.config.step_site)
            {
                this.onSuccess(projeto);
                return;
            }

            var restante = int.Parse(Config.read("Break", "restante_site"));
            var projetos = BreakProject.atEmpresasAtualizadasSite(projeto);
            var projetoDesatualizado = projetos[0];
            var projetoAtualizado = projetos[1];
            var isConcat = true;

            if (projetoDesatualizado.empresas.Count == 0 && projetoAtualizado.empresas.Count == 0)
            {
                return;
            }

            if (projetoDesatualizado.empresas.Count == 0)
            {
                this.onSuccess(projetoAtualizado);
                return;
            }

            Log.WriteLine(String.Format("[SiteWork] projeto {0} selecionado com : {1} empresas atualizadas e {2} empresas desatualizadas", projetoDesatualizado.getParteName(), projetoAtualizado.empresas.Count, projetoDesatualizado.empresas.Count));

            if (projetoDesatualizado.empresas.Count > restante)
            {
                this.onSuccess(projetoAtualizado);
                isConcat = false;
            }

            System.Threading.Thread.Sleep(10000);

            var empresas = projetoDesatualizado.empresas;
            var telefone = new resultys.prospecta.vendor.search.Telefone();
            empresas = telefone.pesquisar(empresas);

            projetoDesatualizado.empresas = isConcat ? empresas.Concat(projetoAtualizado.empresas).ToList() : empresas;

            Log.WriteLine(String.Format("[SiteWork] projeto {0} finalizado com {1} empresas atualizadas", projetoDesatualizado.getParteName(), empresas.Count));

            this.onSuccess(projetoDesatualizado);
        }
    }
}
