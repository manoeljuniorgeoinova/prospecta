using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;
using resultys.prospecta.models;

namespace resultys.prospecta.vendor.pabx
{
    public class CompareTelefone : IEqualityComparer<Telefone>
    {
        public bool Equals(Telefone t1, Telefone t2)
        {
            return t1.compareTo(t2);
        }

        public int GetHashCode(Telefone obj)
        {
            return (int)obj.GetHashCode();
        }
    }

    public class EmpresaChamada
    {
        public Empresa empresa { get; set; }

        public int totalChamadasPertenceEmpresa
        {
            get
            {
                var total = 0;

                foreach (var chamada in this.empresa.contato.chamadas)
                {
                    if (chamada.resposta == ChamadaResposta.TELEFONE_PERTENCE_A_PESSOA) total++;
                }

                return total;
            }
        }

        public int totalChamadasAtendidasValidas {
            get
            {
                var total = 0;

                foreach (var chamada in this.empresa.contato.chamadas)
                {
                    if (chamada.status == ChamadaStatus.ATENDIDA && chamada.resposta == ChamadaResposta.TELEFONE_NAO_PERTENCE_A_PESSOA) continue;
                    if (chamada.status == ChamadaStatus.ABERTA && chamada.resposta == ChamadaResposta.TELEFONE_PERTENCE_AO_CONTADOR) continue;
                    if (chamada.status == ChamadaStatus.ATENDIDA) total++;
                }

                return total;
            }
        }

        public int totalChamadasValidasParaLigacao
        {
            get
            {
                var total = 0;

                foreach (var chamada in this.empresa.contato.chamadas)
                {                    
                    if (chamada.status == ChamadaStatus.NAO_ATENDIDA && chamada.tentativas <= chamada.maxTentativasNaoAtendida) total++;
                    if (chamada.status == ChamadaStatus.OCUPADO && chamada.tentativas <= chamada.maxTentativasOcupado) total++;
                    if (chamada.status == ChamadaStatus.ABERTA) total++;
                }

                return total;
            }
        }

        public EmpresaChamada(Empresa empresa)
        {
            this.empresa = empresa;

            this.loadChamadas();
            this.loadMensagem();

            this.orderFonte();
        }

        private void loadChamadas()
        {
            if (this.empresa.telefones == null) this.empresa.telefones = new List<Telefone>();
            if (this.empresa.contato == null) this.empresa.contato = new EmpresaContato();
            if (this.empresa.contato.chamadas == null) this.empresa.contato.chamadas = new List<Chamada>();

            this.empresa.telefones = this.empresa.telefones.Distinct(new CompareTelefone()).ToList();

            // <temporario>        
            var chamadas = new List<Chamada>();
            foreach (var ch in this.empresa.contato.chamadas)
            {
                if (!ch.telefone.ddd.belongTo(this.empresa.municipio, this.empresa.uf)) continue;
                chamadas.Add(ch);
            }
            this.empresa.contato.chamadas = chamadas;
            // </temporario>

            foreach (var telefone in this.empresa.telefones)
            {
                var ch = this.empresa.contato.chamadas.Find(c => c.telefone.numero == telefone.numero);                

                if (ch == null)
                {
                    ch = new Chamada(telefone, new Mensagem(this.empresa));
                    this.empresa.contato.chamadas.Add(ch);
                } else
                {
                    ch.mensagem = new Mensagem(this.empresa);
                }
            }
        }

        public void loadMensagem()
        {
            if (this.empresa == null) return;
            if (this.empresa.contato == null) return;
            if (this.empresa.contato.chamadas == null) return;

            foreach (var ch in this.empresa.contato.chamadas)
            {
                ch.mensagem = new Mensagem(this.empresa);
            }
        }

        public Chamada findChamadaOcupada()
        {
            var tentativasOcupado = int.Parse(Config.read("Chamada", "max_tentativas_ocupado"));
            var tempoMaximoEspera = int.Parse(Config.read("Chamada", "max_tempo_espera"));
            var chamadas = this.empresa.contato.chamadas;

            foreach (var chamada in chamadas)
            {
                if (chamada.status == ChamadaStatus.OCUPADO && !chamada.isBloqueada && chamada.tentativas <= tentativasOcupado) return chamada;
            }

            return null;
        }

        public Chamada findChamadaNaoAtendida()
        {
            var tentativasNaoAtendida = int.Parse(Config.read("Chamada", "max_tentativas_nao_atendida"));
            var tempoMaximoEspera = int.Parse(Config.read("Chamada", "max_tempo_espera"));
            var chamadas = this.empresa.contato.chamadas;

            foreach (var chamada in chamadas)
            {
                if (chamada.status == ChamadaStatus.NAO_ATENDIDA && !chamada.isBloqueada && chamada.tentativas <= tentativasNaoAtendida) return chamada;
            }

            return null;
        }

        public Chamada findChamadaAberta()
        {
            var chamadas = this.empresa.contato.chamadas;

            foreach (var chamada in chamadas)
            {
                if (chamada.status == ChamadaStatus.ABERTA) return chamada;
            }

            return null;
        }

        private void orderFonte()
        {
            var chamadaGoogle = new List<Chamada>();
            var chamadaSite = new List<Chamada>();
            var chamadaTelelista = new List<Chamada>();
            var chamadaReceita = new List<Chamada>();
            var chamadaInvalidas = new List<Chamada>();
            var chamadaGuiaMais = new List<Chamada>();

            foreach (var ch in this.empresa.contato.chamadas)
            {
                if (ch.resposta == ChamadaResposta.TELEFONE_NAO_PERTENCE_A_PESSOA) chamadaInvalidas.Add(ch);
                else if (ch.resposta == ChamadaResposta.TELEFONE_PERTENCE_AO_CONTADOR) chamadaInvalidas.Add(ch);
                else if (ch.status == ChamadaStatus.INDEFINIDA) chamadaInvalidas.Add(ch);
                else if (ch.telefone.fonte == TelefoneFonte.GUIAMAIS) chamadaGuiaMais.Add(ch);
                else if (ch.telefone.fonte == TelefoneFonte.GOOGLE) chamadaGoogle.Add(ch);
                else if (ch.telefone.fonte == TelefoneFonte.SITE) chamadaSite.Add(ch);
                else if (ch.telefone.fonte == TelefoneFonte.TELELISTA) chamadaTelelista.Add(ch);
                else if (ch.telefone.fonte == TelefoneFonte.RECEITA) chamadaReceita.Add(ch);
            }

            this.empresa.contato.chamadas = chamadaGoogle
                                            .Concat(chamadaReceita)
                                            .Concat(chamadaTelelista)
                                            .Concat(chamadaSite)
                                            .Concat(chamadaGuiaMais)
                                            .Concat(chamadaInvalidas).ToList();
        }
    }
}
