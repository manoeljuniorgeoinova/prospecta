using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.models
{
    public class Cliente
    {
        public int id { get; set; }
        public string tipo { get; set; }

        [PetaPoco.Ignore] public bool isDemonstrativo {
            get {
                return this.tipo == "D";
            }
        }
    }
}