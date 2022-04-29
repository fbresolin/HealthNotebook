using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace HealthNotebook.Entities.DbSet
{
  public class RefreshToken : BaseEntity
  {
    public string UserId { get; set; } // user id when logged in
    public string Token { get; set; }
    public string JwtId { get; set; } // the id generated when a jwt id has been requested
    public bool IsUsed { get; set; } // to make sure that the token is only used once
    public bool IsRevoked { get; set; } // make sure it is valid
    public DateTime ExpiryDate { get; set; }
    [ForeignKey(nameof(UserId))]
    public IdentityUser User { get; set; }
  }
}