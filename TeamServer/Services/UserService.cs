using System.Threading.Tasks;

using TeamServer.Interfaces;

namespace TeamServer.Services
{
    public class UserService : IUserService
    {
        private string _password;

        public void SetPassword(string password)
            => _password = password;

        public Task<bool> IsValid(string password)
            => Task.FromResult(_password.Equals(password));
    }
}