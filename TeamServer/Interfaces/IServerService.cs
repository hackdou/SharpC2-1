using System.Collections.Generic;
using System.Threading.Tasks;

using TeamServer.Models;
using TeamServer.Modules;

namespace TeamServer.Interfaces
{
    public interface IServerService
    {
        Module LoadModule(byte[] bytes);
        Module GetModule(string name);
        IEnumerable<Module> GetModules();
        Task HandleC2Message(C2Message message);
    }
}