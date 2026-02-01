using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _db;

    public UsersController(UserDbContext db)
    {
        _db = db;
    }

    // GET /api/users


    // GET /api/users/{id}
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                IsActive = u.IsActive,
                Roles = u.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                IsActive = u.IsActive,
                Roles = u.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return user == null ? NotFound() : Ok(user);
    }

    // POST /api/users
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // POST /api/users/{id}/roles
    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AssignRole(Guid id, AssignRoleDto dto)
    {
        var exists = await _db.UserRoles
            .AnyAsync(ur => ur.UserId == id && ur.RoleId == dto.RoleId);

        if (exists)
            return BadRequest("Role already assigned.");

        _db.UserRoles.Add(new UserRole
        {
            UserId = id,
            RoleId = dto.RoleId
        });

        await _db.SaveChangesAsync();
        return Ok();
    }
}