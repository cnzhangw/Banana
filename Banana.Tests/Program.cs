using Banana.Repository;
using MySql.Data.MySqlClient;
using Project.Dapper.Extension;
using System;
using System.Text.Json;

namespace Banana.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            var repo = new UserRepository();
            var user = repo.QuerySet<User>().Where(x => x.Id == new Guid("08d94093-2f5e-4c85-818b-828bc29310b0")).Get();
            Console.WriteLine(JsonSerializer.Serialize(user));
            Console.ReadLine();

            //var where= ExpressExpansion.

            var user2= repo.QuerySet<User>().Where(x => x.Id == new Guid("08d940fe-118e-4246-86e5-6b22a3c42c31")).Get();
            Console.WriteLine(JsonSerializer.Serialize(user));
            Console.ReadLine();
        }
    }
    class UserRepository : MyBaseRespository<User>
    {

    }

    class MyBaseRespository<T> : AbstractRepository<T>
    {

        public override void OnConfiguring(RepositoryOptionsBuilder builder)
        {
            builder.BuildConnection(x => new MySqlConnection(@"server=127.0.0.1;port=3306;uid=root;pwd=123456;database=cdb;charset=utf8"))
                  .BuildAutoSyncStructure(false)
                  .BuildProvider(new MySqlProvider());
        }


    }
}
