using System;
using System.Collections.Generic;
using System.Linq;

using botnet.server;
using botnet.library;
using botnet.client;
using botnet.model;

using resultys.prospecta.models;
using resultys.prospecta.lib;

namespace resultys.prospecta.vendor.receita
{
    public class Receita
    {
        private Web server { get; set; }

        private Network network { get; set; }

        private NetworkClient networkClient { get; set; }
        private MasterClient masterClient { get; set; }
        private SiteClient siteClient { get; set; }

        private static object lockthis = new object();

        private Semaphore semaphore;
        private List<Empresa> empresas;
        private List<Empresa> currentEmpresas;

        public Receita()
        {
            this.masterClient = new MasterClient
            {
                ip = resultys.prospecta.lib.Config.read("MasterSaoPaulo", "ip"),
                port = int.Parse(resultys.prospecta.lib.Config.read("MasterSaoPaulo", "port"))
            };

            this.siteClient = new SiteClient
            {
                ip = resultys.prospecta.lib.Config.read("BotSite", "ip"),
                port = int.Parse(resultys.prospecta.lib.Config.read("BotSite", "port"))
            };

            this.networkClient = new NetworkClient(this.masterClient);
            this.network = this.networkClient.get("captura_dados_receita");

            this.semaphore = new Semaphore(this.network.totalBots);            

            this.server = new Web {
                port = int.Parse(resultys.prospecta.lib.Config.read("ReceitaService", "port"))
            };

            this.server.addRoute("/receita/set/empresas", setEmpresas);
        }

        private string setEmpresas(QueryString qs, string postData)
        {
            var botnumber = qs.get("botnumber");
            var empresas = new List<Empresa>();
            var empresasValidas = new List<Empresa>();

            try
            {
                empresas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Empresa>>(postData);
            }
            catch (Exception e)
            {
                Log.WriteLine(e.Message);
            }
            
            if (empresas == null) empresas = new List<Empresa>();

            lock (lockthis)
            {
                try
                {
                    foreach (var empresa in empresas)
                    {
                        if (empresa == null) continue;
                        if (empresa.cnpj == null) continue;

                        empresa.wasUpdatedReceita = true;

                        var e = this.currentEmpresas.Find(_e => _e.compareTo(empresa));

                        if (empresa.socios != null && empresa.socios.Count > 0) e.socios = empresa.socios;
                        if (empresa.email != null && empresa.email.Length > 0) e.email = empresa.email;
                        
                        var telefones = new List<Telefone>();
                        if (empresa.telefones == null) empresa.telefones = telefones;

                        empresa.contato = e.contato;

                        foreach (var telefone in empresa.telefones)
                        {
                            if (telefone == null) continue;

                            telefone.fonte = TelefoneFonte.RECEITA;
                            telefones.Add(telefone);
                        }
                        
                        empresasValidas.Add(empresa);
                    }
                }
                catch (Exception e)
                {
                    Log.WriteLine(String.Format("Receita Error: {0}", e.Message));
                }

                this.empresas = this.empresas.Concat(empresasValidas).ToList();

                this.semaphore.release();
            }

            return "";
        }

        public List<Empresa> buscarInformacoes(List<Empresa> empresas)
        {
            this.empresas = new List<Empresa>();

            this.currentEmpresas = empresas;

            this.siteClient.setVariable(new Variable
            {
                name = "empresas_prospecta_receita",
                value = Newtonsoft.Json.JsonConvert.SerializeObject(empresas)
            });

            this.siteClient.setVariable(new Variable
            {
                name = "prospecta_service_receita",
                value = String.Format("{0}:{1}", resultys.prospecta.lib.Config.read("App", "ip"), resultys.prospecta.lib.Config.read("ReceitaService", "port"))
            });

            this.networkClient.start(this.network.id);

            this.server.start();

            this.semaphore.wait();

            this.server.stop();

            return this.empresas;
        }

        private string convertEmpresasToList(List<Empresa> empresas)
        {
            var list = new List<string>();

            foreach (var empresa in empresas)
            {
                list.Add(empresa.getRawCnpj());
            }

            return String.Join(",", list);
        }

    }
}
