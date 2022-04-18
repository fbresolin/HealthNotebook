using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthNotebook.Authentication.Models.Dto.Outgoing
{
  public class AuthResult
  {
    public string Token { get; set; }
    public bool Sucess { get; set; }
    public List<string> Errors { get; set; }
  }
}