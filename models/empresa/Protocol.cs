using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;
using resultys.prospecta.vendor.pabx;

namespace resultys.prospecta.models
{
    public class Protocol
    {
        public static string serialize(List<Chamada> chamadas)
        {
            var list = new List<string>();

            foreach (var chamada in chamadas)
            {
                list.Add(Protocol.serialize(chamada));
            }

            return String.Join(",", list.ToArray());
        }

        public static string serialize(Chamada chamada)
        {
            return String.Format("V1C{0:ddMMyy}{1}{2}{3}",
                chamada.data,
                chamada.tentativas > 9 ? 9 : chamada.tentativas,
                ((int)chamada.status),
                ((int)chamada.resposta)
                ) + String.Format("T{0}{1}{2}{3}{4}",
                ((int)chamada.telefone.fonte),
                ((int)chamada.telefone.tipo),
                ((int)chamada.telefone.status),
                ((int)chamada.telefone.owner),
                chamada.telefone.numero
            );
        }

        public static List<Chamada> deserialize(string protocol)
        {
            var arr = protocol.Split(',');
            var list = new List<Chamada>();

            foreach (var p in arr)
            {
                var ch = Protocol.deserializeObject(p);
                if (ch == null) continue;

                list.Add(ch);
            }

            return list;
        }

        private static string[] extractSegments(string protocol)
        {
            var p = protocol.Split('C');
            var v = p[0];

            p = p[1].Split('T');
            var c = p[0];
            var t = p[1];

            return new string[] {
                v.Substring(1),
                c,
                t
            };
        }

        private static Chamada deserializeObject(string protocol)
        {
            if (protocol == null) return null;
            if (protocol.Length == 0) return null;

            var segments = extractSegments(protocol);
            var chamada = segments[1];
            var telefone = segments[2];

            // dados chamada
            var dia = int.Parse(chamada.Substring(0, 2));
            var mes = int.Parse(chamada.Substring(2, 2));
            var ano = int.Parse(String.Format("20{0}", chamada.Substring(4, 2)));
            var tentativas = int.Parse(chamada.Substring(6, 1));
            var statusChamada = int.Parse(chamada.Substring(7, 1));
            var resposta = int.Parse(chamada.Substring(8, 1));

            // dados telefone
            var fonte = int.Parse(telefone.Substring(0, 1));
            var tipo = int.Parse(telefone.Substring(1, 1));
            var statusTelefone = int.Parse(telefone.Substring(2, 1));
            var owner = int.Parse(telefone.Substring(3, 1));
            var numero = telefone.Substring(4);

            return new Chamada
            {
                data = new DateTime(ano, mes, dia, 0, 0, 0),
                tentativas = tentativas,
                resposta = (ChamadaResposta)resposta,
                status = (ChamadaStatus)statusChamada,
                telefone = new Telefone(numero)
                {
                    fonte = (TelefoneFonte)fonte,
                    owner = (TelefoneOwner)owner,
                    status = (TelefoneStatus)statusTelefone,
                    tipo = (TelefoneTipo)tipo
                }
            };
        }

    }
}
