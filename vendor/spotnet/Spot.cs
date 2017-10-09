using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using spotnet.bot;
using spotnet.core;
using spotnet.lib;

using resultys.prospecta.models;

namespace resultys.prospecta.vendor.spotnet
{
    public class Spot
    {
        public GoogleBot google { get; set; }
        public ApontadorBot apontador { get; set; }
        public GuiaMaisBot guiaMais { get; set; }
        public TelelistaBot telelista { get; set; }
        public SitesBot sites { get; set; }

        public List<Empresa> empresas { get; set; }

        public object lockthis { get; set; }

        public Spot(string url)
        {
            this.google = new GoogleBot { url = url };
            this.apontador = new ApontadorBot { url = url };
            this.guiaMais = new GuiaMaisBot { url = url };
            this.telelista = new TelelistaBot { url = url };
            this.sites = new SitesBot { url = url };

            this.lockthis = new object();
        }

        public List<Empresa> searchDados(List<Empresa> empresas)
        {
            this.empresas = empresas;

            var threads = new List<Thread>();

            threads.Add(this.createThread(this.google, empresas, TelefoneFonte.GOOGLE));
            //threads.Add(this.createThread(this.apontador, empresas, TelefoneFonte.APONTADOR));
            //threads.Add(this.createThread(this.guiaMais, empresas, TelefoneFonte.GUIAMAIS));
            //threads.Add(this.createThread(this.telelista, empresas, TelefoneFonte.TELELISTA));

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            var th = this.createThread(this.sites, empresas, TelefoneFonte.SITE);
            th.Start();
            th.Join();

            return empresas;
        }

        private Thread createThread(IBot bot, List<Empresa> empresas, TelefoneFonte fonte)
        {
            return new Thread(() =>
            {
                var es = bot.run<Empresa>(empresas);
                lock (this.lockthis)
                {
                    this.populeTelefone(es, fonte);
                }
            });
        }

        private void populeTelefone(List<Empresa> empresas, TelefoneFonte tipo)
        {
            foreach (var empresaAtualizada in empresas)
            {
                var empresa = this.empresas.Find(_e => _e.compareTo(empresaAtualizada));

                if (empresa == null) continue;
                if (empresaAtualizada.telefones == null) continue;

                empresa.wasUpdatedSite = true;

                if (empresaAtualizada.contato != null)
                {
                    if (empresa.contato == null) empresa.contato = new EmpresaContato();

                    if(tipo == TelefoneFonte.GOOGLE)
                    {
                        empresa.contato.facebook = empresaAtualizada.contato.facebook;
                        empresa.contato.twitter = empresaAtualizada.contato.twitter;
                        empresa.contato.linkedin = empresaAtualizada.contato.linkedin;
                        empresa.contato.sites = empresaAtualizada.contato.sites;
                    }                    

                    if (tipo == TelefoneFonte.SITE)
                    {
                        empresa.contato.emails = empresaAtualizada.contato.emails;
                        empresa.contato.sites = empresaAtualizada.contato.sites;
                    }
                }

                foreach (var telefone in empresaAtualizada.telefones)
                {
                    if (empresa.telefones == null) empresa.telefones = new List<models.Telefone>();
                    if (telefone == null) continue;

                    telefone.fonte = tipo;

                    empresa.telefones.Add(telefone);
                }
            }
        }

    }
}
