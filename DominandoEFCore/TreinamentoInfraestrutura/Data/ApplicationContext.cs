using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using TreinamentoInfraestrutura.Domain;

namespace TreinamentoInfraestrutura.Data
{
    public class ApplicationContext : DbContext
    {
        //Gravando seus logs em um arquivo | append = usar o mesmo log
        private readonly StreamWriter _writer = new StreamWriter("meu_log_do_ef_core.txt", append: true);

        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //QuerySplittingBehavior este comando faz a divisão de consulta quando tiver relacionamento
            const string strConnection = "Server=DESKTOP-N8415MR\\SQLEXPRESS;Database=DominandoEFCore;User Id=sa;Password=root;pooling=true;";
            optionsBuilder
                .UseSqlServer(
                    strConnection, o => o
                            .MaxBatchSize(100) //enviar muitos registros em uma única massa Habilitando Batch Size
                            .CommandTimeout(5)
                            .EnableRetryOnFailure(4, TimeSpan.FromSeconds(10), null)) //Habilitando resiliência para sua aplicação
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging();

            // optionsBuilder
            // .UseSqlServer(strConnection)
            //.LogTo(Console.WriteLine);                       // Configurando um log simplificado
            //.LogTo(Console.WriteLine, LogLevel.Information); //visualizando apenas os logs do tipo Information
            //.LogTo(Console.WriteLine, new[] {CoreEventId.ContextInitialized, RelationalEventId.CommandExecuted},
            //  LogLevel.Information,
            //  DbContextLoggerOptions.LocalTime | DbContextLoggerOptions.SingleLine
            //);                                               //Filtrando eventos de seus logs
            //.LogTo(_writer.WriteLine, LogLevel.Information); //Gravando seus logs em um arquivo
            //.EnableDetailedErrors(); //Habilitando erros detalhados
            //.EnableSensitiveDataLogging();//Habilitando visualização dos dados sensíveis

        }
        

        public override void Dispose()
        {
            base.Dispose();
            _writer.Dispose();
        }

    }
}
