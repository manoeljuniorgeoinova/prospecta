using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class ProjetoRepositorio
    {
        
        public static int getQtdeProspect(int id)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT COUNT(*) FROM prospect.prospect WHERE status = 'EQ' AND id_projeto = @0", id);
            return Repositorio.count(sql);
        }

        public static void updateStatus(Projeto projeto)
        {
            Repositorio.update("prospect.projeto", "id", new
            {
                id = projeto.id,
                status = projeto.status
            });
        }

        public static Projeto fetch(int id)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT Projeto.* FROM prospect.projeto AS Projeto WHERE id = @0", id);

            return Repositorio.fetchOne<Projeto>(sql);
        }

        public static List<Projeto> fetchEmQualificacao()
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT Projeto.*, Prospect.*, ConfigureMotor.*")
                                          .Append("FROM prospect.projeto as Projeto")
                                          .Append("LEFT JOIN prospect.configure_motor as ConfigureMotor ON ConfigureMotor.id_projeto = Projeto.id")
                                          .Append("LEFT JOIN (SELECT * FROM prospect.prospect ORDER BY prospect.prospect.id DESC LIMIT 1) as Prospect ON Prospect.id_projeto = Projeto.id")
                                          //.Append("WHERE Projeto.id IN (" + Config.read("Projeto", "load") + ")")
                                          //.Append("WHERE Projeto.id IN (917)")
                                          .Append("WHERE Projeto.status = 'EQ'")
                                          .Append("ORDER BY Projeto.id");

            var projetos = Repositorio.fetch<ProjetoTable, Prospect, ConfigureMotor, Projeto>((projeto, prospect, configure) =>
            {
                var prj = new Projeto
                {
                    id = projeto.id,
                    nome_perfil = projeto.nome_perfil,
                    id_cliente = projeto.id_cliente,
                    id_usuario = projeto.id_usuario,
                    status = projeto.status,
                    parametroPesquisa = new Pesquisa
                    {
                        id_cliente = projeto.id_cliente,
                        id_usuario = projeto.id_usuario,
                        id_projeto = projeto.id,
                        data_fim = projeto.data_fim,
                        data_inicio = projeto.data_inicio,
                        estado = projeto.estado,
                        municipio = projeto.municipio,
                        porte_empresa = projeto.porte_empresa,
                        quantidade_funcionario = convertArrayToArray(projeto.quantidade_funcionario),
                        regime_tributario = projeto.regime_tributario,
                        quantidade_prospect = projeto.quantidade_prospect,
                        segmento = projeto.segmento,
                        palavra_chave_in = projeto.palavra_chave_in,
                        palavra_chave_out = projeto.palavra_chave_out
                    }
                };

                if (configure.id == 0)
                {
                    configure.id_projeto = prj.id;
                    configure.step_receita = true;
                    configure.step_site = true;
                    configure.step_telefone = true;
                    configure.confirma_telefone = false;
                    configure.validade_receita = int.Parse(Config.read("Break", "quantidade_dias_atualizada_receita"));
                    configure.validate_site = int.Parse(Config.read("Break", "quantidade_dias_atualizada_site"));
                    configure.base_cnpj = false;
                    configure.taxa_busca_prospects = int.Parse(Config.read("Empresa", "taxa_prospect"));
                    configure.taxa_prospeccao = int.Parse(Config.read("Prospect", "taxa_prospeccao"));
                }

                prj.config = configure;

                return prj;
            }, sql);

            foreach (var p in projetos)
            {
                var sql2 = PetaPoco.Sql.Builder.Append("SELECT forcar_prospeccao FROM prospect.projeto WHERE id = @0", p.id);
                p.isForceProspeccao = Repositorio.fetchOne<bool>(sql2);
                p.cliente = ClienteRepositorio.fetch(p.id_cliente);
            }

            return projetos;
        }

        private static string[] convertArrayToArray(string[] arr)
        {
            return arr.Length == 0 ? new string[0] : arr[0].Split(';');
        }
    }
}
