using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using resultys.prospecta.vendor.receita;
using resultys.prospecta.vendor.search;
using resultys.prospecta.models;
using resultys.prospecta.rules;
using resultys.prospecta.lib;
using resultys.prospecta.worker;

namespace resultys.prospecta
{
    class Program
    {

        static void Main(string[] args)
        {
            Log.WriteLine("==Prospecta 2.0");
            var prospectRules = new ProspectRules();
            prospectRules.prospectar();
        }

        //static void Main(string[] args)
        //{
        //    var projeto = ProjetoRepositorio.fetch(449);
        //    projeto.empresas = new List<Empresa>();

        //    for (int i = 0; i < 21; i++)
        //    {
        //        projeto.empresas.Add(new Empresa() { cnpj = (i + 1).ToString() });
        //    }

        //    var projetos = BreakProject.atLimit(projeto);
        //}
    }

    //    static void Main(string[] args)
    //    {
    //        Log.WriteLine("==Prospecta 2.0");

    //        while (true)
    //        {
    //            var prospectRules = new ProspectRules();
    //            prospectRules.prospectar();

    //            System.Threading.Thread.Sleep(4 * 60 * 1000);
    //        }
    //    }   
    //}
}
