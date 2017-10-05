using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.models;

namespace resultys.prospecta.lib
{
    public class Mensagem
    {
        public string texto { get; set; }

        public Mensagem(Empresa empresa)
        {
            var nome = "";

            if (empresa != null && empresa.nome_fantasia != null && empresa.nome_fantasia.Length > 0) nome = empresa.nome_fantasia;
            else if (empresa != null && empresa.razao_social != null && empresa.razao_social.Length > 0) nome = empresa.razao_social;

            this.texto = nome;
        }

        public string get()
        {
            var mensagem = resultys.prospecta.lib.Config.read("Mensagem", "texto");
            var keywords = new Dictionary<string, string>();
            keywords.Add("#NOME", this.clear(this.texto));

            return Pillar.Util.Template.Inject(mensagem, keywords);
        }

        private string clear(string text)
        {
            text = this.removerPalavrasChave(text);
            text = this.removeCaracteresEspecial(text);
            text = this.removerCnpjTexto(text);

            return text;
        }

        private string removerCnpjTexto(string texto)
        {
            return System.Text.RegularExpressions.Regex.Replace(texto, @"\d{11}", "");
        }

        private string removerPalavrasChave(string text)
        {
            var remove = resultys.prospecta.lib.Config.read("Mensagem", "remover_palavras");
            var pp = remove.Split(',');

            foreach (var p in pp)
            {
                var _p = p.Trim();
                text = text.Replace(_p, "");
                text = text.Replace(_p.ToLower(), "");
                text = text.Replace(_p.ToUpper(), "");
            }

            return text;
        }

        private string removeCaracteresEspecial(string text)
        {
            text = text.Replace("-", "");
            text = text.Replace("/", "");            

            return text;
        }


    }
}
