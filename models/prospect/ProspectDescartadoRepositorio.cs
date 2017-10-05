using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class ProspectDescartadoRepositorio
    {
        public static ProspectDescartado insert(ProspectDescartado prospect)
        {
            prospect.id = Repositorio.insert("prospect.prospect_descartado", "id", prospect);

            return prospect;
        }

        public static bool exist(string cnpj)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT COUNT(*) FROM prospect.prospect_descartado WHERE cnpj = @0", cnpj);

            return Repositorio.exist(sql);
        }

        public static void delete(string cnpj)
        {
            var sql = PetaPoco.Sql.Builder.Append("DELETE FROM prospect.prospect_descartado WHERE cnpj = @0", cnpj);

            Repositorio.execute(sql);
        }
    }
}
