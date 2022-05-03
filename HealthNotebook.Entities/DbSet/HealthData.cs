using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthNotebook.Entities.DbSet
{
  public class HealthData : BaseEntity
  {
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
    public string BloodType { get; set; } // make in a enum
    public string Race { get; set; }
    public bool UseGlasses { get; set; }
  }
}