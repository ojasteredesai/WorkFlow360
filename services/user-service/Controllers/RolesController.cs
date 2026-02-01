using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;

namespace UserService.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly UserDbContext _db;

    public RolesController(UserDbContext db)
    {
        _db = db;
    }

    // GET /api/roles
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        return Ok(await _db.Roles.ToListAsync());
    }
}