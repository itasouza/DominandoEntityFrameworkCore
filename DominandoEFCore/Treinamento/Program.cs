using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Treinamento
{
    class Program
    {
        static void Main(string[] args)
        {
            // EnsureCreatedAndDeleted();
            //GapDoEnsureCreated();
            //HealthCheckBancoDeDados();
            //ExecuteSQL();
            //SqlInjection();
            //MigracoesPendentes();
            //AplicarMigracaoEmTempodeExecucao();
            // TodasMigracoes();
            // MigracoesJaAplicadas();
            //ScriptGeralDoBancoDeDados();
            //CarregamentoAdiantado();
            //CarregamentoExplicito();
            //CarregamentoLento();

            // FiltroGlobal();
            //IgnoreFiltroGlobal();
            // ConsultaProjetada();
            // ConsultaParametrizada();
            // ConsultaInterpolada();
            //ConsultaComTAG();
            //EntendendoConsulta1NN1();
            //DivisaoDeConsulta();
            // CriarStoredProcedure();
            //InserirDadosViaProcedure();
            //CriarStoredProcedureDeConsulta();
            ConsultaViaProcedure();

            //new Treinamento.Data.ApplicationContext().Departamentos.AsNoTracking().Any();
            //_count=0;
            //GerenciarEstadoDaConexao(false);
            //_count=0;
            //GerenciarEstadoDaConexao(true);

        }

        //consultar dados atraves de procedure
        static void ConsultaViaProcedure()
        {
            using var db = new Treinamento.Data.ApplicationContext();

            var dep = new SqlParameter("@dep", "Departamento");

            var departamentos = db.Departamentos
                //.FromSqlRaw("EXECUTE GetDepartamentos @dep", dep)
                .FromSqlInterpolated($"EXECUTE GetDepartamentos {dep}")
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine(departamento.Descricao);
            }
        }



        static void CriarStoredProcedureDeConsulta()
        {
            var criarDepartamento = @"
            CREATE OR ALTER PROCEDURE GetDepartamentos
                @Descricao VARCHAR(50)
            AS
            BEGIN
                SELECT * FROM Departamentos Where Descricao Like @Descricao + '%'
            END        
            ";

            using var db = new Treinamento.Data.ApplicationContext();

            db.Database.ExecuteSqlRaw(criarDepartamento);
        }



        //comando para inseri dados usando procedure
        static void InserirDadosViaProcedure()
        {
            using var db = new Treinamento.Data.ApplicationContext();

            db.Database.ExecuteSqlRaw("execute CriarDepartamento @p0, @p1", "Departamento Via Procedure", true);
        }


        //comando para criar uma procedure no banco
        static void CriarStoredProcedure()
        {
            var criarDepartamento = @"
            CREATE OR ALTER PROCEDURE CriarDepartamento
                @Descricao VARCHAR(50),
                @Ativo bit
            AS
            BEGIN
                INSERT INTO 
                    Departamentos(Descricao, Ativo, Excluido) 
                VALUES (@Descricao, @Ativo, 0)
            END        
            ";

            using var db = new Treinamento.Data.ApplicationContext();

            db.Database.ExecuteSqlRaw(criarDepartamento);
        }


        //AsSplitQuery este comando divide a consulta sql
        //AsSingleQuery este comando ignora o comando de divisão se a mesma estiver configurado como global
        static void DivisaoDeConsulta()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            var departamentos = db.Departamentos
                .Include(p => p.Funcionarios)
                .Where(p => p.Id < 3)
                //.AsSplitQuery()
                .AsSingleQuery()
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
                foreach (var funcionario in departamento.Funcionarios)
                {
                    Console.WriteLine($"\tNome: {funcionario.Nome}");
                }
            }
        }



        static void EntendendoConsulta1NN1()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            //exibindo os funcionarios e os departamentos
            var funcionarios = db.Funcionarios
                .Include(p => p.Departamento)
                .ToList();


            foreach (var funcionario in funcionarios)
            {
                Console.WriteLine($"Nome: {funcionario.Nome} / Descricap Dep: {funcionario.Departamento.Descricao}");
            }

            //exibindo os departamentos e funcionarios
            var departamentos = db.Departamentos
                .Include(p => p.Funcionarios)
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
                foreach (var funcionario in departamento.Funcionarios)
                {
                    Console.WriteLine($"\tNome: {funcionario.Nome}");
                }
            }
        }



        //o recuso TAG em suas consultas para auditar comandos
        static void ConsultaComTAG()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            var departamentos = db.Departamentos
                .TagWith(@"Estou enviando um comentario para o servidor
                Segundo comentario
                Terceiro comentario")
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
            }
        }



        //este tipo de consulta evitar ataques de injeção sql
        static void ConsultaInterpolada()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            var id = 1;
            var departamentos = db.Departamentos
                .FromSqlInterpolated($"SELECT * FROM Departamentos WHERE Id>{id}")
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
            }
        }


        //tipos de consulta quando não consegue utilizar o link
        static void ConsultaParametrizada()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            var id = new SqlParameter
            {
                Value = 1,
                SqlDbType = System.Data.SqlDbType.Int
            };
            var departamentos = db.Departamentos
                 .FromSqlRaw("SELECT * FROM Departamentos WHERE Id>{0}", id)
                 .Where(p => !p.Excluido)
                 .ToList();


            var departamentos2 = db.Departamentos
                     .FromSqlRaw("SELECT * FROM Departamentos WHERE Id>{0}", id)
                     .Include(p => p.Funcionarios)
                     .Where(p => !p.Excluido)
                     .ToList();


            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");
            }
        }




        //nesta consulta e exibido a descrição do departamento e nome do funcionário
        static void ConsultaProjetada()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            var departamentos = db.Departamentos
                .Where(p => p.Id > 0)
                .Select(p => new { p.Descricao, Funcionarios = p.Funcionarios.Select(f => f.Nome) })
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao}");

                foreach (var funcionario in departamento.Funcionarios)
                {
                    Console.WriteLine($"\t Nome: {funcionario}");
                }
            }
        }




        //caso seja necessário desabilitar o filtro global
        //para uma consulta trazer todos os registros
        static void IgnoreFiltroGlobal()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            var departamentos = db.Departamentos.IgnoreQueryFilters().Where(p => p.Id > 0).ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao} \t Excluido: {departamento.Excluido}");
            }
        }


        //configurando um filtro de forma global no ApplicationContext não visualizar registros excluidos
        static void FiltroGlobal()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            var departamentos = db.Departamentos.Where(p => p.Id > 0).ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine($"Descrição: {departamento.Descricao} \t Excluido: {departamento.Excluido}");
            }
        }




        //os dados relacionado são carregados sob demanda do banco de dados quando
        //a propriedade de navegação é acessada
        //caregamento usando .UseLazyLoadingProxies()
        static void CarregamentoLento()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            //desabilitar o UseLazyLoadingProxies
            //db.ChangeTracker.LazyLoadingEnabled = false;

            var departamentos = db
                .Departamentos
                .ToList();

            foreach (var departamento in departamentos)
            {
                Console.WriteLine("---------------------------------------");
                Console.WriteLine($"Departamento: {departamento.Descricao}");

                if (departamento.Funcionarios?.Any() ?? false)
                {
                    foreach (var funcionario in departamento.Funcionarios)
                    {
                        Console.WriteLine($"\tFuncionario: {funcionario.Nome}");
                    }
                }
                else
                {
                    Console.WriteLine($"\tNenhum funcionario encontrado!");
                }
            }
        }

    

        //fazer o carregamento de dados de forma explicita
        static void CarregamentoExplicito()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            var departamentos = db
                .Departamentos
                .ToList();

            foreach (var departamento in departamentos)
            {
                if (departamento.Id == 2)
                {
                    //db.Entry(departamento).Collection(p=>p.Funcionarios).Load();
                    db.Entry(departamento).Collection(p => p.Funcionarios).Query().Where(p => p.Id > 2).ToList();
                }

                Console.WriteLine("---------------------------------------");
                Console.WriteLine($"Departamento: {departamento.Descricao}");

                if (departamento.Funcionarios?.Any() ?? false)
                {
                    foreach (var funcionario in departamento.Funcionarios)
                    {
                        Console.WriteLine($"\tFuncionario: {funcionario.Nome}");
                    }
                }
                else
                {
                    Console.WriteLine($"\tNenhum funcionario encontrado!");
                }
            }
        }



        //fazer o carregamento de dados de forma adiantada
        //desvantagem quando a tabela possui muitos campos
        static void CarregamentoAdiantado()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            SetupTiposCarregamentos(db);

            var departamentos = db
                .Departamentos
                .Include(p => p.Funcionarios);

            foreach (var departamento in departamentos)
            {

                Console.WriteLine("---------------------------------------");
                Console.WriteLine($"Departamento: {departamento.Descricao}");

                if (departamento.Funcionarios?.Any() ?? false)
                {
                    foreach (var funcionario in departamento.Funcionarios)
                    {
                        Console.WriteLine($"\tFuncionario: {funcionario.Nome}");
                    }
                }
                else
                {
                    Console.WriteLine($"\tNenhum funcionario encontrado!");
                }
            }
        }


        static void SetupTiposCarregamentos(Treinamento.Data.ApplicationContext db)
        {
            if (!db.Departamentos.Any())
            {
                db.Departamentos.AddRange(
                    new Treinamento.Domain.Departamento
                    {
                        Descricao = "Departamento 01",
                        Funcionarios = new System.Collections.Generic.List<Treinamento.Domain.Funcionario>
                        {
                            new Treinamento.Domain.Funcionario
                            {
                                Nome = "Rafael Almeida",
                                CPF = "99999999911",
                                RG= "2100062"
                            }
                        },
                        Excluido = true
                    },
                    new Treinamento.Domain.Departamento
                    {
                        Descricao = "Departamento 02",
                        Funcionarios = new System.Collections.Generic.List<Treinamento.Domain.Funcionario>
                        {
                            new Treinamento.Domain.Funcionario
                            {
                                Nome = "Bruno Brito",
                                CPF = "88888888811",
                                RG= "3100062"
                            },
                            new Treinamento.Domain.Funcionario
                            {
                                Nome = "Eduardo Pires",
                                CPF = "77777777711",
                                RG= "1100062"
                            }
                        }
                    });

                db.SaveChanges();
                db.ChangeTracker.Clear();
            }
        }



        //gerando o script completo do banco de dados 
        static void ScriptGeralDoBancoDeDados()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            var script = db.Database.GenerateCreateScript();

            Console.WriteLine(script);
        }


        //fazer a recuperação das migrações já aplicadas
        static void MigracoesJaAplicadas()
        {
            using var db = new Treinamento.Data.ApplicationContext();

            var migracoes = db.Database.GetAppliedMigrations();

            Console.WriteLine($"Total: {migracoes.Count()}");

            foreach (var migracao in migracoes)
            {
                Console.WriteLine($"Migração: {migracao}");
            }
        }


        //visualizar a quantidade e descrição das migrações já existente
        static void TodasMigracoes()
        {
            using var db = new Treinamento.Data.ApplicationContext();

            var migracoes = db.Database.GetMigrations();

            Console.WriteLine($"Total: {migracoes.Count()}");

            foreach (var migracao in migracoes)
            {
                Console.WriteLine($"Migração: {migracao}");
            }
        }


        //fazer a criãçao da migração em tempo de execução
        static void AplicarMigracaoEmTempodeExecucao()
        {
            using var db = new Treinamento.Data.ApplicationContext();

            db.Database.Migrate();
        }


        //visualizar se existem migrações que ainda não foram aplicadas ao banco
        static void MigracoesPendentes()
        {
            using var db = new Treinamento.Data.ApplicationContext();

            var migracoesPendentes = db.Database.GetPendingMigrations();

            Console.WriteLine($"Total: {migracoesPendentes.Count()}");

            foreach (var migracao in migracoesPendentes)
            {
                Console.WriteLine($"Migração: {migracao}");
            }
        }




        //como evitar ataque de sql injection
        static void SqlInjection()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departamentos.AddRange(
                new Treinamento.Domain.Departamento
                {
                    Descricao = "Departamento 01"
                },
                new Treinamento.Domain.Departamento
                {
                    Descricao = "Departamento 02"
                });
            db.SaveChanges();

            //var descricao = "Teste ' or 1='1";
            //db.Database.ExecuteSqlRaw("update departamentos set descricao='AtaqueSqlInjection' where descricao={0}",descricao);
            //db.Database.ExecuteSqlRaw($"update departamentos set descricao='AtaqueSqlInjection' where descricao='{descricao}'");
            foreach (var departamento in db.Departamentos.AsNoTracking())
            {
                Console.WriteLine($"Id: {departamento.Id}, Descricao: {departamento.Descricao}");
            }
        }



        //metodo para criar comandos sql
        static void ExecuteSQL()
        {
            using var db = new Treinamento.Data.ApplicationContext();

            // Primeira Opcao
            using (var cmd = db.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteNonQuery();
            }

            // Segunda Opcao
            var descricao = "TESTE";
            db.Database.ExecuteSqlRaw("update departamentos set descricao={0} where id=1", descricao);

            //Terceira Opcao
            db.Database.ExecuteSqlInterpolated($"update departamentos set descricao={descricao} where id=1");
        }




        //gerenciar o estado da conexão 
        static int _count;
        static void GerenciarEstadoDaConexao(bool gerenciarEstadoConexao)
        {
            using var db = new Treinamento.Data.ApplicationContext();
            var time = System.Diagnostics.Stopwatch.StartNew();

            var conexao = db.Database.GetDbConnection();

            conexao.StateChange += (_, __) => ++_count;

            if (gerenciarEstadoConexao)
            {
                conexao.Open();
            }

            for (var i = 0; i < 200; i++)
            {
                db.Departamentos.AsNoTracking().Any();
            }

            time.Stop();
            var mensagem = $"Tempo: {time.Elapsed.ToString()}, {gerenciarEstadoConexao}, Contador: {_count}";

            Console.WriteLine(mensagem);
        }



        //validando a conexão com o banco de dados
        static void HealthCheckBancoDeDados()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            var canConnect = db.Database.CanConnect();

            if (canConnect)
            {

                Console.WriteLine("Posso me conectar");
            }
            else
            {
                Console.WriteLine("Não posso me conectar");
            }
        }



        //comando para fazer a criação do banco de dados e deleção juntamente com todas as tabelas
        static void EnsureCreatedAndDeleted()
        {
            using var db = new Treinamento.Data.ApplicationContext();
            db.Database.EnsureCreated();
           // db.Database.EnsureDeleted();
        }

        //comando para fazer a criação do banco de dados usando dois contexto
        static void GapDoEnsureCreated()
        {
            using var db1 = new Treinamento.Data.ApplicationContext();
            //using var db2 = new Treinamento.Data.ApplicationContextCidade();

            db1.Database.EnsureCreated();
            //db2.Database.EnsureCreated();

            //var databaseCreator = db2.GetService<IRelationalDatabaseCreator>();
            //databaseCreator.CreateTables();
        }

    }
}
