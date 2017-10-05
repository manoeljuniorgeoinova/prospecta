using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;
using resultys.prospecta.vendor.pabx;

namespace resultys.prospecta.models
{    
    public class EmpresaContato
    {
        public int id { get; set; }
        public int id_empresa { get; set; }
        public string cnpj { get; set; }

        public string protocol { get; set; }

        public string linkedin { get; set; }
        public string facebook { get; set; }
        public string twitter { get; set; }

        public string emails { get; set; }
        public string sites { get; set; }

        [PetaPoco.Ignore] public List<Chamada> chamadas { get; set; }

        public bool isDescartar
        {
            get
            {
                if (this.chamadas == null) return false;
                if (this.chamadas.Count == 0) return true;

                var b = true;
                foreach (var ch in this.chamadas)
                {
                    if (!ch.isDescartar)
                    {
                        b = false;
                        break;
                    }
                }

                return b;
            }
        }
        
        public Chamada getChamadaValidada()
        {
            Chamada chamada = null;

            foreach (var ch in this.chamadas)
            {
                if (ch.resposta == ChamadaResposta.TELEFONE_PERTENCE_AO_CONTADOR) continue;
                if (ch.resposta == ChamadaResposta.TELEFONE_NAO_PERTENCE_A_PESSOA) continue;
                if (ch.resposta == ChamadaResposta.TELEFONE_PERTENCE_A_PESSOA) return ch;
                if (ch.status == ChamadaStatus.ATENDIDA) chamada = ch;
            }

            return chamada;
        }

        public List<Chamada> getChamadasValidas()
        {
            var chs = new List<Chamada>();

            if (this.chamadas == null) return chs;

            foreach (var ch in this.chamadas)
            {
                if (ch.isVencida) continue;
                if (ch.resposta == ChamadaResposta.TELEFONE_PERTENCE_AO_CONTADOR) continue;
                if (ch.resposta == ChamadaResposta.TELEFONE_NAO_PERTENCE_A_PESSOA) continue;

                if (ch.status == ChamadaStatus.ATENDIDA) {
                    chs.Add(ch);
                    continue;
                }
            }

            return chs;
        }

        public EmpresaContato orderByChamadasValidas()
        {
            var chamadasAtendidas = new List<Chamada>();
            var chamadasInvalidas = new List<Chamada>();
            var chamadasOutras = new List<Chamada>(); 

            if (this.chamadas == null) this.chamadas = new List<Chamada>();

            foreach (var ch in this.chamadas)
            {
                if (ch.resposta == ChamadaResposta.TELEFONE_NAO_PERTENCE_A_PESSOA) chamadasInvalidas.Add(ch);
                else if (ch.resposta == ChamadaResposta.TELEFONE_PERTENCE_AO_CONTADOR) chamadasInvalidas.Add(ch);
                else if (ch.status == ChamadaStatus.ATENDIDA) chamadasAtendidas.Add(ch);
                else chamadasOutras.Add(ch);
            }

            this.chamadas = chamadasAtendidas.Concat(chamadasOutras).Concat(chamadasInvalidas).ToList();

            return this;
        }
    }
}
