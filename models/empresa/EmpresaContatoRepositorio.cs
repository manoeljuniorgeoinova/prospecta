using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using resultys.prospecta.lib;

namespace resultys.prospecta.models
{
    public class EmpresaContatoRepositorio
    {
        public static EmpresaContato insert(EmpresaContato contato)
        {
            contato.id = (int)Repositorio.insert("empresa.empresa_contato", "id", new
            {
                id_empresa = contato.id_empresa,
                protocol = Protocol.serialize(contato.chamadas),
                emails = contato.emails,
                sites = contato.sites,
                facebook = contato.facebook,
                linkedin = contato.linkedin,
                twitter = contato.twitter,
                cnpj = contato.cnpj
            });

            return contato;
        }

        public static EmpresaContato fetchOne(string cnpj)
        {
            var sql = PetaPoco.Sql.Builder.Append("SELECT * FROM empresa.empresa_contato WHERE cnpj = @0", cnpj);

            return Repositorio.fetchOne<EmpresaContato>(sql);
        }

        public static void delete(string cnpj)
        {
            var sql = PetaPoco.Sql.Builder.Append("DELETE FROM empresa.empresa_contato WHERE cnpj = @0", cnpj);
            Repositorio.execute(sql);
        }

        public static void delete(EmpresaContato contato)
        {
            EmpresaContatoRepositorio.delete(contato.cnpj);
        }

        public static void update(EmpresaContato contato)
        {
            EmpresaContatoRepositorio.delete(contato);
            EmpresaContatoRepositorio.insert(contato);
        }
    }
}
