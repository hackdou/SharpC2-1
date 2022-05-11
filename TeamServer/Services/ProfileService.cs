using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Storage;

using YamlDotNet.Serialization;

namespace TeamServer.Services;

public class ProfileService : IProfileService
{
    private readonly IDatabaseService _db;

    public ProfileService(IDatabaseService db)
    {
        _db = db;

        // load profiles from disk
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Profiles");
        var files = Directory.GetFiles(path, "*.yaml");
        
        if (files.Length == 0)
            return;
        
        var deserializer = new Deserializer();
        var conn = _db.GetConnection();
        
        foreach (var file in files)
        {
            var yaml = File.ReadAllText(file);
            
            try
            {
                var profile = deserializer.Deserialize<C2Profile>(yaml);
                var dao = new C2ProfileDao
                {
                    Name = profile.Name,
                    Yaml = yaml
                };
                
                conn.Insert(dao);
            }
            catch
            {
                // ignore
            }
        }
    }
    
    public async Task AddProfile(C2Profile profile)
    {
        var conn = _db.GetAsyncConnection();
        var serializer = new Serializer();
        var yaml = serializer.Serialize(profile);
        var dao = new C2ProfileDao
        {
            Name = profile.Name,
            Yaml = yaml
        };

        await conn.InsertAsync(dao);
    }

    public async Task<C2Profile> GetProfile(string name)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<C2ProfileDao>().FirstOrDefaultAsync(d => d.Name.Equals(name));

        if (dao is null)
            return null;
        
        var deserializer = new Deserializer();
        return deserializer.Deserialize<C2Profile>(dao.Yaml);
    }

    public async Task<IEnumerable<C2Profile>> GetProfiles()
    {
        var conn = _db.GetAsyncConnection();
        var daos = await conn.Table<C2ProfileDao>().ToArrayAsync();
        var deserializer = new Deserializer();

        return daos.Select(dao => deserializer.Deserialize<C2Profile>(dao.Yaml));
    }

    public async Task UpdateProfile(C2Profile profile)
    {
        var conn = _db.GetAsyncConnection();
        var serializer = new Serializer();
        var yaml = serializer.Serialize(profile);
        var dao = new C2ProfileDao
        {
            Name = profile.Name,
            Yaml = yaml
        };

        await conn.UpdateAsync(dao);
    }

    public async Task DeleteProfile(C2Profile profile)
    {
        var conn = _db.GetAsyncConnection();
        var serializer = new Serializer();
        var yaml = serializer.Serialize(profile);
        var dao = new C2ProfileDao
        {
            Name = profile.Name,
            Yaml = yaml
        };

        await conn.DeleteAsync(dao);
    }
}