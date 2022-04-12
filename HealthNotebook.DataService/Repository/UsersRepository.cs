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
        ILogger logger) : base(context, logger) { }

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

  }
}