using bayaaAPI.Data;
using bayaaAPI.DTOs;
using bayaaAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bayaaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        
        public AuthController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Check  if email or mobile is null or empty
            if(string.IsNullOrWhiteSpace(dto.Login))
            {
                return BadRequest( new { message = "Email or mobile number is required." });
            }

            // Check  if password is null or empty
            if(string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest( new { message = "Password is required." });
            }

            // Check if any account exist with this email or mobile  
            var login = dto.Login.Trim();

            var exists = await _context.Users
                .AnyAsync(u => u.Email == login || u.Mobile == login);

            if (exists)
            {
                return BadRequest( new { message = "An account with this email or mobile number already exists." });
            }

            // Create user object with Hash password
            var isEmail = login.Contains("@");

            var user = new User
            {
                Email = isEmail ? login : null,                
                Mobile = isEmail ? null : login,

                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            //Defines which table to add the user in the database
            _context.Users.Add(user);

            // Add user to the database
            await _context.SaveChangesAsync();

            return Ok( new { message = "User Registration successful." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if(string.IsNullOrWhiteSpace(dto.Login))
            {
                return BadRequest( new { message = "Email or mobile number is required." });
            }

            if(string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest( new { message = "Password is required." });
            }

            var login = dto.Login.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == login || u.Mobile == login);

            if(user == null)
            {
                return Unauthorized( new { message = "Invalid email/mobile or password." });
            }

            var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if(!passwordValid)
            {
                return Unauthorized( new { message = "Invalid email/mobile or password." });
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Mobile = user.Mobile,
                FullName = user.FullName,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                Division = user.Division,
                District = user.District,
                Area = user.Area,
                Address = user.Address
            };

            return Ok( new { message = "Login successful.", user = userDto });

        }

    }
}
