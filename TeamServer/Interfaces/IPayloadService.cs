using System.Threading.Tasks;

using TeamServer.Handlers;

namespace TeamServer.Interfaces
{
    public interface IPayloadService
    {
        Task<byte[]> GeneratePayload(Handler handler, string format);
    }
}