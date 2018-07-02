using Banana;
using System;

namespace ClassLibrary1
{
    [TableName("tb_user")]
    [PrimaryKey("id")]
    public class Class1
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("user_age")]
        public string UserAge { get; set; }
    }
}
