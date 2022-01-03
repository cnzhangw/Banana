using Banana.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Tests
{
    /// <summary>
    /// 说明：User
    /// 作者：zhangw
    /// 时间：2022/1/3 22:22:24
    /// CLR版本：4.0.30319.42000
    /// 唯一标识：b87cd285-dc76-4f46-8493-3e4cb8111ee2
    /// </summary>

    [ColumnAttribute(Rename = "mem.user_wechat")]
    [Serializable]
    public class User : IBaseEntity<User, Guid>
    {
        //[Identity(false)]
        //[Display(Rename = "id")]
        //public string Id { get; set; }

        [PrimaryKey(false)]
        [Column(Rename = "id")]
        public override Guid Id { get; set; }


        [ColumnAttribute(Rename = "nickname")]
        public string Name { get; set; }

        [ColumnAttribute(Rename = "headimg")]
        public string HeadImg { get; set; }

        [ColumnAttribute(Rename = "miniappopenid")]
        public string OpenId { get; set; }

        [ColumnAttribute(Rename = "createon")]
        public DateTime CreateOn { get; set; }

        [ColumnAttribute(Rename = "issubscribe")]
        public int IsSubscribe { get; set; }
    }
}
