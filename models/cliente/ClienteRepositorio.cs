using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class ClienteRepositorio
    {

        public static Cliente fetch(int id)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT * FROM cliente.cliente WHERE id = @0", id);

            return Repositorio.fetchOne<Cliente>(sql);
        }

    }
}
