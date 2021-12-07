using System.Threading.Tasks;

using TeamServer.Handlers;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Interfaces
{
    public interface IPayloadService
    {
        Task<Payload> GeneratePayload(SharpC2Service.PayloadFormat format, Handler handler);
    }
}