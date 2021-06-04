using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Treinamento.Domain;

namespace Treinamento.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //QuerySplittingBehavior este comando faz a divisão de consulta quando tiver relacionamento
            const string strConnection = "Server=DESKTOP-N8415MR\\SQLEXPRESS;Database=DominandoEFCore;User Id=sa;Password=root;pooling=true;";
            optionsBuilder
                //.UseSqlServer(strConnection,p=>p.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .UseSqlServer(strConnection)
                .EnableSensitiveDataLogging()
                //.UseLazyLoadingProxies()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }

        //usando um filtro global para não listar os registros de forma a não trazer registros excluidos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Departamento>().HasQueryFilter(p=>!p.Excluido);
        }


    }
}
