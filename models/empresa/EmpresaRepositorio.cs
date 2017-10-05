using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class EmpresaRepositorio
    {
        public static void updateSocios(Empresa empresa)
        {
            var socios = convertSociosToArray(empresa.socios);
            if (socios == null && empresa.email == null) return;
            else if (socios == null)
            {
                Repositorio.update(Config.read("Empresa", "table"), "cnpj", new
                {
                    cnpj = empresa.getRawCnpj(),
                    email = empresa.email
                });
            }
            else if (empresa.email == null)
            {
                Repositorio.update(Config.read("Empresa", "table"), "cnpj", new
                {
                    cnpj = empresa.getRawCnpj(),
                    socio = socios
                });
            }
            else
            {
                Repositorio.update(Config.read("Empresa", "table"), "cnpj", new
                {
                    cnpj = empresa.getRawCnpj(),
                    socio = socios,
                    email = empresa.email
                });
            }
        }

        public static void updateDataAtualizacaoReceita(Empresa empresa)
        {
            Repositorio.update(Config.read("Empresa", "table"), "cnpj", new
            {
                cnpj = empresa.getRawCnpj(),
                data_ultima_atualizacao = DateTime.Now
            });
        }

        public static void updateDataAtualizacaoSite(Empresa empresa)
        {
            Repositorio.update(Config.read("Empresa", "table"), "cnpj", new
            {
                cnpj = empresa.getRawCnpj(),
                data_ultima_atualizacao_site = DateTime.Now,
            });
        }

        public static void update(Empresa empresa)
        {
            Repositorio.update(Config.read("Empresa", "table"), "cnpj", new
            {
                cnpj = empresa.getRawCnpj(),
                nome_fantasia = empresa.nome_fantasia,
                razao_social = empresa.razao_social,
                email = empresa.email,
                cnae_primario_codigo = empresa.cnae_primario_codigo,
                municipio = empresa.municipio,
                uf = empresa.uf,
                situacao = empresa.situacao
            });
        }

        public static Empresa fetch(string cnpj)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT Empresa.*, EmpresaContato.*")
                                         .Append("FROM " + Config.read("Empresa", "table") + " AS Empresa")
                                         .Append("LEFT JOIN empresa.empresa_contato AS EmpresaContato ON EmpresaContato.cnpj = Empresa.cnpj")
                                         .Append("WHERE Empresa.cnpj = @0", cnpj);

            var es = Repositorio.fetch<Empresa, EmpresaContato, Empresa>((e, ec) =>
            {

                if (ec != null && ec.id > 0)
                {
                    e.contato = ec;
                    e.contato.chamadas = Protocol.deserialize(ec.protocol);
                }
                else
                {
                    e.contato = new EmpresaContato();
                    e.contato.chamadas = new List<vendor.pabx.Chamada>();
                }

                e.contato.id_empresa = e.id;
                e.contato.cnpj = e.cnpj;

                return e;
            }, sql);

            if (es.Count == 0) return null;
            else return es[0];
        }

        private static string[] convertSociosToArray(List<Socio> socios)
        {
            if (socios == null) return null;
            if (socios.Count == 0) return null;

            var arr = new string[socios.Count];

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = socios[i].nome;
            }

            return arr;
        }

        public static bool isTelefonePertenceContador(string telefone, Pesquisa parametro)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT COUNT(*)")
                                          .Append("FROM empresa.empresa_contador as Empresa")
                                          .Append("WHERE cnae_primario_codigo IN ('6920601', '6920602')")
                                          .Append("AND (empresa.fn_clean_telefone(telefone1) = empresa.fn_clean_telefone(@0) OR empresa.fn_clean_telefone(telefone2) = empresa.fn_clean_telefone(@0))", telefone);

            if (parametro.estado.Length > 0)
            {
                sql.Append("AND Empresa.uf IN " + convertArrayToString(parametro.estado));
            }

            if (parametro.municipio.Length > 0)
            {
                sql.Append("AND Empresa.municipio IN " + convertArrayToString(parametro.municipio));
            }

            return Repositorio.count(sql) > 0;
        }

        public static List<Empresa> searchByCnpjQualificado(int projetoID)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT Empresa.*, EmpresaContato.*")
                                          .Append("FROM prospect.cnpj_qualificado as CnpjQualificado")
                                          .Append("INNER JOIN " + Config.read("Empresa", "table") + " AS Empresa ON Empresa.cnpj = CnpjQualificado.cnpj")
                                          .Append("LEFT JOIN empresa.empresa_contato AS EmpresaContato ON EmpresaContato.cnpj = Empresa.cnpj")
                                          .Append("WHERE CnpjQualificado.id_projeto = @0", projetoID);

            return Repositorio.fetch<Empresa, EmpresaContato, Empresa>((e, ec) =>
            {

                if (ec != null && ec.id > 0)
                {
                    e.contato = ec;
                    e.contato.chamadas = Protocol.deserialize(ec.protocol);
                }
                else
                {
                    e.contato = new EmpresaContato();
                    e.contato.chamadas = new List<vendor.pabx.Chamada>();
                }

                e.contato.id_empresa = e.id;
                e.contato.cnpj = e.cnpj;

                if (e.socio != null)
                {
                    e.socios = new List<Socio>();
                    foreach (var s in e.socio)
                    {
                        e.socios.Add(new Socio { nome = s });
                    }
                }

                return e;
            }, sql);
        }

        public static List<Empresa> search(Pesquisa parametro, int limit)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT Empresa.*, EmpresaContato.*")
                                          .Append("FROM " + Config.read("Empresa", "table") + " AS Empresa")
                                          .Append("LEFT JOIN empresa.empresa_contato AS EmpresaContato ON EmpresaContato.cnpj = Empresa.cnpj")
                                          .Append("WHERE upper(Empresa.situacao) = 'ATIVA'");

            //sql.Append("AND Empresa.cnpj IN ('00664752000157','02450658000120','16682694000124','08903567000180','02834518000156','16949221000140','04392231000149','03414732000116','15833271000103','06085129000126')");

            if (parametro.segmento.Length > 0)
            {
                sql.Append("AND Empresa.cnae_primario_codigo IN " + convertArrayToString(parametro.segmento));
            }

            if (parametro.estado.Length > 0)
            {
                sql.Append("AND Empresa.uf IN " + convertArrayToString(parametro.estado));
            }

            if (parametro.municipio.Length > 0)
            {
                sql.Append("AND Empresa.municipio IN " + convertArrayToString(parametro.municipio));
            }

            if (parametro.porte_empresa.Length > 0)
            {
                sql.Append("AND Empresa.porte_tipo IN " + convertArrayToString(parametro.porte_empresa));
            }

            //if (parametro.regime_tributario.Length > 0)
            //{
            //    sql.Append("AND Empresa.regime_tributario IN " + convertArrayToString(parametro.regime_tributario));
            //}

            if (parametro.quantidade_funcionario.Length > 1)
            {
                sql.Append("AND Empresa.quantidade_funcionario BETWEEN @0 AND @1", int.Parse(parametro.quantidade_funcionario[0]), int.Parse(parametro.quantidade_funcionario[1]));
            }

            if (Data.isNotNull(parametro.data_inicio) && Data.isNotNull(parametro.data_fim))
            {
                sql.Append("AND Empresa.data_abertura BETWEEN @0 AND @1", parametro.data_inicio, parametro.data_fim);
            }
            else if (Data.isNotNull(parametro.data_inicio))
            {
                sql.Append("AND Empresa.data_abertura >= @0", parametro.data_inicio);
            }
            else if (Data.isNotNull(parametro.data_fim))
            {
                sql.Append("AND Empresa.data_abertura <= @0", parametro.data_fim);
            }

            if (parametro.palavra_chave_in != null && parametro.palavra_chave_in.Length > 0)
            {
                foreach (var key in parametro.palavra_chave_in)
                {
                    sql.Append("AND Empresa.razao_social LIKE @0", String.Format("%{0}%", key));
                }
            }

            if (parametro.palavra_chave_out != null && parametro.palavra_chave_out.Length > 0)
            {
                foreach (var key in parametro.palavra_chave_out)
                {
                    sql.Append("AND Empresa.razao_social NOT LIKE @0", String.Format("%{0}%", key));
                }
            }

            sql.Append("AND Empresa.natureza_juridica_codigo NOT IN ('2143','1015','1023','1031','1040','1058','1066','1074','1082','1104','1112','1120','1139','1147','1155','1163','1171','1180','1198','1201','1210','2011','4090')");
            sql.Append("AND Empresa.cnpj NOT IN(SELECT cnpj FROM cliente.cliente_dado WHERE id_cliente = @0)", parametro.id_cliente);
            sql.Append("AND Empresa.cnpj NOT IN(SELECT cnpj FROM cliente.cliente_prospect WHERE id_cliente = @0)", parametro.id_cliente);
            sql.Append("AND Empresa.cnpj NOT IN(SELECT cnpj FROM prospect.prospect_qualificado WHERE prospect.prospect_qualificado.id_cliente = @0)", parametro.id_cliente);
            sql.Append("AND Empresa.cnpj NOT IN(SELECT cnpj FROM prospect.prospect_descartado AS pd WHERE (pd.id_projeto = 0 OR pd.id_projeto = @0) AND (NOW()::date - create_at::date <= @1))", parametro.id_projeto, int.Parse(Config.read("Prospect", "quantidade_dias_descartado")));

            sql.Append("ORDER BY empresa.fn_get_index_porte_empresa(Empresa.porte_tipo), Empresa.quantidade_funcionario DESC");

            sql.Append("LIMIT @0", limit);

            return Repositorio.fetch<Empresa, EmpresaContato, Empresa>((e, ec) =>
            {

                if (ec != null && ec.id > 0)
                {
                    e.contato = ec;
                    e.contato.chamadas = Protocol.deserialize(ec.protocol);
                }
                else
                {
                    e.contato = new EmpresaContato();
                    e.contato.chamadas = new List<vendor.pabx.Chamada>();
                }

                e.contato.id_empresa = e.id;
                e.contato.cnpj = e.cnpj;

                if (e.socio != null)
                {
                    e.socios = new List<Socio>();
                    foreach (var s in e.socio)
                    {
                        e.socios.Add(new Socio { nome = s });
                    }
                }

                return e;
            }, sql);
        }

        private static string convertArrayToString(string[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = arr[i].Replace("'", "''");
            }
            return String.Format("('{0}')", String.Join("','", arr));
        }
    }
}
