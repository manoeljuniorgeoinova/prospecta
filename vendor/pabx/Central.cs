using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;
using resultys.prospecta.models;

namespace resultys.prospecta.vendor.pabx
{
    public class Central
    {
        private List<Empresa> listaEmpresasValidadas { get; set; }
        private List<EmpresaChamada> listaEmpresaChamadas { get; set; }
        private List<EmpresaChamada> listaEmpresaChamadas2 { get; set; }

        private Tronco tronco { get; set; }

        private List<Chamada> chamadas { get; set; }

        public bool validaSomenteTelefoneConfirmado { get; set; }

        public Central()
        {
            this.tronco = new Tronco();
        }

        public List<Empresa> validarTelefones(List<Empresa> empresas)
        {
            var qteLote = int.Parse(Config.read("CentralTelefonica", "total_ligacao_por_empresa_lote"));
            this.loadChamadaEmpresas(empresas);

            for (var i = 0; i < qteLote; i++)
            {
                this.limparChamadasTronco();

                this.loadChamada();

                this.getTotalChamadas();

                if (this.getTotalChamadas() == 0) break;

                this.ligar();

                this.loadChamadasPertenceEmpresa();
            }

            this.loadChamadasValidas();

            this.desbloquearChamadas();

            return this.orderChamadas(this.listaEmpresasValidadas);
        }

        private List<Empresa> orderChamadas(List<Empresa> empresas)
        {
            foreach (var e in empresas)
            {
                e.contato.orderByChamadasValidas();
            }

            return empresas;
        }

        private void desbloquearChamadas()
        {
            foreach (var ch in this.chamadas)
            {
                ch.isBloqueada = false;
            }            
        }

        private void limparChamadasTronco()
        {
            this.tronco.clear();
        }

        private void ligar() {            
            this.tronco.ligar();
        }

        private int getTotalChamadas()
        {
            return this.listaEmpresaChamadas.Count;
        }

        private void loadChamadaEmpresas(List<Empresa> empresas)
        {
            this.listaEmpresaChamadas   = new List<EmpresaChamada>();
            this.listaEmpresasValidadas = new List<Empresa>();
            this.listaEmpresaChamadas2  = new List<EmpresaChamada>();
            this.chamadas               = new List<Chamada>();

            foreach (var empresa in empresas)
            {
                var ec = new EmpresaChamada(empresa);

                this.listaEmpresaChamadas.Add(ec);
                this.listaEmpresaChamadas2.Add(ec);
            }
        }

        private void loadChamadasValidas()
        {
            for (int i = 0; i < this.listaEmpresaChamadas2.Count; i++)
            {
                var ec = this.listaEmpresaChamadas2[i];                

                if(!this.validaSomenteTelefoneConfirmado)
                {
                    // totalChamadasPertenceEmpresa == 0 : nenhum dos telefones da lista foi respondido como pertence a empresa
                    // totalChamadasValidasParaLigacao == 0 : existe ligação valida para tentar ligar novamente
                    // totalChamadasAtendidasValidas > 0 : usuario atendeu e nao digitou nada ou uma resposta invalida
                    if (ec.totalChamadasPertenceEmpresa == 0 && ec.totalChamadasValidasParaLigacao == 0 && ec.totalChamadasAtendidasValidas > 0)
                    {
                        this.listaEmpresasValidadas.Add(ec.empresa);
                    }
                }
            }
        }
        
        private void loadChamadasPertenceEmpresa()
        {
            for (int i = 0; i < this.listaEmpresaChamadas.Count; i++)
            {
                var ec = this.listaEmpresaChamadas[i];

                if (ec.totalChamadasPertenceEmpresa > 0)
                {
                    this.listaEmpresasValidadas.Add(this.listaEmpresaChamadas[i].empresa);
                    this.listaEmpresaChamadas.Remove(this.listaEmpresaChamadas[i]);
                    i--;
                    continue;
                }
            }
        }

        private void loadChamada()
        {
            for (int i = 0; i < this.listaEmpresaChamadas.Count; i++)
            {
                var empresaChamada = this.listaEmpresaChamadas[i];
                
                var chamada = empresaChamada.findChamadaOcupada();
                if (chamada == null) chamada = empresaChamada.findChamadaAberta();
                if (chamada == null) chamada = empresaChamada.findChamadaNaoAtendida();
                if (chamada == null)
                {
                    this.listaEmpresaChamadas.Remove(listaEmpresaChamadas[i]);
                    i--;
                    continue;
                };

                chamada.isBloqueada = true;

                this.chamadas.Add(chamada);
                this.tronco.add(chamada);
            }
        }

        private int totalChamadasValidas()
        {
            var total = 0;
            for (int i = 0; i < this.listaEmpresaChamadas.Count; i++)
            {
                var empresaChamada = this.listaEmpresaChamadas[i];

                var chamada = empresaChamada.findChamadaOcupada();
                if (chamada == null) chamada = empresaChamada.findChamadaAberta();
                if (chamada == null) chamada = empresaChamada.findChamadaNaoAtendida();
                if (chamada != null) total++;
            }

            return total;
        }

    }
}
