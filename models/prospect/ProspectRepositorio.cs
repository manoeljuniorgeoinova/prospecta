using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class ProspectRepositorio
    {
        public static Prospect insert(Prospect prospect)
        {
            prospect.id = Repositorio.insert("prospect.prospect", "id", prospect);

            return prospect;
        }

        public static void update(Prospect prospect)
        {
            Repositorio.update("prospect.prospect", "id", prospect);
        }

        public static Prospect fetchOne(int projetoId, string status)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT * FROM prospect.prospect WHERE id_projeto = @0 AND status = @1", projetoId, status);

            return Repositorio.fetchOne<Prospect>(sql);
        }
    }
}
