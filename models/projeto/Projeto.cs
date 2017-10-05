using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class Projeto
    {
        public int id { get; set; }
        public int id_usuario { get; set; }
        public int id_cliente { get; set; }

        public string nome_perfil { get; set; }        
        public string status { get; set; }        
        public bool isForceProspeccao { get; set; }
        public string webhook { get; set; }

        [PetaPoco.Ignore] public Prospect prospect { get; set; }
        [PetaPoco.Ignore] public List<Empresa> empresas { get; set; }
        [PetaPoco.Ignore] public Pesquisa parametroPesquisa { get; set; }

        [PetaPoco.Ignore] public int numerador { get; set; }
        [PetaPoco.Ignore] public int denominador { get; set; }
        [PetaPoco.Ignore] public string parte { get; set; }
        [PetaPoco.Ignore] public int totalEmpresasEncontradas { get; set; }

        [PetaPoco.Ignore] public ConfigureMotor config { get; set; }
        [PetaPoco.Ignore] public Cliente cliente { get; set; }

        public string getParteName()
        {
            return String.Format("({0} {1}/{2}{3})", this.id, this.numerador, this.denominador, this.parte);
        }
        
        public Projeto clone()
        {
            var p = new Projeto();

            p.id = this.id;
            p.id_usuario = this.id_usuario;
            p.id_cliente = this.id_cliente;

            p.nome_perfil = this.nome_perfil;            
            p.status = this.status;            
            p.isForceProspeccao = this.isForceProspeccao;

            p.prospect = this.prospect;
            p.empresas = this.empresas;
            p.parametroPesquisa = this.parametroPesquisa;

            p.numerador = this.numerador;
            p.denominador = this.denominador;
            p.parte = this.parte;

            p.config = this.config;

            p.totalEmpresasEncontradas = this.totalEmpresasEncontradas;

            p.webhook = this.webhook;
            p.cliente = this.cliente;

            return p;
        }
    }
}
