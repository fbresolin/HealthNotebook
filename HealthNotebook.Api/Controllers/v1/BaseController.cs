using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthNotebook.DataService.IConfiguration;
using Microsoft.AspNetCore.Mvc;

namespace HealthNotebook.Api.Controllers.v1
{
  [Route("api/v{version:apiVersion}/[controller]")]
  [ApiController]
  [ApiVersion("1.0")]
  public class BaseController : ControllerBase
  {
    protected IUnitOfWork _unitOfWork;
    public BaseController(IUnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }
  }
}