using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthNotebook.DataService.IConfiguration;
using HealthNotebook.DataService.IRepository;
using HealthNotebook.DataService.Repository;
using Microsoft.Extensions.Logging;

namespace HealthNotebook.DataService.Data
{
  public class UnitOfWork : IUnitOfWork, IDisposable
  {
    private readonly AppDbContext _context;
    private readonly ILogger _logger;
    public IUsersRepository Users { get; private set; }
    public IRefreshTokenRepository RefreshTokens { get; private set; }
    public IHealthDataRepository IHealthData { get; private set; }
    public UnitOfWork(
        AppDbContext context,
        ILoggerFactory loggerFactory)
    {
      _context = context;
      _logger = loggerFactory.CreateLogger("dblogs");

      Users = new UsersRepository(_context, _logger);
      RefreshTokens = new RefreshTokensRepository(_context, _logger);
      IHealthData = new HealthDataRepository(_context, _logger);
    }

    public async Task CompleteAsync()
    {
      await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
      _context.Dispose();
    }
  }
}