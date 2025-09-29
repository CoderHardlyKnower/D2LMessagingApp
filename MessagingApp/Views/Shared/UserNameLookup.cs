using MessagingApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MessagingApp.Services
{
    public interface IUserNameLookup
    {
        Task<string> GetDisplayNameAsync(int userId, CancellationToken ct = default);
    }

    public class UserNameLookup : IUserNameLookup
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private static readonly MemoryCacheEntryOptions CacheOpts =
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

        public UserNameLookup(AppDbContext db, IMemoryCache cache)
        {
            _db = db; _cache = cache;
        }

        public async Task<string> GetDisplayNameAsync(int userId, CancellationToken ct = default)
        {
            var key = $"uname:{userId}";
            if (_cache.TryGetValue<string>(key, out var cached))
                return cached;

            var name = await _db.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.DisplayName)
                .FirstOrDefaultAsync(ct) ?? "User";

            _cache.Set(key, name, CacheOpts);
            return name;
        }
    }
}
