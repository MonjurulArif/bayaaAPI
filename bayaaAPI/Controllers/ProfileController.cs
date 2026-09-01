using bayaaAPI.Data;
using bayaaAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bayaaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if(user == null)
            {
                return NotFound();
            }

            return Ok(new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email,
                Mobile = user.Mobile,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                Division = user.Division,
                District = user.District,
                Area = user.Area,
                Address = user.Address
            });
        }


        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if(user == null)
            {
                return NotFound();
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.Mobile = dto.Mobile;
            user.Gender = dto.Gender;
            user.BirthDate = dto.BirthDate;
            user.Division = dto.Division;
            user.District = dto.District;
            user.Area = dto.Area;
            user.Address = dto.Address;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully",
            });

        }
    }
}
