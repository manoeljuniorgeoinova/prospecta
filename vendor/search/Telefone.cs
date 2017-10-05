using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using resultys.prospecta.models;
using resultys.prospecta.lib;

using botnet.server;
using botnet.library;
using botnet.client;
using botnet.model;

namespace resultys.prospecta.vendor.search
{
    public class Telefone
    {
        private Semaphore semaphoreGoogle;
        private Semaphore semaphoreSite;
        private Semaphore semaphoreTelelista;
        private Semaphore semaphoreGuiaMais;
        private Semaphore semaphoreApontador;

        private object lockthis;
        private Web server;

        private Network networkGoogle { get; set; }
        private Network networkSite { get; set; }
        private Network networkTelelista { get; set; }
        private Network networkGuiaMais { get; set; }
        private Network networkApontador { get; set; }

        private NetworkClient networkClient { get; set; }
        private MasterClient masterClient { get; set; }
        private SiteClient siteClient { get; set; }

        public List<Empresa> empresas { get; set; }

        public Telefone()
        {
            this.lockthis = new object();

            this.masterClient = new MasterClient
            {
                ip = resultys.prospecta.lib.Config.read("MasterVirginia", "ip"),
                port = int.Parse(resultys.prospecta.lib.Config.read("MasterVirginia", "port"))
            };

            this.siteClient = new SiteClient {
                ip = resultys.prospecta.lib.Config.read("BotSite", "ip"),
                port = int.Parse(resultys.prospecta.lib.Config.read("BotSite", "port"))
            };

            this.networkClient = new NetworkClient(this.masterClient);
            this.networkGoogle = this.networkClient.get("captura_telefone_google");
            this.networkSite = this.networkClient.get("captura_telefone_site");
            this.networkTelelista = this.networkClient.get("captura_telefone_telelista");
            this.networkGuiaMais = this.networkClient.get("captura_telefone_guiamais");
            this.networkApontador = this.networkClient.get("captura_telefone_apontador");

            this.semaphoreGoogle = new Semaphore("google", this.networkGoogle.totalBots);
            this.semaphoreSite = new Semaphore("site", this.networkSite.totalBots);
            this.semaphoreTelelista = new Semaphore("telelista", this.networkTelelista.totalBots);
            this.semaphoreGuiaMais = new Semaphore("guiamas", this.networkGuiaMais.totalBots);
            this.semaphoreApontador = new Semaphore("apontador", this.networkApontador.totalBots);

            this.server = new Web {
                port = int.Parse(resultys.prospecta.lib.Config.read("TelefoneService", "port"))
            };

            this.server.addRoute("/telefone/set/google", setTelefonesGoogle);
            this.server.addRoute("/telefone/set/site", setTelefonesSite);
            this.server.addRoute("/telefone/set/telelista", setTelefonesTelelista);
            this.server.addRoute("/telefone/set/guiamais", setTelefonesGuiaMais);
            this.server.addRoute("/telefone/set/apontador", setTelefonesApontador);
        }

        private string setTelefonesApontador(QueryString qs, string postData)
        {
            var empresas = new List<Empresa>();

            try
            {
                empresas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Empresa>>(postData);
            }
            catch { }

            lock (lockthis)
            {                
                this.populeTelefone(empresas, TelefoneFonte.APONTADOR);
                this.semaphoreApontador.release();
            }

            return "";
        }

        private string setTelefonesGoogle(QueryString qs, string postData)
        {
            var empresas = new List<Empresa>();

            try
            {
                empresas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Empresa>>(postData);
            }
            catch { }

            lock (lockthis)
            {
                this.populeRedeSocial(empresas);
                this.populeTelefone(empresas, TelefoneFonte.GOOGLE);
                this.semaphoreGoogle.release();
            }

            return "";
        }

        private string setTelefonesSite(QueryString qs, string postData)
        {
            var empresas = new List<Empresa>();

            try
            {
                empresas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Empresa>>(postData);
            }
            catch { }

            lock (lockthis)
            {                
                this.populeTelefone(empresas, TelefoneFonte.SITE);
                this.semaphoreSite.release();
            }

            return "";
        }

        private string setTelefonesTelelista(QueryString qs, string postData)
        {
            var empresas = new List<Empresa>();

            try
            {
                empresas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Empresa>>(postData);
            }
            catch { }

            lock (lockthis)
            {
                this.populeTelefone(empresas, TelefoneFonte.TELELISTA);
                this.semaphoreTelelista.release();
            }

            return "";
        }

        private string setTelefonesGuiaMais(QueryString qs, string postData)
        {
            var empresas = new List<Empresa>();

            try
            {
                empresas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Empresa>>(postData);
            }
            catch { }

            lock (lockthis)
            {
                this.populeTelefone(empresas, TelefoneFonte.GUIAMAIS);
                this.semaphoreGuiaMais.release();
            }

            return "";
        }

        public List<Empresa> pesquisar(List<Empresa> empresas)
        {
            this.empresas = empresas;

            this.siteClient.setVariable(new Variable
            {
                name = "empresas_prospecta",
                value = Newtonsoft.Json.JsonConvert.SerializeObject(empresas)
            });

            this.siteClient.setVariable(new Variable
            {
                name = "prospecta_service",
                value = String.Format("{0}:{1}", resultys.prospecta.lib.Config.read("App", "ip"), resultys.prospecta.lib.Config.read("TelefoneService", "port"))
            });

            this.server.start();

            this.semaphoreGoogle.reset();
            this.semaphoreTelelista.reset();
            this.semaphoreGuiaMais.reset();
            this.semaphoreApontador.reset();

            this.startNetworks(new List<Network> {
                this.networkGoogle,
                this.networkTelelista,
                this.networkGuiaMais,
                this.networkApontador
            });
            
            this.semaphoreGoogle.wait();
            this.semaphoreTelelista.wait();
            this.semaphoreGuiaMais.wait();
            this.semaphoreApontador.wait();

            this.semaphoreSite.reset();
            this.siteClient.setVariable(new Variable
            {
                name = "empresas_prospecta",
                value = Newtonsoft.Json.JsonConvert.SerializeObject(this.empresas)
            });
            this.startNetworks(new List<Network> { this.networkSite });
            this.semaphoreSite.wait();

            this.server.stop();

            return empresas;
        }

        private void startNetworks(List<Network> nets)
        {
            foreach (var net in nets)
            {
                var thread = new System.Threading.Thread(startNetwork);
                thread.Start(net);
            }
        }

        private void startNetwork(object param)
        {
            var net = param as Network;

            this.networkClient.start(net.id);
        }

        private void populeRedeSocial(List<Empresa> empresas) {
            foreach (var empresa in empresas)
            {
                var e = this.empresas.Find(_e => _e.compareTo(empresa));

                if (e == null) continue;

                e.contato = empresa.contato;
            }
        }


        private void populeTelefone(List<Empresa> empresas, TelefoneFonte tipo)
        {
            foreach (var empresa in empresas)
            {
                var e = this.empresas.Find(_e => _e.compareTo(empresa));

                if (e == null) continue;
                if (empresa.telefones == null) continue;

                e.wasUpdatedSite = true;

                if (empresa.contato != null) {
                    if (e.contato == null) e.contato = new EmpresaContato();

                    e.contato.facebook = empresa.contato.facebook;
                    e.contato.twitter = empresa.contato.twitter;
                    e.contato.linkedin = empresa.contato.linkedin;
                    e.contato.sites = empresa.contato.sites;

                    if(tipo == TelefoneFonte.SITE)
                    {
                        e.contato.emails = empresa.contato.emails;
                    }
                }
                
                foreach (var telefone in empresa.telefones)
                {
                    if (e.telefones == null) e.telefones = new List<models.Telefone>();
                    if (telefone == null) continue;

                    telefone.fonte = tipo;

                    e.telefones.Add(telefone);
                }
            }
        }

    }
}
