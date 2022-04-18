using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HealthNotebook.Authentication.Configuration;
using HealthNotebook.Authentication.Models.Dto.Incoming;
using HealthNotebook.Authentication.Models.Dto.Outgoing;
using HealthNotebook.DataService.IConfiguration;
using HealthNotebook.Entities.DbSet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthNotebook.Api.Controllers.v1
{
  public class AccountsController : BaseController
  {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtConfig _jwtConfig;
    public AccountsController(
        IUnitOfWork unitOfWork,
        UserManager<IdentityUser> userManager,
        IOptionsMonitor<JwtConfig> optionsMonitor) : base(unitOfWork)
    {
      _userManager = userManager;
      _jwtConfig = optionsMonitor.CurrentValue;
    }

    // Register Action
    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto registrationDto)
    {
      // check if the model is valid
      if (!ModelState.IsValid)
        return BadRequest(new UserRegistrationResponseDto
        {
          Sucess = false,
          Errors = new List<string>(){
                "Invalid Payload"
            }
        });

      // check if email already exists
      var userExist = await _userManager.FindByEmailAsync(registrationDto.Email);
      if (userExist != null)
        return BadRequest(new UserRegistrationResponseDto
        {
          Sucess = false,
          Errors = new List<string>(){
                "Email already used"
                }
        });

      // Add User
      var newUser = new IdentityUser()
      {
        Email = registrationDto.Email,
        UserName = registrationDto.Email,
        EmailConfirmed = true, // ToDo build email confirmation funcionality
      };

      // Adding User to the Table
      var isCreated = await _userManager.CreateAsync(newUser, registrationDto.Password);
      if (!isCreated.Succeeded) // if the registration fails
        return BadRequest(new UserRegistrationResponseDto
        {
          Sucess = false,
          Errors = isCreated.Errors.Select(x => x.Description).ToList()
        });

      //Adding user to the database
      var user = new User();
      user.IdentityId = new Guid(newUser.Id);
      user.FirstName = registrationDto.FirstName;
      user.LastName = registrationDto.LastName;
      user.Email = registrationDto.Email;
      user.Phone = ""; //registrationDto.Phone;
      user.Country = ""; //registrationDto.Country;
      user.DateOfBirth = DateTime.UtcNow; //Convert.ToDateTime(registrationDto.DateOfBirth);
      user.Status = 1;

      await _unitOfWork.Users.Add(user);
      await _unitOfWork.CompleteAsync();

      // Create Jwt token
      var token = GenerateJwtToken(newUser);

      // return back to the user
      return Ok(new UserRegistrationResponseDto
      {
        Sucess = true,
        Token = token
      });
    }
    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginDto)
    {
      if (!ModelState.IsValid)
        return BadRequest(new UserLoginResponseDto
        {
          Sucess = false,
          Errors = new List<string>(){
                "Invalid Payload"
            }
        });

      var userExist = await _userManager.FindByEmailAsync(loginDto.Email);
      if (userExist == null)
        return BadRequest(new UserLoginResponseDto
        {
          Sucess = false,
          Errors = new List<string>(){
                "Invalid authentication request"
                }
        });

      var isCorrect = await _userManager.CheckPasswordAsync(userExist, loginDto.Password);

      if (isCorrect)
      {
        var jwtToken = GenerateJwtToken(userExist);
        return Ok(new UserLoginResponseDto
        {
          Sucess = true,
          Token = jwtToken
        });
      }
      else
      {
        return BadRequest(new UserLoginResponseDto
        {
          Sucess = false,
          Errors = new List<string>(){
                "Invalid authentication request"
                }
        });
      }
    }
    private string GenerateJwtToken(IdentityUser user)
    {
      // handler is responsible to create the token
      var jwtHandler = new JwtSecurityTokenHandler();

      // get the security key
      var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new[]{
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email), //unique Id
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //used by the refresh token
            }),
        Expires = DateTime.UtcNow.AddHours(3), // todo update to minutes
        SigningCredentials = new SigningCredentials(
              new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature //todo review security algorithm
          )
      };

      // generate the security token
      var token = jwtHandler.CreateToken(tokenDescriptor);

      // convert security obj token into a string
      var jwtToken = jwtHandler.WriteToken(token);

      return jwtToken;
    }
  }
}