using Banana.Core;
using System.Linq;

namespace ClassLibrary1
{
    class Class1BLL : BaseService<Class1>
    {
        public void Test()
        {
            var list = Service.Query().ToList();
        }
    }
}
