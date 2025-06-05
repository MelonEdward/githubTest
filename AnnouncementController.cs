using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.client.DTO;
using SchoolSystem.Data;
using SchoolSystem.HubConnection;
using SchoolSystem.Services.AnnouncementService;
using SchoolSystem.shared.Models;

namespace SchoolSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementController : Controller
    {
        private readonly DataContext _context;
        private readonly IAnnouncementService _announcementService;
        private readonly IHubContext<PaymentHub> _hubContext;

        public AnnouncementController(DataContext context, IAnnouncementService announcementService, IHubContext<PaymentHub> hubContext)
        {
            _context = context;
            _announcementService = announcementService;
            _hubContext = hubContext;
        }

        [HttpGet("announcement")]
        public async Task<ActionResult<List<AnnouncementDTO>>> GetAllAnnouncementAsync()
        {
            return await _announcementService.GetAllAnnouncementAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AnnouncementDTO>> GetAnonuncementByIdAsync(int id)
        {
            var result = await _context.Announcements.FindAsync(id);
            if (result == null)
                return NotFound("Announcement Not Found");

            // Map Announcement to AnnouncementDTO
            var announcementDTO = new AnnouncementDTO
            {
                Id = result.Id.ToString(),
                Title = result.Title,
                Description = result.Description,
                Date = result.Date // Directly use DateTime
            };

            return Ok(announcementDTO);
        }

        [HttpGet("count")]
        public async Task<int> GetCount()
        {
            return await _context.Announcements.CountAsync();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncementAsync(int id)
        {
            var announce = await _context.Announcements.FindAsync(id);
            if (announce == null)
                return NotFound();

            _context.Announcements.Remove(announce);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("AnnouncementDeleted");
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAnnouncementAsync(int id, AnnouncementDTO dto)
        {
            var dbAnnounce = await _context.Announcements.FindAsync(id);
            if (dbAnnounce == null)
                return NotFound("Announcement Not Found");

            dbAnnounce.Title = dto.Title;
            dbAnnounce.Description = dto.Description;
            dbAnnounce.Date = dto.Date;

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("AnnouncementUpdated");
            return Ok(dbAnnounce);
        }

        [HttpPost("add")]
        public async Task<ActionResult<Announcement>> AddAnnouncement(AnnouncementDTO dto)
        {
            var newAnnouncement = new Announcement
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date
            };

            _context.Announcements.Add(newAnnouncement);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("AnnouncementAdded");
            return Ok(newAnnouncement);
        }
    }
}
