using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;
using resultys.prospecta.models;

namespace resultys.prospecta.worker
{
    public class TelefoneWorker : WorkerAbstract
    {
        public ProjectDelegate onQualificado { get; set; }
        public ProjectDelegate onNaoQualificado { get; set; }
        public ProjectDelegate onForceQualificado { get; set; }

        public TelefoneWorker() : base() {
            this.onWork += new ProjectDelegate(work);
        }
        
        public void work(Projeto projeto)
        {
            var higienize = new Higienize()
            {
                parametros = projeto.parametroPesquisa
            };

            projeto.empresas = higienize.clearEmpresas(projeto.empresas);
            projeto.empresas = higienize.clearTelefones(projeto.empresas);
            projeto.empresas = higienize.uniqueTelefones(projeto.empresas);
            projeto.empresas = higienize.removeTelefoneContador(projeto.empresas);

            if (!projeto.config.step_telefone)
            {
                foreach (var empresa in projeto.empresas)
                {
                    new resultys.prospecta.vendor.pabx.EmpresaChamada(empresa);
                }

                this.onForceQualificado(projeto);
                return;
            }

            if (projeto.empresas.Count == 0)
            {
                return;
            }

            var empresas = projeto.empresas;
            Log.WriteLine(String.Format("[TelefoneWork] projeto {0} selecionado com {1} empresas", projeto.getParteName(), empresas.Count));

            var central = new resultys.prospecta.vendor.pabx.Central() {
                validaSomenteTelefoneConfirmado = projeto.config.confirma_telefone
            };

            var empresasValidadas = central.validarTelefones(empresas);
            var empresasNaoValidadas = this.remover(empresas, empresasValidadas);

            var p1 = projeto.clone();
            var p2 = projeto.clone();
            var p3 = projeto.clone();

            p1.empresas = empresasValidadas;
            this.onQualificado(p1);

            p2.empresas = empresasNaoValidadas;
            this.onNaoQualificado(p2);

            p3.empresas = empresasValidadas.Concat(empresasNaoValidadas).ToList();

            Log.WriteLine(String.Format("[TelefoneWork] projeto {0} finalizado com {1} empresas validadas e {2} empresas não validadas", projeto.getParteName(), p1.empresas.Count, p2.empresas.Count));

            if (this.onSuccess != null) this.onSuccess(p3);
        }

        private List<Empresa> remover(List<Empresa> enq, List<Empresa> eq)
        {
            for (int i = 0; i < enq.Count; i++)
            {
                if (eq.Find(e => e.compareTo(enq[i])) != null)
                {
                    enq.Remove(enq[i]);
                    i--;
                }
            }

            return enq;
        }

    }
}
