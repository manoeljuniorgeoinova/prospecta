using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public enum TelefoneTipo
    {
        FIXO,
        CELULAR,
        GRATUITO,
        LOCAL,
        INVALIDO
    }

    public enum TelefoneStatus
    {
        NAO_VALIDADO = 2,
        VALIDADO = 1,
        INVALIDO = 3,
    }

    public enum TelefoneOwner
    {
        EMPRESA = 1,
        DESCONHECIDO = 2,
        INDEFINIDO = 3
    }

    public class Telefone
    {
        public string numero { get; set; }
        public DDD ddd { get; set; }

        public TelefoneTipo tipo { get; set; }
        public TelefoneFonte fonte { get; set; }
        public TelefoneStatus status { get; set; }
        public TelefoneOwner owner { get; set; }

        public Telefone(string numero)
        {
            this.numero = this.higienizar(numero);
            this.tipo = this.extractTipoTelefone();
            this.ddd = this.extractDDD();

            this.status = TelefoneStatus.NAO_VALIDADO;
            this.owner = TelefoneOwner.INDEFINIDO;
        }

        public bool compareTo(Telefone telefone)
        {
            if (telefone == null) return false;
            return this.numero == telefone.numero;
        }

        private string higienizar(string numero)
        {
            numero = this.clean(numero.Trim());
            numero = this.normalizarDigitos(numero);

            return numero;
        }

        private string normalizarDigitos(string numero)
        {
            numero = this.removeZeroInicio(numero);

            if (numero.Length == 9) return String.Format("{0}{1}3{2}{3}{4}{5}{6}{7}{8}", numero[0], numero[1], numero[2], numero[3], numero[4], numero[5], numero[6], numero[7], numero[8]);
            else return numero;
        }

        private string clean(string numero)
        {
            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(numero, "");
        }

        private DDD extractDDD()
        {
            var prefixo = this.extractPrefixo();
            if (prefixo == null) return new DDD { prefixo = "" };

            var ddd = DDD.find(prefixo);

            if (ddd == null) return new DDD { prefixo = prefixo };
            return ddd; 
        }

        private string extractPrefixo()
        {
            if (this.tipo == TelefoneTipo.GRATUITO || this.tipo == TelefoneTipo.LOCAL || this.tipo == TelefoneTipo.INVALIDO)
            {
                return null;
            }

            return String.Format("{0}{1}", this.numero[0], this.numero[1]);
        }

        private TelefoneTipo extractTipoTelefone()
        {
            if (this.numero == null) return TelefoneTipo.INVALIDO;
            if (this.numero.Length == 0) return TelefoneTipo.INVALIDO;
            if (this.numero.IndexOf("0800") == 0) return TelefoneTipo.GRATUITO;
            else if (this.numero.IndexOf("400") == 0) return TelefoneTipo.LOCAL;
            else if (this.numero.Length < 9) return TelefoneTipo.INVALIDO;
            else if (this.numero[2] == '9' || this.numero[2] == '8') return TelefoneTipo.CELULAR;
            else if (this.numero.Length == 10) return TelefoneTipo.FIXO;
            else return TelefoneTipo.INVALIDO;
        }
        
        public string getNumeroFormatado()
        {
            if (this.tipo == TelefoneTipo.GRATUITO || this.tipo == TelefoneTipo.LOCAL) return numero;

            if (numero.Length == 9) return String.Format("({0}{1}) 3{2}{3}{4}-{5}{6}{7}{8}", numero[0], numero[1], numero[2], numero[3], numero[4], numero[5], numero[6], numero[7], numero[8]);
            if (numero.Length == 10) return String.Format("({0}{1}) {2}{3}{4}{5}-{6}{7}{8}{9}", numero[0], numero[1], numero[2], numero[3], numero[4], numero[5], numero[6], numero[7], numero[8], numero[9]);
            if (numero.Length == 11) return String.Format("({0}{1}) {2}{3}{4}{5}{6}-{7}{8}{9}{10}", numero[0], numero[1], numero[2], numero[3], numero[4], numero[5], numero[6], numero[7], numero[8], numero[9], numero[10]);

            return numero;
        }

        private string removeZeroInicio(string numero)
        {
            if (numero.IndexOf("0800") == 0) return numero;

            for (int i = 0; i < numero.Length; i++)
            {
                if (int.Parse(numero[i].ToString()) > 0) break;

                i--;
                numero = numero.Remove(0, 1);
            }

            return numero;
        }
        
        public static implicit operator Telefone(string numero)
        {
            return new Telefone(numero);
        }
        
        public string raw()
        {
            return String.Format("{0}", this.numero);
        }
    }
}
