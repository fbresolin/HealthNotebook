using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthNotebook.DataService.Data;
using HealthNotebook.DataService.IConfiguration;
using HealthNotebook.Entities.DbSet;
using Microsoft.AspNetCore.Mvc;

namespace HealthNotebook.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UsersController : ControllerBase
  {
    private IUnitOfWork _unitOfWork;
    public UsersController(IUnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }
    // Get
    [HttpGet]
    [Route("GetUser", Name = "GetUser")]
    public async Task<IActionResult> GetUser(Guid Id)
    {
      var user = await _unitOfWork.Users.GetById(Id);

      if (user == null)
        return NotFound();

      return Ok(user);
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
      return Ok(users);
    }
  }
}