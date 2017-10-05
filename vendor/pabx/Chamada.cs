using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.models;
using resultys.prospecta.lib;

namespace resultys.prospecta.vendor.pabx
{
    public enum ChamadaStatus
    {
        NAO_ATENDIDA    = 1,        
        ATENDIDA        = 3,       
        ABERTA          = 0,
        OCUPADO         = 4,
        INDEFINIDA      = 5,
        TIMEOUT         = 6
    }

    public enum ChamadaResposta
    {
        DESCONHECIDA = 0,
        TELEFONE_PERTENCE_A_PESSOA = 1,
        TELEFONE_NAO_PERTENCE_A_PESSOA = 2,
        INVALIDA = 3,
        SEM_RESPOSTA = 4,
        TELEFONE_PERTENCE_AO_CONTADOR = 5
    }

    public class Chamada
    {
        public Telefone telefone { get; set; }
        public Mensagem mensagem { get; set; }

        public ChamadaStatus status { get; set; }
        public ChamadaResposta resposta { get; set; }

        private string respostaPertence { get; set; }
        private string respostaNaoPertence { get; set; }
        public string respostaContador { get; set; }

        public DateTime data { get; set; }
        public int tentativas { get; set; }

        public bool isBloqueada { get; set; }
        
        public int maxTentativasOcupado
        {
            get
            {
                return int.Parse(Config.read("Chamada", "max_tentativas_ocupado"));
            }
        }

        public int maxTentativasNaoAtendida
        {
            get
            {
                return int.Parse(Config.read("Chamada", "max_tentativas_nao_atendida"));
            }
        }

        public bool isDescartar
        {
            get
            {
                if (this.status == ChamadaStatus.ATENDIDA && this.resposta == ChamadaResposta.TELEFONE_NAO_PERTENCE_A_PESSOA) return true;
                if (this.status == ChamadaStatus.ABERTA && this.resposta == ChamadaResposta.TELEFONE_PERTENCE_AO_CONTADOR) return true;
                if (this.status == ChamadaStatus.ATENDIDA) return false;
                if (this.status == ChamadaStatus.ABERTA) return false;

                if (this.status == ChamadaStatus.OCUPADO && this.tentativas <= this.maxTentativasOcupado)
                {
                    return false;
                }

                if (this.status == ChamadaStatus.NAO_ATENDIDA && this.tentativas <= this.maxTentativasNaoAtendida)
                {
                    return false;
                }

                return true;
            }
        }

        public bool isVencida
        {
            get
            {
                return DateTime.Now.Subtract(this.data).TotalDays > int.Parse(Config.read("Chamada", "validade"));
            }
        }

        public Chamada()
        {
            this.respostaPertence = resultys.prospecta.lib.Config.read("Chamada", "resposta_pertence");
            this.respostaNaoPertence = resultys.prospecta.lib.Config.read("Chamada", "resposta_nao_pertence");
            this.respostaContador = resultys.prospecta.lib.Config.read("Chamada", "resposta_contador");

            this.status = ChamadaStatus.ABERTA;

            this.isBloqueada = false;
        }

        public Chamada(Telefone telefone, Mensagem mensagem) : this()
        {
            this.telefone = telefone;
            this.mensagem = mensagem;
        }

        private void relatorio(int callerId, string description)
        {
            try
            {
                var content = String.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}\r\n",
                this.data,
                this.telefone.numero,
                this.telefone.fonte,
                this.telefone.tipo,
                this.status,
                this.resposta,
                this.tentativas,
                callerId,
                description);

                System.IO.File.AppendAllText(String.Format(@"{0}\chamada.relatorio.csv", System.IO.Directory.GetCurrentDirectory()), content);
            }
            catch { }
        }
        
        public ChamadaStatus call()
        {
            var totalvoice = new resultys.prospecta.vendor.totalvoice.TotalVoiceAPI();            

            if (this.mensagem == null)
            {
                this.status = ChamadaStatus.ABERTA;
                return ChamadaStatus.ABERTA;
            }

            totalvoice.call(this.telefone.raw(), this.mensagem.get());

            this.data = DateTime.Now;
            this.tentativas++;

            var now = DateTime.Now;

            while (true)
            {
                var status = totalvoice.status;

                if(DateTime.Now.Subtract(now).Minutes > int.Parse(Config.read("Chamada", "timeout")))
                {
                    this.status = ChamadaStatus.TIMEOUT;
                    this.relatorio(totalvoice.callerId, "timeout");

                    return ChamadaStatus.TIMEOUT;
                }

                if (status == null)
                {
                    this.status = ChamadaStatus.ABERTA;
                    this.relatorio(totalvoice.callerId, "status null");

                    return ChamadaStatus.ABERTA;
                }

                if (status == "ocupado")
                {
                    this.status = ChamadaStatus.OCUPADO;
                    this.relatorio(totalvoice.callerId, "");

                    return this.status;
                }

                if (status == "sem resposta"     ||
                    status == "congestionado"    ||
                    status == "falha"            ||
                    status == "cancelada"        ||
                    status == "desconhecido")
                {
                    this.status = ChamadaStatus.NAO_ATENDIDA;
                    this.relatorio(totalvoice.callerId, "");

                    return this.status;
                }

                if (status == "atendida")
                {
                    
                    if (totalvoice.response != this.respostaPertence && totalvoice.duracao_fala <= int.Parse(Config.read("Chamada", "tempo_minimo_ligacao")))
                    {
                        this.status = ChamadaStatus.INDEFINIDA;
                        this.resposta = ChamadaResposta.DESCONHECIDA;

                        this.relatorio(totalvoice.callerId, "");

                        return this.status;
                    }

                    this.status = ChamadaStatus.ATENDIDA;
                    this.telefone.status = TelefoneStatus.VALIDADO;

                    if (totalvoice.response == null)
                    {
                        this.resposta = ChamadaResposta.SEM_RESPOSTA;
                        this.telefone.owner = TelefoneOwner.INDEFINIDO;

                        this.relatorio(totalvoice.callerId, "");

                        return this.status;
                    }

                    if (totalvoice.response.Length == 0)
                    {
                        this.resposta = ChamadaResposta.SEM_RESPOSTA;
                        this.telefone.owner = TelefoneOwner.INDEFINIDO;

                        this.relatorio(totalvoice.callerId, "");

                        return this.status;
                    }

                    if (this.respostaPertence == "*") {

                        this.resposta = ChamadaResposta.TELEFONE_PERTENCE_A_PESSOA;
                        this.telefone.owner = TelefoneOwner.EMPRESA;

                        this.relatorio(totalvoice.callerId, "");

                        return this.status;

                    } else if (totalvoice.response == this.respostaContador) {

                        this.resposta = ChamadaResposta.TELEFONE_PERTENCE_AO_CONTADOR;
                        this.telefone.owner = TelefoneOwner.DESCONHECIDO;

                        this.relatorio(totalvoice.callerId, "");

                        return this.status;

                    } else if (totalvoice.response == this.respostaPertence) {

                        this.resposta = ChamadaResposta.TELEFONE_PERTENCE_A_PESSOA;
                        this.telefone.owner = TelefoneOwner.EMPRESA;

                        this.relatorio(totalvoice.callerId, "");

                        return this.status;

                    } else if (this.respostaNaoPertence == "*") {

                        this.resposta = ChamadaResposta.TELEFONE_NAO_PERTENCE_A_PESSOA;
                        this.telefone.owner = TelefoneOwner.DESCONHECIDO;

                        this.relatorio(totalvoice.callerId, "");

                        return this.status;

                    } else if (totalvoice.response == this.respostaNaoPertence) {

                        this.resposta = ChamadaResposta.TELEFONE_NAO_PERTENCE_A_PESSOA;
                        this.telefone.owner = TelefoneOwner.DESCONHECIDO;

                        this.relatorio(totalvoice.callerId, "");

                        return this.status;

                    } else {

                        this.resposta = ChamadaResposta.INVALIDA;
                        this.telefone.owner = TelefoneOwner.INDEFINIDO;

                        this.relatorio(totalvoice.callerId, "");

                        return this.status;
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
