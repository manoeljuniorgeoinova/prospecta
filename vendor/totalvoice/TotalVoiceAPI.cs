using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;
using Evoline.API;

namespace resultys.prospecta.vendor.totalvoice
{
    public class TotalVoiceAPI
    {
        private Client client = null;
        private Caller caller = null;
        private Status callerStatus = null;

        public int callerId
        {
            get
            {
                if (this.callerStatus == null) return 0;
                else return (int)this.callerStatus.id;
            }
        }

        public string response {
            get
            {
                if (callerStatus == null) return "";

                return callerStatus.resposta;
            }
        }
        public string status
        {
            get
            {
                if (this.caller == null) return "falha";

                this.callerStatus = this.client.statusComposto(this.caller);

                if (this.callerStatus == null) return "falha";

                return this.callerStatus.status;
            }
        }

        public int duracao_fala
        {
            get
            {
                if (this.callerStatus.duracao_falada_segundos == null) return 0;
                else return (int)this.callerStatus.duracao_falada_segundos;
            }
        }

        public TotalVoiceAPI()
        {
            this.client = new Evoline.API.Client(resultys.prospecta.lib.Config.read("TotalVoiceAPI", "token"));
            this.client.velocidade = int.Parse(resultys.prospecta.lib.Config.read("TotalVoiceAPI", "velocidade_fala"));
        }

        public Caller call(string numero, string text)
        {
            var tentativas = 3;

            while (tentativas-- != 0)
            {
                try
                {
                    this.caller = client.callComposto(numero, new List<IAcao> {
                        new Audio {
                            url = "http://resultys.com.br/audio/audio1.mp3",
                            aguardarRespostaUsuario = false
                        },
                        new TTS {
                            aguardarRespostaUsuario = false,
                            texto = this.cleanText(text),
                            velocidade = this.client.velocidade
                        },
                        new Audio
                        {
                            url = "http://resultys.com.br/audio/audio2.mp3",
                            aguardarRespostaUsuario = true
                        },
                        new Audio
                        {
                            url = "http://resultys.com.br/audio/audio3.mp3"
                        }
                    });
                }
                catch (Exception)
                {

                }

                if (this.caller == null)
                {
                    System.Threading.Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (tentativas == 0)
            {
                Log.WriteLine(String.Format("Error ao tentar chamar telefone {0}", numero));
            }

            return this.caller;
        }

        private string cleanText(string nome)
        {
            nome = nome.Replace("-", "");
            nome = nome.Replace("\"", "");
            nome = nome.Replace("&", "");
            nome = nome.Replace(" ", "    ");

            return nome;
        }

    }
}
