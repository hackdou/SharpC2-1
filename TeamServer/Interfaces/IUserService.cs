using System.Threading.Tasks;

namespace TeamServer.Interfaces
{
    public interface IUserService
    {
        void SetPassword(string password);
        Task<bool> IsValid(string password);
    }
}