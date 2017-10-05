using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.models;

namespace resultys.prospecta.lib
{
    class HigienizeTelefone
    {
        public Empresa empresa { get; set; }
        public Telefone telefone { get; set; }
    }

    class ComparerHigienize : IEqualityComparer<HigienizeTelefone>
    {
        public bool Equals(HigienizeTelefone x, HigienizeTelefone y)
        {
            return x.telefone.compareTo(y.telefone);
        }

        public int GetHashCode(HigienizeTelefone obj)
        {
            return obj.GetHashCode();
        }
    }

    class ComparerTelefone : IEqualityComparer<Telefone>
    {
        public bool Equals(Telefone x, Telefone y)
        {
            return x.compareTo(y);
        }

        public int GetHashCode(Telefone obj)
        {
            return obj.GetHashCode();
        }
    }

    public class Higienize
    {
        public Pesquisa parametros { get; set; }
        private static List<string> blacklist = Config.read("Higienize", "blacklist").Split(',').ToList();

        public List<Empresa> removeTelefoneContador(List<Empresa> empresas)
        {
            for (int i = 0; i < empresas.Count; i++)
            {
                if (empresas[i].telefones == null) continue;
                for (int j = 0; j < empresas[i].telefones.Count; j++)
                {
                    var telefone = empresas[i].telefones[j];
                    if (telefone == null) continue;

                    var isContador = resultys.prospecta.models.EmpresaRepositorio.isTelefonePertenceContador(telefone.getNumeroFormatado(), this.parametros);
                    if (isContador)
                    {
                        empresas[i].telefones.RemoveAt(j);
                        j--;
                        continue;
                    }
                }
            }

            return empresas;
        }

        public static string removeContabilEmail(string email)
        {
            if (email == null) return null;

            var find = blacklist.Find(e => email.ToLower().IndexOf(e.ToLower()) > -1);
            if (find == null) return email;
            else return "";
        }

        public static string[] removeContabilEmail(string[] emails)
        {
            var newlist = new List<string>();

            foreach (var email in emails)
            {
                var find = blacklist.Find(e => email.ToLower().IndexOf(e.ToLower()) > -1);
                if (find == null) newlist.Add(find);
            }

            return newlist.ToArray();
        }

        public List<Empresa> uniqueTelefones(List<Empresa> empresas)
        {
            var htelefones = new List<HigienizeTelefone>();

            for (int i = 0; i < empresas.Count; i++)
            {
                var empresa = empresas[i];
                if (empresa.telefones != null)
                {
                    for (int j = 0; j < empresa.telefones.Count; j++)
                    {
                        var telefone = empresa.telefones[j];
                        if (telefone == null)
                        {
                            empresa.telefones.RemoveAt(j);
                            j--;
                            continue;
                        }

                        htelefones.Add(new HigienizeTelefone
                        {
                            empresa = empresa,
                            telefone = telefone
                        });
                    }
                }

                if (empresa.contato != null && empresa.contato.chamadas != null)
                {
                    for (int j = 0; j < empresa.contato.chamadas.Count; j++)
                    {
                        var chamada = empresa.contato.chamadas[j];
                        if (chamada == null)
                        {
                            empresa.contato.chamadas.RemoveAt(j);
                            j--;
                            continue;
                        }

                        if (chamada.telefone == null)
                        {
                            empresa.contato.chamadas.RemoveAt(j);
                            j--;
                            continue;
                        }

                        htelefones.Add(new HigienizeTelefone
                        {
                            empresa = empresa,
                            telefone = chamada.telefone
                        });

                    }
                }
                
            }

            htelefones = htelefones.Distinct(new ComparerHigienize()).ToList();

            for (int i = 0; i < empresas.Count; i++)
            {
                var empresa = empresas[i];
                if (empresa.telefones != null)
                {
                    for (int j = 0; j < empresa.telefones.Count; j++)
                    {
                        var telefone = empresa.telefones[j];
                        var ht = htelefones.Find(e => e.telefone.compareTo(telefone) && !e.empresa.compareTo(empresa));
                        if (ht != null)
                        {
                            empresa.telefones.RemoveAt(j);
                            j--;
                            continue;
                        }
                    }
                }

                if (empresa.contato != null && empresa.contato.chamadas != null)
                {
                    for (int j = 0; j < empresa.contato.chamadas.Count; j++)
                    {
                        var telefone = empresa.contato.chamadas[j].telefone;
                        var ht = htelefones.Find(e => e.telefone.compareTo(telefone) && !e.empresa.compareTo(empresa));
                        if (ht != null)
                        {
                            empresa.contato.chamadas.RemoveAt(j);
                            j--;
                            continue;
                        }
                    }
                }
            }

            return empresas;
        }

        public List<Empresa> clearEmpresas(List<Empresa> empresas) {
            var newlist = new List<Empresa>();

            foreach (var empresa in empresas)
            {
                if (!this.isValidEstado(empresa)) continue;
                if (!this.isValidCnae(empresa)) continue;
                if (!this.isEmpresaAtiva(empresa)) continue;

                newlist.Add(empresa);
            }

            return newlist;
        }

        public List<Empresa> clearTelefones(List<Empresa> empresas)
        {
            foreach (var empresa in empresas)
            {
                if (empresa.telefones == null) continue;
                
                var telefones = new List<Telefone>();
                foreach (var telefone in empresa.telefones)
                {
                    if (telefone == null) continue;

                    if ((telefone.tipo == TelefoneTipo.FIXO || telefone.tipo == TelefoneTipo.CELULAR) && telefone.numero.Length < 10) continue;
                    if (telefone.ddd.prefixo.Length == 0) continue;

                    if (!telefone.ddd.belongTo(empresa.municipio, empresa.uf)) continue;

                    if (telefone.tipo == TelefoneTipo.FIXO)
                    {
                        telefones.Add(telefone);
                    }

                    if(telefone.tipo == TelefoneTipo.LOCAL)
                    {
                        telefones.Add(telefone);
                    }

                    if(telefone.tipo == TelefoneTipo.GRATUITO)
                    {
                        telefones.Add(telefone);
                    }       

                    if(telefone.tipo == TelefoneTipo.CELULAR)
                    {
                        telefones.Add(telefone);
                    }
                }
                
                empresa.telefones = telefones.Distinct(new ComparerTelefone()).ToList();
            }

            return empresas;
        }

        private bool isValidEstado(Empresa empresa)
        {
            if (this.parametros.estado == null) return true;
            if (this.parametros.estado.Length == 0) return true;

            foreach (var uf in this.parametros.estado)
            {
                if (uf == empresa.uf) return true;
            }

            return false;
        }

        private bool isEmpresaAtiva(Empresa empresa)
        {
            return empresa.situacao.ToUpper() == "ATIVA";
        }

        private bool isValidCnae(Empresa empresa)
        {
            if (this.parametros.segmento == null) return true;
            if (this.parametros.segmento.Length == 0) return true;

            foreach (var cnae in this.parametros.segmento)
            {
                if (cnae == empresa.cnae_primario_codigo) return true;
            }

            return false;
        }

    }
}
