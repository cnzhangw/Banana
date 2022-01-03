using System;
using System.Collections.Generic;
using System.Text;

namespace Banana.Attributes
{
    public class PrimaryKeyAttribute : BaseAttrbute
    {
        /// <summary>
        /// 是否自增
        /// </summary>
        public bool AutoIncrement { get; set; }
        public PrimaryKeyAttribute(bool autoIncrement = false)
        {
            this.AutoIncrement = autoIncrement;
        }
    }
}
