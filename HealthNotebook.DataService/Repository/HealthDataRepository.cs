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
  public class HealthDataRepository : GenericRepository<HealthData>, IHealthDataRepository
  {
    public HealthDataRepository(
        AppDbContext context,
        ILogger logger) : base(context, logger)
    { }

    public override async Task<IEnumerable<HealthData>> All()
    {
      try
      {
        return await dbSet.Where(u => u.Status == 1)
        .AsNoTracking()
        .ToListAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "{Repo} All method has generated and error", typeof(HealthDataRepository));
        return new List<HealthData>();
      }
    }
    public async Task<bool> UpdateHealthData(HealthData healthData)
    {
      try
      {
        var existingHealthData = await dbSet
        .FirstOrDefaultAsync(u => u.Id == healthData.Id);

        if (existingHealthData == null)
          return false;

        existingHealthData.BloodType = healthData.BloodType;
        existingHealthData.Height = healthData.Height;
        existingHealthData.Race = healthData.Race;
        existingHealthData.Weight = healthData.Weight;
        existingHealthData.UseGlasses = healthData.UseGlasses;
        existingHealthData.UpdateDate = DateTime.UtcNow;

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "{Repo} UpdateHealthData method has generated and error", typeof(HealthDataRepository));
        return false;
      }
    }
  }
}