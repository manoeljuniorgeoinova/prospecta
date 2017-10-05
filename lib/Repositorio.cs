using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.lib
{
    public class Repositorio
    {

        private static object objLock = new object();
        public PetaPoco.Database db { get; set; }
        private static Repositorio __repositorio = null;

        public Repositorio()
        {
            this.db = new PetaPoco.Database(Config.read("Database", "cs"), Config.read("Database", "provider"));
        }

        public static Repositorio getInstance()
        {
            if (__repositorio == null) refresh();

            return __repositorio;
        }

        public static void refresh()
        {
            __repositorio = new Repositorio();
        }

        public static bool exist(PetaPoco.Sql sql)
        {
            lock (Repositorio.objLock)
            {
                while (true)
                {
                    try
                    {
                        return Repositorio.getInstance().db.ExecuteScalar<bool>(sql);
                    }
                    catch (Exception ex) { Log.WriteLine(String.Format("Repositorio exist: {0}", ex.Message)); refresh(); System.Threading.Thread.Sleep(2000); }
                }
            }
        }

        public static int count(PetaPoco.Sql sql)
        {
            lock (Repositorio.objLock)
            {
                while (true)
                {
                    try
                    {
                        return Repositorio.getInstance().db.ExecuteScalar<int>(sql);
                    }
                    catch (Exception ex) { Log.WriteLine(String.Format("Repositorio exist: {0}", ex.Message)); refresh(); System.Threading.Thread.Sleep(2000); }
                }
            }
        }

        public static void update(string table, string id, object obj)
        {
            lock (Repositorio.objLock)
            {
                while (true)
                {
                    try
                    {
                        Repositorio.getInstance().db.Update(table, id, obj);
                        break;
                    }
                    catch (Exception ex) { Log.WriteLine(String.Format("Repositorio update: {0}", ex.Message)); refresh(); System.Threading.Thread.Sleep(2000); }
                }
            }
            
        }

        public static void execute(PetaPoco.Sql sql)
        {
            lock (Repositorio.objLock)
            {
                while (true)
                {
                    try
                    {
                        Repositorio.getInstance().db.Execute(sql);
                        break;
                    }
                    catch (Exception ex) { Log.WriteLine(String.Format("Repositorio execute: {0}", ex.Message)); refresh(); System.Threading.Thread.Sleep(2000); }
                }
            }
        }

        public static int insert(string table, string id, object obj)
        {
            lock (Repositorio.objLock)
            {
                while (true)
                {
                    try
                    {
                        return (int)Repositorio.getInstance().db.Insert(table, id, obj);
                    }
                    catch (Exception ex) { Log.WriteLine(String.Format("Repositorio insert: {0}", ex.Message)); refresh(); System.Threading.Thread.Sleep(2000); }
                }

            }
        }

        public static T fetchOne<T>(PetaPoco.Sql sql)
        {
            lock (Repositorio.objLock)
            {
                while (true)
                {
                    try
                    {
                        return Repositorio.getInstance().db.SingleOrDefault<T>(sql);
                    }
                    catch (Exception ex) { Log.WriteLine(String.Format("Repositorio fetchOne: {0}", ex.Message)); refresh(); System.Threading.Thread.Sleep(2000); }
                }
            }
        }
        
        public static List<R> fetch<T1, T2, R>(Func<T1, T2, R> fn, PetaPoco.Sql sql)
        {
            lock (Repositorio.objLock)
            {
                while (true)
                {
                    try
                    {
                        return Repositorio.getInstance().db.Fetch<T1, T2, R>(fn, sql);
                    }
                    catch (Exception ex) { Log.WriteLine(String.Format("Repositorio fetch: {0}", ex.Message)); refresh(); System.Threading.Thread.Sleep(2000); }
                }
            }
        }

        public static List<R> fetch<T1, T2, T3, R>(Func<T1, T2, T3, R> fn, PetaPoco.Sql sql)
        {
            lock (Repositorio.objLock)
            {
                while (true)
                {
                    try
                    {
                        return Repositorio.getInstance().db.Fetch<T1, T2, T3, R>(fn, sql);
                    }
                    catch (Exception ex) { Log.WriteLine(String.Format("Repositorio fetch: {0}", ex.Message)); refresh(); System.Threading.Thread.Sleep(2000); }
                }
            }
        }
    }
}
