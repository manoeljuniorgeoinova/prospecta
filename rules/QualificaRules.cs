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
    public delegate List<Projeto> QualificaRulesEmptyDelegate();

    public class QualificaRules
    {
        public ProjetoDelegate onQualificado { get; set; }
        public ProjetoDelegate onNaoQualificado { get; set; }
        public ProjetoDelegate onForceQualificacao { get; set; }
        public QualificaRulesEmptyDelegate onEmpty { get; set; }

        public ReceitaWorker receitaWorker { get; set; }
        public SiteWorker siteWorker { get; set; }
        public TelefoneWorker telefoneWorker { get; set; }

        private Dictionary<int, DateTime> emailsEnviados = new Dictionary<int, DateTime>();

        public QualificaRules()
        {
            this.receitaWorker = new ReceitaWorker();
            this.siteWorker = new SiteWorker();
            this.telefoneWorker = new TelefoneWorker();

            this.receitaWorker.onEmpty += new FilaDelegate(onReceitaEmpty);
            this.receitaWorker.onSuccess += new ProjetoDelegate(onReceitaSuccess);

            this.siteWorker.onSuccess += new ProjetoDelegate(onSiteSuccess);

            this.telefoneWorker.onQualificado += new ProjetoDelegate(onTelefoneQualificado);
            this.telefoneWorker.onNaoQualificado += new ProjetoDelegate(onTelefoneNaoQualificado);
            this.telefoneWorker.onForceQualificado += new ProjetoDelegate(onForceQualificado);
        }

        public void start()
        {
            Log.WriteLine("Qualificacao Iniciada");

            this.receitaWorker.start();
            this.siteWorker.start();
            this.telefoneWorker.start();
        }

        private void checkTotalEmpresas(Projeto projeto)
        {
            var totalEmpresasQualificadas = ProspectQualificadoRepositorio.fetchTotalEmpresasQualificadas(projeto.prospect);
            var empresas = projeto.empresas;
            if (projeto.totalEmpresasEncontradas + totalEmpresasQualificadas < projeto.parametroPesquisa.quantidade_prospect && !projeto.isForceProspeccao)
            {
                this.enviarEmail(projeto, empresas);
            }
        }

        private void enviarEmail(Projeto projeto, List<Empresa> empresas)
        {
            if (Config.read("Qualifica", "send_email_insulficientes") == "false") return;

            var b = this.emailsEnviados.ContainsKey(projeto.id);
            if (b)
            {
                var dataUltimoEmailEnviado = this.emailsEnviados[projeto.id];
                if (dataUltimoEmailEnviado != null && DateTime.Now.Subtract(dataUltimoEmailEnviado).Hours < 1) return;
            }
            else
            {
                this.emailsEnviados.Add(projeto.id, DateTime.Now);
            }

            this.emailsEnviados[projeto.id] = DateTime.Now;

            Log.WriteLine(String.Format("[Qualify] Prospects Insuficientes {0}", projeto.getParteName()));

            var email = new Email
            {
                assunto = "Prospects Insuficientes",
                mensagem = String.Format(
                    "Ops, aqui é o Robotys acho que temos um pequeno problema. Não foi encontrado o número de leads necessário em nossa base para qualificar. Isso é pode ser resolvido por você meu amigo.<br>" +
                    "Nome do projeto = " + projeto.nome_perfil + "<br>" +
                    "Projeto = " + projeto.id + "<br>" +
                    "Prospect = " + projeto.prospect.id + "<br>" +
                    "Total Empresas Solicitadas = " + projeto.parametroPesquisa.quantidade_prospect + "<br>" +
                    "Total Empresas Restantes = " + projeto.parametroPesquisa.totalEmpresaRestantes + "<br>" +
                    "Total Empresas Encontradas na base = " + empresas.Count + "<br>" +
                    "Total Empresas Necessarias inserir na base = " + (projeto.parametroPesquisa.totalEmpresaRestantes - empresas.Count)
                )
            };

            email.send();
        }

        public void onReceitaEmpty(Fila fila)
        {
            var projetos = BreakProject.atLimit(this.onEmpty());            

            fila.exclusive(p => {
                var total = 0;
                for (int i = 0; i < projetos.Count; i++)
                {
                    var projeto = projetos[i];
                    
                    checkTotalEmpresas(projeto);

                    if (projeto.empresas.Count == 0) continue;
                    if (this.siteWorker.existInWork(projeto)) continue;
                    if (this.telefoneWorker.existInWork(projeto)) continue;

                    fila.add(projeto);

                    total++;
                }

                if (total > 0) Log.WriteLine(String.Format("[Qualify] {0} projeto(s) inserido(s) na fila na receita", total));
            });
        }

        public void onReceitaSuccess(Projeto projeto)
        {
            Log.WriteLine(String.Format("[Qualify] projeto {0} adicionado a fila do site", projeto.getParteName()));

            this.siteWorker.fila.add(projeto);

            var empresas = projeto.empresas;
            foreach (var e in empresas)
            {
                if (e.wasUpdatedReceita)
                {
                    EmpresaRepositorio.update(e);
                    EmpresaContatoRepositorio.update(e.contato);
                    EmpresaRepositorio.updateSocios(e);
                    EmpresaRepositorio.updateDataAtualizacaoReceita(e);
                }
            }
        }

        public void onSiteSuccess(Projeto projeto)
        {
            Log.WriteLine(String.Format("[Qualify] projeto {0} adicionado a fila do telefone", projeto.getParteName()));

            this.telefoneWorker.fila.add(projeto);

            var empresas = projeto.empresas;
            foreach (var e in empresas)
            {
                if (e.wasUpdatedSite)
                {
                    if (e.contato.emails != null) e.contato.emails = String.Join(",", Higienize.removeContabilEmail(e.contato.emails.Split(',')));

                    EmpresaRepositorio.update(e);
                    EmpresaContatoRepositorio.update(e.contato);
                    EmpresaRepositorio.updateDataAtualizacaoSite(e);
                }
            }
        }

        public void onTelefoneQualificado(Projeto projeto)
        {            
            this.onQualificado(projeto);
        }

        public void onTelefoneNaoQualificado(Projeto projeto)
        {
            this.onNaoQualificado(projeto);
        }

        public void onForceQualificado(Projeto projeto)
        {
            this.onForceQualificacao(projeto);
        }

    }
}
