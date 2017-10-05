using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using resultys.prospecta.lib;
using resultys.prospecta.models;

namespace resultys.prospecta.worker
{
    public class TelefoneWorker : WorkerAbstract
    {
        public ProjetoDelegate onQualificado { get; set; }
        public ProjetoDelegate onNaoQualificado { get; set; }
        public ProjetoDelegate onForceQualificado { get; set; }

        private object objlock { get; set; }
        private int totalCanaisDisponivel { get; set; }

        public TelefoneWorker() : base() {
            this.onPreProcess += new FilaDelegate(preWork);
            this.onWork += new ProjetoDelegate(work);

            this.totalCanaisDisponivel = int.Parse(resultys.prospecta.lib.Config.read("CentralTelefonica", "quantidade_ramais"));
            this.objlock = new object();
        }

        private bool exitCanalDisponivel()
        {
            var b = true;

            lock (this.objlock)
            {
                if (this.totalCanaisDisponivel == 0) b = false;
            }

            return b;
        }

        public void preWork(Fila fila)
        {
            while (!this.exitCanalDisponivel()) System.Threading.Thread.Sleep(100);

            lock (this.objlock)
            {
                var projeto = fila.shift();
                var higienize = new Higienize()
                {
                    parametros = projeto.parametroPesquisa
                };

                projeto.empresas = higienize.clearEmpresas(projeto.empresas);
                projeto.empresas = higienize.clearTelefones(projeto.empresas);
                projeto.empresas = higienize.uniqueTelefones(projeto.empresas);
                projeto.empresas = higienize.removeTelefoneContador(projeto.empresas);

                if (projeto.empresas.Count > this.totalCanaisDisponivel) {
                    var projetos = BreakProject.at(projeto, this.totalCanaisDisponivel);
                    var p1 = projetos[0];
                    var p2 = projetos[1];

                    fila.addFirst(p1);
                    fila.add(p2);

                    this.totalCanaisDisponivel -= p1.empresas.Count;
                }else
                {
                    fila.addFirst(projeto);
                    this.totalCanaisDisponivel -= projeto.empresas.Count;
                }                
            }

        }
        
        public void work(Projeto projeto)
        {
            var totalEmpresaLotadas = projeto.empresas.Count;

            if (!projeto.config.step_telefone)
            {
                //fix corrigir
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

            new Thread(() => {
                var empresas = projeto.empresas;
                Log.WriteLine(String.Format("[TelefoneWork] projeto {0} selecionado com {1} empresas", projeto.getParteName(), empresas.Count));

                var central = new resultys.prospecta.vendor.pabx.Central()
                {
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

                lock (this.objlock)
                {
                    this.totalCanaisDisponivel += totalEmpresaLotadas;
                }

            }).Start();
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
