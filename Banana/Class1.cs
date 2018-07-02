//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//#if NETCOREAPP2_0
//using Microsoft.Extensions.Configuration;
//#endif

//namespace Banana
//{
//    class Class1
//    {
//        public Task<string> GetName()
//        {
//#if NET40
//            var task=new Task<string>(()=>"jake");
//            task.Start();
//            return task;
//#elif NET45
//            return null;
//#else

//            return Task.FromResult<string>("zhangw");
//#endif
//        }


//        public void Start()
//        {
//            //MySql.Data.MySqlClient


//#if NETCOREAPP2_0
//            var builder = new ConfigurationBuilder();
//            builder.AddJsonFile("appsettings.json");
//            builder.AddInMemoryCollection(new[] {
//                new KeyValuePair<string,string>("zhang","wei")
//            });

//            var configuration = builder.Build();

//            string value = configuration["zhang"];
//#endif



//        }

//    }
//}
