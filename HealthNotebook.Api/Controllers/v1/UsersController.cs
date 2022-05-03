using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HealthNotebook.Configuration.Messages;
using HealthNotebook.DataService.Data;
using HealthNotebook.DataService.IConfiguration;
using HealthNotebook.Entities.DbSet;
using HealthNotebook.Entities.Dtos.Generic;
using HealthNotebook.Entities.Dtos.Outgoing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthNotebook.Api.Controllers.v1;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UsersController : BaseController
{
  public UsersController(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    UserManager<IdentityUser> userManager) : base(mapper, unitOfWork, userManager)
  {
  }
  // Get
  [HttpGet]
  [Route("GetUser", Name = "GetUser")]
  public async Task<IActionResult> GetUser(Guid Id)
  {
    var user = await _unitOfWork.Users.GetById(Id);

    var result = new Result<ProfileDto>();

    if (user == null)
    {
      result.Error = PopulateError(404,
        ErrorMessages.Users.UserNotFound,
        ErrorMessages.Generic.ObjectNotFound);
      return NotFound(result);
    }

    var mappedProfile = _mapper.Map<ProfileDto>(user);
    result.Content = mappedProfile;
    return Ok(result);
  }
  // Post
  [HttpPost]
  public async Task<IActionResult> AddUser(UserDto userDto)
  {
    var user = _mapper.Map<UserDto, User>(userDto);

    await _unitOfWork.Users.Add(user);
    await _unitOfWork.CompleteAsync();

    var result = new Result<UserDto>();
    result.Content = userDto;

    return CreatedAtRoute("GetUser", new { Id = user.Id }, result); // return a 201
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