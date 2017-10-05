using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class ProspectQualificadoRepositorio
    {
        public static bool exist(Prospect prospect, string cnpj)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT COUNT(*) FROM prospect.prospect_qualificado WHERE id_prospect = @0 AND cnpj = @1", prospect.id, cnpj);

            return Repositorio.exist(sql);
        }

        public static ProspectQualificado insert(ProspectQualificado prospect)
        {
            prospect.id = Repositorio.insert("prospect.prospect_qualificado", "id", prospect);

            return prospect;
        }

        public static int fetchTotalEmpresasQualificadas(Prospect prospect)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT COUNT(*) FROM prospect.prospect_qualificado WHERE id_prospect = @0", prospect.id);

            return Repositorio.count(sql);
        }
    }
}
