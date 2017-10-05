using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.models;

namespace resultys.prospecta.lib
{
    public class BreakProject
    {

        public static Projeto[] at(Projeto projeto, int limit)
        {
            var p1 = projeto.clone();
            var p2 = projeto.clone();

            p1.empresas = new List<Empresa>();
            p2.empresas = new List<Empresa>();

            for (int i = 0; i < limit; i++)
            {
                p1.empresas.Add(projeto.empresas[i]);
            }

            for (int i = limit; i < projeto.empresas.Count; i++)
            {
                p2.empresas.Add(projeto.empresas[i]);
            }

            return new Projeto[2] { p1, p2 };
        }

        public static Projeto[] atEmpresasAtualizadasSite(Projeto projeto)
        {
            var quantidadeDias = projeto.config.validate_site;
            var empresasDesatualizadas = projeto.empresas.FindAll(e => DateTime.Now.Subtract(e.data_ultima_atualizacao_site).TotalDays > quantidadeDias);
            var empresasAtualizadas = projeto.empresas.FindAll(e => DateTime.Now.Subtract(e.data_ultima_atualizacao_site).TotalDays <= quantidadeDias);

            var p1 = projeto.clone();
            var p2 = projeto.clone();

            p1.empresas = new List<Empresa>();
            p2.empresas = new List<Empresa>();

            for (int i = 0; i < empresasDesatualizadas.Count; i++)
            {
                p1.empresas.Add(empresasDesatualizadas[i]);
            }

            for (int i = 0; i < empresasAtualizadas.Count; i++)
            {
                p2.empresas.Add(empresasAtualizadas[i]);
            }

            if (p1.parte == null) p1.parte = "";
            if (p2.parte == null) p2.parte = "";

            p1.parte += "AS";

            return new Projeto[2] { p1, p2 };
        }

        public static Projeto[] atEmpresasAtualizadasReceita(Projeto projeto)
        {
            var quantidadeDias = projeto.config.validade_receita;
            var empresasDesatualizadas = projeto.empresas.FindAll(e => DateTime.Now.Subtract(e.data_ultima_atualizacao).TotalDays > quantidadeDias);
            var empresasAtualizadas = projeto.empresas.FindAll(e => DateTime.Now.Subtract(e.data_ultima_atualizacao).TotalDays <= quantidadeDias);

            var p1 = projeto.clone();
            var p2 = projeto.clone();

            p1.empresas = new List<Empresa>();
            p2.empresas = new List<Empresa>();

            for (int i = 0; i < empresasDesatualizadas.Count; i++)
            {
                p1.empresas.Add(empresasDesatualizadas[i]);
            }

            for (int i = 0; i < empresasAtualizadas.Count; i++)
            {
                p2.empresas.Add(empresasAtualizadas[i]);
            }

            if (p1.parte == null) p1.parte = "";
            if (p2.parte == null) p2.parte = "";

            p1.parte += "AR";            

            return new Projeto[2] { p1, p2 };
        }

        public static List<Projeto> atLimiteOne(Projeto projeto)
        {
            var limit = int.Parse(Config.read("Break", "limit"));
            var list = new List<Projeto>();
            var clone = projeto.clone();
            clone.empresas = new List<Empresa>();

            projeto.numerador = 1;
            projeto.denominador = 1;

            if (projeto.empresas.Count <= limit) return new List<Projeto> { projeto };
            
            list.Add(clone);
            for (int i = 0; i < projeto.empresas.Count; i++)
            {
                clone.empresas.Add(projeto.empresas[i]);

                if (clone.empresas.Count == limit)
                {
                    clone = clone.clone();
                    clone.empresas = new List<Empresa>();
                    list.Add(clone);
                }
            }

            if (list[list.Count - 1].empresas.Count == 0) list.Remove(list[list.Count - 1]);

            for (int i = 0; i < list.Count; i++)
            {
                list[i].numerador = i + 1;
                list[i].denominador = list.Count;
            }

            return list;
        }

        public static List<Projeto> atLimit(List<Projeto> projetos)
        {
            var ps = new List<Projeto>();

            for (int i = 0; i < projetos.Count; i++)
            {
                var projeto = projetos[i];
                ps = ps.Concat(BreakProject.atLimiteOne(projeto)).ToList();
            }

            return ps;
        }

    }
}
