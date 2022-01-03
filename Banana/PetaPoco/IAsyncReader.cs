using System;
using System.Threading.Tasks;

namespace Banana.PetaPoco
{
    public interface IAsyncReader<out T> : IDisposable
    {
        T Poco { get; }

        Task<bool> ReadAsync();
    }
}