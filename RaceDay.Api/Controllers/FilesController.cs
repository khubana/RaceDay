using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController(RaceDayContext db, BlobStorageService blobs) : ControllerBase
{
    private int CurrentId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("event-banner")]
    [Authorize(Roles = "Organiser")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> EventBanner([FromForm] int eventId, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "Please select an image file." });
        var raceEvent = await db.Events.SingleOrDefaultAsync(x => x.EventID == eventId, ct);
        if (raceEvent is null) return NotFound(new { message = "Event not found." });
        if (raceEvent.OrganiserID != CurrentId) return Forbid();
        try { var url = await blobs.UploadAsync(file, "event-banners", ct); return Ok(new { url, persistedInDatabase = false, note = "Your existing Events table has no banner URL column, so the URL is returned but not saved in SQL Server." }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("profile-picture")]
    [Authorize(Roles = "Participant")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> ProfilePicture(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "Please select an image file." });
        if (!await db.Users.AnyAsync(x => x.UserID == CurrentId, ct)) return NotFound(new { message = "User not found." });
        try { var url = await blobs.UploadAsync(file, "profile-pictures", ct); return Ok(new { url, persistedInDatabase = false, note = "Your existing Users table has no profile picture URL column, so the URL is returned but not saved in SQL Server." }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
