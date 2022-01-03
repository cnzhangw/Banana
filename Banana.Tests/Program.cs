using Banana.Repository;
using MySql.Data.MySqlClient;
using Project.Dapper.Extension;
using System;

namespace Banana.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            var repo = new UserRepository();
            var user = repo.QuerySet<User>().Where(x => x.Id == new Guid("08d94093-2f5e-4c85-818b-828bc29310b0")).Get();

        }
    }
    class UserRepository : MyBaseRespository<User>
    {

    }

    class MyBaseRespository<T> : AbstractRepository<T>
    {

        public override void OnConfiguring(RepositoryOptionsBuilder builder)
        {
            builder.BuildConnection(x => new MySqlConnection(@"server=47.96.189.232;port=3306;uid=root;pwd=100200;database=cdb;charset=utf8"))
                  .BuildAutoSyncStructure(false)
                  .BuildProvider(new MySqlProvider());
        }


    }
}
