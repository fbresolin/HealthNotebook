using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthNotebook.Configuration.Messages;
using HealthNotebook.DataService.IConfiguration;
using HealthNotebook.Entities.DbSet;
using HealthNotebook.Entities.Dtos.Errors;
using HealthNotebook.Entities.Dtos.Generic;
using HealthNotebook.Entities.Dtos.Incoming.Profile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthNotebook.Api.Controllers.v1;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProfileController : BaseController
{
  public ProfileController(
      IUnitOfWork unitOfWork,
      UserManager<IdentityUser> userManager) : base(unitOfWork, userManager)
  {
  }
  [HttpGet]
  public async Task<IActionResult> GetProfile()
  {
    var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);
    var result = new Result<User>();

    if (loggedInUser == null)
    {
      result.Error = PopulateError(400,
        ErrorMessages.Profile.UserNotFound,
        ErrorMessages.Generic.TypeBadRequest);
      return BadRequest(result);
    }

    var identityId = new Guid(loggedInUser.Id);

    var profile = await _unitOfWork.Users.GetByIdentityId(identityId);

    if (profile == null)
    {
      result.Error = PopulateError(400,
        ErrorMessages.Profile.UserNotFound,
        ErrorMessages.Generic.TypeBadRequest);
      return BadRequest(result);
    }

    return Ok(profile);
  }
  [HttpPut]
  public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto profileDto)
  {
    var result = new Result<User>();
    if (!ModelState.IsValid)
    {
      result.Error = PopulateError(400,
        ErrorMessages.Generic.InvalidPayload,
        ErrorMessages.Generic.TypeBadRequest);
      return BadRequest(result);
    }

    var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);

    if (loggedInUser == null)
    {
      result.Error = PopulateError(400,
        ErrorMessages.Profile.UserNotFound,
        ErrorMessages.Generic.TypeBadRequest);
      return BadRequest(result);
    }

    var identityId = new Guid(loggedInUser.Id);

    var profile = await _unitOfWork.Users.GetByIdentityId(identityId);

    if (profile == null)
    {
      result.Error = PopulateError(400,
        ErrorMessages.Profile.UserNotFound,
        ErrorMessages.Generic.TypeBadRequest);
      return BadRequest(result);
    }

    profile.Address = profileDto.Address;
    profile.Sex = profileDto.Sex;
    profile.MobileNumber = profileDto.MobileNumber;
    profile.Country = profileDto.Country;

    var isUpdated = await _unitOfWork.Users.UpdateUserProfile(profile);

    if (isUpdated)
    {
      await _unitOfWork.CompleteAsync();

      result.Content = profile;

      return Ok(result);
    }

    result.Error = PopulateError(500,
      ErrorMessages.Generic.SomethingWentWrong,
     ErrorMessages.Generic.UnableToProcess);
    return BadRequest(result);
  }
}
