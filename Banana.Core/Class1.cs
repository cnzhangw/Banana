using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Banana.Core
{
    [TableName("Test")]
    [PrimaryKey("id")]
    public class Test : DataContextModel
    {
        [Column("id")]
        public int Id { get; set; }

        public string Name { get; set; }

        public int pRice { get; set; }

        public DateTime Addtime { get; set; }

        public string Addrees { get; set; }

        [Ignore]
        public string BuYao { get; set; }

        public string HelloCoulumn { get; set; }

    }

    [TableName("Account")]
    [PrimaryKey("ID")]
    public class Account : DataContextModel
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public string PhoneNumber { get; set; }
    }

    class Class1
    {
        static void a()
        {
            DateTime t = DateTime.Now.AddMonths(10);
            //Test account = DALManager<Test>.GetInstance().Single(e => e.ShopId == 88 || (e.Name != "张炜" || e.IsWechat == true));

            var instance = DALManager<Test>.GetInstance();
            //var result = instance.Transaction((e) =>
            //{
            //    var aaaa = e.Delete(f => f.Id == 230); //e.Single(f => f.id == 53);
            //    //string ad = aaaa.Name;
            //    //throw new Exception("hhhhhhhhhhhhh");
            //});
            //instance.a();

            //instance.ExecuteProcedure("qqqq", (e) =>
            //{
            //    //e.Add(new SqlParameter("ddd", 3));
            //});
            //instance.ExecuteProcedure()
            //var b = instance.Update(new Test() { Id = 41, Addrees = "火星" , pRice=189 }, e => new { e.Addrees, e.pRice });

            //var aafff = instance.GetDataTable("select * from Test");
            //var aa = instance.Query(e => e.Id == 41 || e.Name.Equals("超神") && (e.Id == 51 || string.IsNullOrWhiteSpace(e.Addrees)), e => e.Addtime);

            //var page = instance.QueryPage(e => e.Name == "超神", 1, 3);
            var aaab = instance.SkipTake(1, 2, e => e.Name == "超神", e => e.Id, true);

            int a = 1;

        }
    }

    class class2 : BaseBLL<Test>
    {
        void aa()
        {

            //var b = DateTime.MinValue;

            //var ss = Service.Single(e => e.Id == 52);

            //Service.ExecuteProcedure("qqq", e =>
            //{
            //    e.Add(new System.my);
            //});

            //var aa = GetInstance<Account>().Single(0);
            //var aa = Service.GetDataTable("SELECT a.`Name`,b.ShopName from Account as a INNER JOIN ShopInfo as b on a.TargetID=b.ID WHERE a.TargetType=2 AND a.`Name` LIKE '%@0%'", "大");

           // var a = Service.ExecuteProcedure("qqq", 336);

            //GetService<Account>().Insert(new Account() { });


            var b = Service.Insert(new Test()
            {
                Addrees = "阿斯蒂芬撒旦法",
                Addtime = DateTime.Now,
                HelloCoulumn = "aaaaa",
                Name = "吴菲",
                pRice = 998
            });


        }

    }

}
