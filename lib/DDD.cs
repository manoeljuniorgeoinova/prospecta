using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.lib
{
    public class DDD
    {

        private const int COLUMN_ESTADO = 0;
        private const int COLUMN_CIDADE = 1;
        private const int COLUMN_PREFIX = 2;

        public string prefixo { get; set; }
        public string cidade { get; set; }
        public string estado { get; set; }

        private List<DDD> ddds { get; set; }

        public DDD()
        {
            this.ddds = new List<DDD>();            
        }

        public static DDD find(string prefixo)
        {
            if (prefixo == null) return null;

            var ddd = new DDD();
            ddd.load();

            foreach (var d in ddd.ddds)
            {
                if (d.prefixo == prefixo) return d;
            }

            return null;
        }

        public bool belongTo(string cidade, string estado)
        {
            var ddd = DDD.find(cidade, estado);
            if (ddd == null) return false;

            return ddd.prefixo == this.prefixo;
        }

        public static DDD find(string cidade, string estado)
        {            
            var ddd = new DDD { 
                cidade = cidade,
                estado = estado
            };

            ddd.load();

            foreach (var d in ddd.ddds)
            {
                if (d.compare(ddd)) return d;
            }

            return null;
        }

        public bool compare(DDD ddd)
        {
            var c1 = this.clean(ddd.cidade);
            var c2 = this.clean(this.cidade);

            var e1 = this.clean(ddd.estado);
            var e2 = this.clean(this.estado);

            return c1 == c2 && e1 == e2;
        }

        private string clean(string text)
        {
            text = this.removerAcentos(text);

            text = text.Replace("'", "");
            text = text.Replace("-", "");

            return text.ToUpper();
        }

        private string removerAcentos(string texto)
        {
            string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
            string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

            for (int i = 0; i < comAcentos.Length; i++)
            {
                texto = texto.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());
            }
            return texto;
        }

        public void load()
        {
            var lines = this.readLinesFile(this.getFilename());

            foreach (var line in lines)
            {
                this.ddds.Add(new DDD
                {
                    estado = this.extractColumnNumber(DDD.COLUMN_ESTADO, line),
                    cidade = this.extractColumnNumber(DDD.COLUMN_CIDADE, line),
                    prefixo = this.extractColumnNumber(DDD.COLUMN_PREFIX, line)
                });
            }
        }
        
        private string extractColumnNumber(int column, string line)
        {
            var p = line.Split(',');

            return p[column].Trim();
        }

        private string getFilename()
        {
            return String.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), Config.read("DDD", "filename"));
        }

        private string[] readLinesFile(string filename)
        {
            return System.IO.File.ReadAllLines(filename);

            var line = "";
            var file = new System.IO.StreamReader(filename);
            var lines = new List<string>();

            while ((line = file.ReadLine()) != null)
            {
                lines.Add(line);
            }

            file.Close();

            return lines.ToArray();
        }

    }
}
