using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HealthNotebook.Authentication.Configuration;
using HealthNotebook.Authentication.Models.Dto.Generic;
using HealthNotebook.Authentication.Models.Dto.Incoming;
using HealthNotebook.Authentication.Models.Dto.Outgoing;
using HealthNotebook.DataService.IConfiguration;
using HealthNotebook.Entities.DbSet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthNotebook.Api.Controllers.v1;
public class AccountsController : BaseController
{
  // class provided by AspNetCore Identity Framework
  private readonly TokenValidationParameters _tokenValidationParameters;
  private readonly JwtConfig _jwtConfig;
  public AccountsController(
      IUnitOfWork unitOfWork,
      UserManager<IdentityUser> userManager,
      TokenValidationParameters tokenValidationParameters,
      IOptionsMonitor<JwtConfig> optionsMonitor) : base(unitOfWork, userManager)
  {
    _tokenValidationParameters = tokenValidationParameters;
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
        Success = false,
        Errors = new List<string>(){
                "Invalid Payload"
            }
      });

    // check if email already exists
    var userExist = await _userManager.FindByEmailAsync(registrationDto.Email);
    if (userExist != null)
      return BadRequest(new UserRegistrationResponseDto
      {
        Success = false,
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
        Success = false,
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
    var token = await GenerateJwtToken(newUser);

    // return back to the user
    return Ok(new UserRegistrationResponseDto()
    {
      Success = true,
      Token = token.JwtToken,
      RefreshToken = token.RefreshToken
    });
  }
  [HttpPost]
  [Route("Login")]
  public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginDto)
  {
    if (!ModelState.IsValid)
      return BadRequest(new UserLoginResponseDto
      {
        Success = false,
        Errors = new List<string>(){
                "Invalid Payload"
            }
      });

    var userExist = await _userManager.FindByEmailAsync(loginDto.Email);
    if (userExist == null)
      return BadRequest(new UserLoginResponseDto
      {
        Success = false,
        Errors = new List<string>(){
                "Invalid authentication request"
                }
      });

    var isCorrect = await _userManager.CheckPasswordAsync(userExist, loginDto.Password);

    if (isCorrect)
    {
      var jwtToken = await GenerateJwtToken(userExist);
      return Ok(new UserLoginResponseDto()
      {
        Success = true,
        Token = jwtToken.JwtToken,
        RefreshToken = jwtToken.RefreshToken
      });
    }
    else
    {
      return BadRequest(new UserLoginResponseDto
      {
        Success = false,
        Errors = new List<string>(){
                "Invalid authentication request"
                }
      });
    }
  }
  [HttpPost]
  [Route("RefreshToken")]
  public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
  {
    if (!ModelState.IsValid)
      return BadRequest(new UserLoginResponseDto
      {
        Success = false,
        Errors = new List<string>(){
                "Invalid authentication request"
                }
      });

    var result = await VerifyToken(tokenRequestDto);

    if (result == null)
      return BadRequest(new UserLoginResponseDto
      {
        Success = false,
        Errors = new List<string>(){
                "Token validation failed"
                }
      });

    return Ok(result);
  }
  private async Task<AuthResult> VerifyToken(TokenRequestDto tokenRequestDto)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    try
    {
      // check validity of the token
      var principal = tokenHandler.ValidateToken(tokenRequestDto.Token, _tokenValidationParameters, out var validateToken);

      // validate results that had been generated
      // check if the string is an actual Jwt token
      if (validateToken is JwtSecurityToken jwtSecurityToken)
      {
        // check if Jwt token is created using the same creation algorithm
        var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

        if (!result)
          return null;
      }

      // check expiry date
      var utcExpiryDate = long
      .Parse(principal.Claims.FirstOrDefault(d => d.Type == JwtRegisteredClaimNames.Exp).Value);

      // convert to date to check
      var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

      if (expiryDate > DateTime.UtcNow)
      {
        return new AuthResult()
        {
          Success = false,
          Errors = new List<string>(){
              "Jwt token has not expired"
            }
        };
      }

      // check if the refresh token exist
      var refreshToken = await _unitOfWork.RefreshTokens
      .GetByRefreshToken(tokenRequestDto.RefreshToken);

      if (refreshToken == null)
        return new AuthResult()
        {
          Success = false,
          Errors = new List<string>(){
              "Invalid refresh token"
            }
        };

      if (refreshToken.ExpiryDate < DateTime.UtcNow)
        return new AuthResult()
        {
          Success = false,
          Errors = new List<string>(){
              "Refresh token has expired, please login again"
            }
        };

      if (refreshToken.IsUsed)
        return new AuthResult()
        {
          Success = false,
          Errors = new List<string>(){
              "Refresh token has been used, it cannot be reused"
            }
        };

      if (refreshToken.IsRevoked)
        return new AuthResult()
        {
          Success = false,
          Errors = new List<string>(){
              "Refresh token has been revoked, it cannot be used"
            }
        };

      var jti = principal.Claims
      .SingleOrDefault(p => p.Type == JwtRegisteredClaimNames.Jti)
      .Value;

      if (refreshToken.JwtId != jti)
        return new AuthResult()
        {
          Success = false,
          Errors = new List<string>(){
              "Refresh token reference does not match the jwt token"
            }
        };

      // start processing a new token
      refreshToken.IsUsed = true;

      var IsTokenMarkedAsUsed = await _unitOfWork.RefreshTokens
      .MarkRefreshTokenAsUsed(refreshToken);

      if (IsTokenMarkedAsUsed)
      {
        await _unitOfWork.CompleteAsync();

        var dbUser = await _userManager.FindByIdAsync(refreshToken.UserId);

        if (dbUser == null)
          return new AuthResult()
          {
            Success = false,
            Errors = new List<string>(){
              "Error processing request"
            }
          };

        var generatedTokens = await GenerateJwtToken(dbUser);

        return new AuthResult
        {
          Token = generatedTokens.JwtToken,
          Success = true,
          RefreshToken = generatedTokens.RefreshToken
        };

      }

      return new AuthResult()
      {
        Success = false,
        Errors = new List<string>(){
              "Error processing request"
            }
      };

    }
    catch (Exception ex)
    {
      // TODO: Add better error handling
      // TODO: Add a logger
      return null;
    };
  }
  private async Task<TokenData> GenerateJwtToken(IdentityUser user)
  {
    // handler is responsible to create the token
    var jwtHandler = new JwtSecurityTokenHandler();

    // get the security key
    var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new[]{
                    new Claim("Id", user.Id),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email), //unique Id
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //used by the refresh token
            }),
      Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTimeFrame), // todo update to minutes
      SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature //todo review security algorithm
        )
    };

    // generate the security token
    var token = jwtHandler.CreateToken(tokenDescriptor);

    // convert security obj token into a string
    var jwtToken = jwtHandler.WriteToken(token);

    // generate a refresh token 
    var refreshToken = new RefreshToken
    {
      AddedDate = DateTime.UtcNow,
      Token = $"{RandomStringGenerator(25)}_{Guid.NewGuid()}",
      UserId = user.Id,
      IsRevoked = false,
      IsUsed = false,
      Status = 1,
      JwtId = token.Id,
      ExpiryDate = DateTime.UtcNow.AddMonths(6)
    };

    Console.WriteLine($"Id: {refreshToken.Id} and Token: {refreshToken.Token}");

    await _unitOfWork.RefreshTokens.Add(refreshToken);
    await _unitOfWork.CompleteAsync();

    var tokenData = new TokenData
    {
      JwtToken = jwtToken,
      RefreshToken = refreshToken.Token
    };

    return tokenData;
  }
  private string RandomStringGenerator(int length)
  {
    var random = new Random();
    const string chars = "ABCDEFGHIJKLMNOPQRTSUVWXYZ0123456789";

    return new string(Enumerable.Repeat(chars, length)
    .Select(s => s[random.Next(s.Length)]).ToArray());
  }
  private DateTime UnixTimeStampToDateTime(long unixDate)
  {
    var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    // Add number of seconds from 1 Jan 1970
    dateTime = dateTime.AddSeconds(unixDate).ToUniversalTime();
    return dateTime;
  }
}
