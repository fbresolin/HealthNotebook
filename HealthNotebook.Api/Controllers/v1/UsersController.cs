using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthNotebook.Configuration.Messages;
using HealthNotebook.DataService.Data;
using HealthNotebook.DataService.IConfiguration;
using HealthNotebook.Entities.DbSet;
using HealthNotebook.Entities.Dtos.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthNotebook.Api.Controllers.v1;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UsersController : BaseController
{
  public UsersController(
    IUnitOfWork unitOfWork,
    UserManager<IdentityUser> userManager) : base(unitOfWork, userManager)
  {
  }
  // Get
  [HttpGet]
  [Route("GetUser", Name = "GetUser")]
  public async Task<IActionResult> GetUser(Guid Id)
  {
    var user = await _unitOfWork.Users.GetById(Id);

    var result = new Result<User>();

    if (user == null)
    {
      result.Error = PopulateError(404,
        ErrorMessages.Users.UserNotFound,
        ErrorMessages.Generic.ObjectNotFound);
      return NotFound(result);
    }

    result.Content = user;
    return Ok(result);
  }
  // Post
  [HttpPost]
  public async Task<IActionResult> AddUser(UserDto userDto)
  {
    var user = new User();
    user.FirstName = userDto.FirstName;
    user.LastName = userDto.LastName;
    user.Email = userDto.Email;
    user.Phone = userDto.Phone;
    user.Country = userDto.Country;
    user.DateOfBirth = Convert.ToDateTime(userDto.DateOfBirth);
    user.Status = 1;

    await _unitOfWork.Users.Add(user);
    await _unitOfWork.CompleteAsync();

    return CreatedAtRoute("GetUser", new { Id = user.Id }, userDto); // return a 201
  }
  // Get All
  [HttpGet]
  public async Task<IActionResult> GetUsers()
  {
    var users = await _unitOfWork.Users.All();

    var result = new PagedResult<User>();
    result.Content = users.ToList();
    result.ResultCount = users.Count();
    return Ok(result);
  }
}