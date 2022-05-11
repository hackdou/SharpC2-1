using TeamServer.Models;

namespace TeamServer.Interfaces;

public interface IProfileService
{
    // create
    Task AddProfile(C2Profile profile);

    // read
    Task<C2Profile> GetProfile(string name);
    Task<IEnumerable<C2Profile>> GetProfiles();

    // update
    Task UpdateProfile(C2Profile profile);

    // delete
    Task DeleteProfile(C2Profile profile);
}