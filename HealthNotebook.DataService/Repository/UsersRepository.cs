using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthNotebook.Entities.DbSet;
using HealthNotebook.DataService.IRepository;
using HealthNotebook.DataService.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HealthNotebook.DataService.Repository
{
  public class UsersRepository : GenericRepository<User>, IUsersRepository
  {
    public UsersRepository(
        AppDbContext context,
        ILogger logger) : base(context, logger)
    { }

    public override async Task<IEnumerable<User>> All()
    {
      try
      {
        return await dbSet.Where(u => u.Status == 1)
        .AsNoTracking()
        .ToListAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "{Repo} All method has generated and error", typeof(UsersRepository));
        return new List<User>();
      }
    }

    public async Task<User?> GetByIdentityId(Guid identityId)
    {
      try
      {
        return await dbSet
        .Where(u => u.Status == 1 &&
        u.IdentityId == identityId)
        .FirstOrDefaultAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "{Repo} GetByIdentityId method has generated and error", typeof(UsersRepository));
        return null;
      }
    }

    public async Task<bool> UpdateUserProfile(User user)
    {
      try
      {
        var existingUser = await dbSet
        .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (existingUser == null)
          return false;

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.MobileNumber = user.MobileNumber;
        existingUser.Phone = user.Phone;
        existingUser.Sex = user.Sex;
        existingUser.Country = user.Country;
        existingUser.Address = user.Address;
        existingUser.UpdateDate = DateTime.UtcNow;

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "{Repo} UpdateUserProfile method has generated and error", typeof(UsersRepository));
        return false;
      }
    }
  }
}