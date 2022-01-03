using Banana.Core.Interfaces;
using Banana.Core.SetC;
using Banana.Core.SetQ;
using Banana.Entites;

namespace Banana
{
    public class DataBaseContext<T> : AbstractDataBaseContext
    {
        public QuerySet<T> QuerySet => (QuerySet<T>)Set;

        public CommandSet<T> CommandSet => (CommandSet<T>)Set;
    }

    public abstract class AbstractDataBaseContext
    {
        public AbstractDapperSet Set { get; set; }

        public EOperateType OperateType { get; set; }
    }
}
