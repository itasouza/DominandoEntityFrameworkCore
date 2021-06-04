using System;

namespace TreinamentoModeloDeDados
{
    class Program
    {
        static void Main(string[] args)
        {
            Collations();
        }


        static void Collations()
        {
            using var db = new TreinamentoModeloDeDados.Data.ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }


    }
}
