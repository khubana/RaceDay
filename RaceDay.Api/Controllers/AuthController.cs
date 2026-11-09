using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(RaceDayContext db, IPasswordHasher<User> hasher, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var roleName = request.RoleName.Trim();
        if (!roleName.Equals("Organiser", StringComparison.OrdinalIgnoreCase) &&
            !roleName.Equals("Participant", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Choose Organiser or Participant." });
        roleName = roleName.Equals("Organiser", StringComparison.OrdinalIgnoreCase) ? "Organiser" : "Participant";
        var email = request.Email.Trim().ToLowerInvariant();
        try
        {
            if (await db.Users.AnyAsync(x => x.Email.ToLower() == email, ct))
                return Conflict(new { message = "This email is already registered. Please log in." });
            var role = await db.Roles.SingleOrDefaultAsync(x => x.RoleName == roleName, ct);
            if (role is null)
                return Problem(title: $"The {roleName} role does not exist in dbo.Roles.", detail: "Check the existing Roles table. No database tables or columns were added.", statusCode: 500);
            var user = new User { FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = email, Phone = request.Phone.Trim(), RoleID = role.RoleID };
            user.PasswordHash = hasher.HashPassword(user, request.Password);
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
            return Created("/api/users/me", new { user.UserID, user.FirstName, user.LastName, user.Email, user.Phone, Role = role.RoleName, user.CreatedAt });
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "SQL Server rejected registration for {Email}", email);
            return Problem(title: "Registration could not be saved.", detail: "Check the API console for the SQL exception. The API uses only the existing Users and Roles columns.", statusCode: 500);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Registration failed for {Email}", email);
            return Problem(title: "Registration failed.", detail: "Check the API console for the underlying error.", statusCode: 500);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await db.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.Email.ToLower() == email, ct);
            if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
                return Unauthorized(new { message = "Email or password is incorrect. If this account contains a placeholder hash, register a new account first." });
            var check = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (check == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Email or password is incorrect. Enter the original password used during registration, not the PasswordHash stored in SQL Server." });
            if (user.Role is null) return Problem(title: "This user has no matching role in dbo.Roles.", statusCode: 500);
            if (check == PasswordVerificationResult.SuccessRehashNeeded)
            { user.PasswordHash = hasher.HashPassword(user, request.Password); await db.SaveChangesAsync(ct); }
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("RoleName", user.Role.RoleName);
            HttpContext.Session.SetString("Email", user.Email);
            return Ok(new { message = "Login successful.", userID = user.UserID, email = user.Email, role = user.Role.RoleName });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed due to a database/server error.");
            return Problem(title: "Login could not be completed.", detail: "Check the API Output window for the SQL Server exception. Confirm the database connection and existing Users/Roles columns.", statusCode: 500);
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout() { HttpContext.Session.Clear(); return Ok(new { message = "Logged out successfully." }); }
}
