using System;
using System.Collections.Generic;
using System.Text;

namespace Banana.Attributes
{
    public class Identity : BaseAttrbute
    {
        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsIncrease { get; set; }
        public Identity(bool IsIncrease = true)
        {
            this.IsIncrease = IsIncrease;
        }
    }
}
