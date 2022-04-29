using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthNotebook.DataService.IRepository;

namespace HealthNotebook.DataService.IConfiguration
{
  public interface IUnitOfWork
  {
    IUsersRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task CompleteAsync();
  }
}