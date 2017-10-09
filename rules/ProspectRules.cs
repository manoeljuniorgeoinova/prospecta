using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;
using resultys.prospecta.models;
using resultys.prospecta.worker;

namespace resultys.prospecta.rules
{
    public class ProspectRules
    {

        public void prospectar()
        {
            var qualificaRules = new QualificaRules();

            qualificaRules.onEmpty += new QualificaRulesEmptyDelegate(onEmpty);
            qualificaRules.onQualificado += new ProjetoDelegate(onQualificado);
            qualificaRules.onNaoQualificado += new ProjetoDelegate(onNaoQualificado);
            qualificaRules.onForceQualificacao += new ProjetoDelegate(onForceQualificacao);

            qualificaRules.start();
        }

        public List<Projeto> onEmpty(List<Projeto> projetos)
        {
            var projetosList = new List<Projeto>();

            for (int i = 0; i < projetos.Count; i++)
            {
                var projeto = projetos[i];

                projeto.prospect = this.buscarProspect(projeto);
                projeto.empresas = this.buscaEmpresas(projeto);
                projeto.totalEmpresasEncontradas = projeto.empresas.Count;

                if (projeto.isForceProspeccao)
                {
                    this.tentarQualificarProjeto(projeto);
                }
                else
                {
                    projetosList.Add(projeto);
                }
            }            

            return projetosList;
        }

        public void onQualificado(Projeto projeto)
        {
            var total = this.salvarEmpresasQualificadas(projeto, true);
            Log.WriteLine(String.Format("[Prospect] {0} empresas qualificadas", total));

            this.atualizarContatoEmpresas(projeto.empresas);
            this.tentarQualificarProjeto(projeto);
        }

        public void onNaoQualificado(Projeto projeto)
        {
            this.atualizarContatoEmpresas(projeto.empresas);
            var total = this.descartarEmpresas(projeto.empresas);
            Log.WriteLine(String.Format("[Prospect] {0} empresas descartadas", total));
        }

        public void onForceQualificacao(Projeto projeto)
        {
            this.atualizarContatoEmpresas(projeto.empresas);
            var total = this.salvarEmpresasQualificadas(projeto, false);
            this.tentarQualificarProjeto(projeto);
            Log.WriteLine(String.Format("[Prospect] {0} empresas qualificadas", total));
        }

        private Prospect buscarProspect(Projeto projeto)
        {
            var prospect = ProspectRepositorio.fetchOne(projeto.id, "EQ");
            if (prospect == null) prospect = ProspectRepositorio.insert(new Prospect
            {
                id_projeto = projeto.id,
                status = "EQ"
            });

            return prospect;
        }

        private List<Empresa> buscaEmpresas(Projeto projeto)
        {
            var totalEmpresasQualificadas = ProspectQualificadoRepositorio.fetchTotalEmpresasQualificadas(projeto.prospect);
            var totalEmpresaRestante = projeto.parametroPesquisa.quantidade_prospect * projeto.config.taxa_prospeccao - totalEmpresasQualificadas;
            if (totalEmpresaRestante < 0) totalEmpresaRestante = 0;

            projeto.parametroPesquisa.totalEmpresaRestantes = totalEmpresaRestante;

            var empresaRules = new EmpresaRules();
            var empresas = empresaRules.buscarEmpresas(projeto);

            return empresas;
        }

        private void atualizarContatoEmpresas(List<Empresa> empresas)
        {
            if (empresas == null) return;

            foreach (var e in empresas)
            {
                e.contato.cnpj = e.getRawCnpj();
                e.contato.id_empresa = e.id;

                EmpresaContatoRepositorio.update(e.contato);
            }
        }

        private void tentarQualificarProjeto(Projeto projeto)
        {
            var prospect = projeto.prospect;
            var totalQualificado = ProspectQualificadoRepositorio.fetchTotalEmpresasQualificadas(prospect);
            var totalSolicitado = projeto.parametroPesquisa.quantidade_prospect;
            var totalDesejado = totalSolicitado * projeto.config.taxa_prospeccao;

            var p = ProjetoRepositorio.fetch(projeto.id);
            if (p.status != "EQ") return;

            if (projeto.isForceProspeccao || totalQualificado >= totalDesejado)
            {
                prospect.nome = String.Format("{0}", projeto.nome_perfil, ProjetoRepositorio.getQtdeProspect(projeto.id));
                prospect.data_geracao = DateTime.Now;
                prospect.quantidade = totalQualificado > totalDesejado ? totalDesejado : totalQualificado;
                ProspectRepositorio.update(prospect);

                projeto.status = "Q";
                ProjetoRepositorio.updateStatus(projeto);

                var email = new Email
                {
                    destinatarios = Config.read("Email", "destinatarios").Split(','),
                    assunto = String.Format("Projeto: {0} Qualificação concluída", projeto.nome_perfil),
                    mensagem = String.Format("Fala galera aqui é o Robotys, passando para te avisar que os leads do projeto {0} já foi qualificado. Total solicitado: {1} Total qualificado: {2}", projeto.nome_perfil, totalSolicitado, totalQualificado)
                };

                try
                {
                    if (p.webhook != null) Pillar.Util.Request.Get(String.Format("{0}?projetoID={1}&prospectID={2}", p.webhook, p.id, prospect.id));
                }
                catch (Exception e){ }

                //email.send();
            }
        }

        private int salvarEmpresasQualificadas(Projeto projeto, bool onlyQualificaChamadasValidas)
        {
            var empresas = projeto.empresas;
            var prospect = projeto.prospect;

            var quantidadeQualificado = ProspectQualificadoRepositorio.fetchTotalEmpresasQualificadas(prospect);
            var quantidadeRestante = projeto.parametroPesquisa.quantidade_prospect * projeto.config.taxa_prospeccao - quantidadeQualificado;

            if (quantidadeRestante < 0) return 0;

            var quantidadeNecessaria = empresas.Count < quantidadeRestante ? empresas.Count : quantidadeRestante;
            var totalQualificado = 0;

            for (var i = 0; i < quantidadeNecessaria; i++)
            {
                var e = empresas[i];

                if (ProspectQualificadoRepositorio.exist(prospect, e.getRawCnpj())) continue;

                var contatoEmail = Higienize.removeContabilEmail(e.email);
                var contatoNome = e.getNomePrincipalSocio();

                if (projeto.cliente.isDemonstrativo && contatoEmail == null) {
                    this.descartarEmpresa(e, projeto);
                    continue;
                }

                if (projeto.cliente.isDemonstrativo && contatoEmail.Length == 0)
                {
                    this.descartarEmpresa(e, projeto);
                    continue;
                }

                if (projeto.cliente.isDemonstrativo && contatoNome == null)
                {
                    this.descartarEmpresa(e, projeto);
                    continue;
                }

                if (projeto.cliente.isDemonstrativo && contatoNome.Length == 0)
                {
                    this.descartarEmpresa(e, projeto);
                    continue;
                }

                if (onlyQualificaChamadasValidas)
                {
                    var chamadaValidada = e.contato.getChamadaValidada();
                    if (chamadaValidada == null) continue;

                    ProspectQualificadoRepositorio.insert(new ProspectQualificado
                    {
                        id_empresa = e.id,
                        id_prospect = prospect.id,
                        id_cliente = projeto.id_cliente,
                        cnpj = e.getRawCnpj(),
                        email = contatoEmail,
                        nome_contato = contatoNome,
                        telefone = chamadaValidada.telefone.getNumeroFormatado(),
                        telefone_pertence_empresa = chamadaValidada.resposta == vendor.pabx.ChamadaResposta.TELEFONE_PERTENCE_A_PESSOA,
                        telefones = this.convertChamadasToTelefones(e.contato.getChamadasValidas(), chamadaValidada)
                    });

                    totalQualificado++;
                }
                else
                {
                    var contato = e.contato;
                    if (contato == null) continue;

                    var chamadas = e.contato.chamadas;
                    if (chamadas == null) continue;
                    if (chamadas.Count == 0) continue;

                    var primeiraChamada = chamadas[0];
                    if (primeiraChamada == null) continue;

                    var primeiroTelefone = primeiraChamada.telefone;
                    ProspectQualificadoRepositorio.insert(new ProspectQualificado
                    {
                        id_empresa = e.id,
                        id_prospect = prospect.id,
                        id_cliente = projeto.id_cliente,
                        cnpj = e.getRawCnpj(),
                        email = contatoEmail,
                        nome_contato = contatoNome,
                        telefone = primeiroTelefone.getNumeroFormatado(),
                        telefone_pertence_empresa = false,
                        telefones = this.convertChamadasToTelefones(chamadas, primeiraChamada)
                    });

                    totalQualificado++;
                }
                
            }

            return totalQualificado;
        }

        private string convertChamadasToTelefones(List<vendor.pabx.Chamada> chamadas, vendor.pabx.Chamada exceto)
        {
            var ts = new List<string>();
            foreach (var ch in chamadas)
            {
                if (ch.telefone.compareTo(exceto.telefone)) continue;

                ts.Add(ch.telefone.getNumeroFormatado());
            }

            return String.Join(",", ts.ToArray()).Trim();
        }

        private void descartarEmpresa(Empresa empresa, Projeto projeto)
        {
            ProspectDescartadoRepositorio.insert(new ProspectDescartado
            {
                cnpj = empresa.getRawCnpj(),
                id_projeto = projeto.id,
                create_at = DateTime.Now
            });
        }

        private int descartarEmpresas(List<Empresa> empresas)
        {
            var total = 0;

            foreach (var e in empresas)
            {
                if (e.razao_social == null) continue;
                if (e.razao_social.Length == 0) continue;
                if (!e.contato.isDescartar) continue;
                if (e.telefones == null) continue;

                ProspectDescartadoRepositorio.delete(e.getRawCnpj());

                ProspectDescartadoRepositorio.insert(new ProspectDescartado
                {
                    cnpj = e.getRawCnpj(),
                    create_at = DateTime.Now
                });

                total++;
            }

            return total;
        }

    }
}
