using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthNotebook.DataService.Data;
using HealthNotebook.Entities.DbSet;
using Microsoft.AspNetCore.Mvc;

namespace HealthNotebook.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UsersController : ControllerBase
  {
    private readonly AppDbContext _context;
    public UsersController(AppDbContext context)
    {
      _context = context;
    }
    // Get
    [HttpGet]
    [Route("GetUser")]
    public IActionResult GetUser(Guid Id)
    {
      var user = _context.Users
        .SingleOrDefault(u => u.Id == Id);

      if (user == null)
        return NotFound();

      return Ok(user);
    }
    // Post
    [HttpPost]
    public IActionResult AddUser(UserDto userDto)
    {
      var user = new User();
      user.FirstName = userDto.FirstName;
      user.LastName = userDto.LastName;
      user.Email = userDto.Email;
      user.Phone = userDto.Phone;
      user.Country = userDto.Country;
      user.DateOfBirth = Convert.ToDateTime(userDto.DateOfBirth);
      user.Status = 1;

      _context.Users.Add(user);
      _context.SaveChanges();

      return Ok(); // return a 201
    }
    // Get All
    [HttpGet]
    public IActionResult GetUsers()
    {
      var users = _context.Users
        .Where(u => u.Status == 1)
        .ToList();

      return Ok(users);
    }
  }
}