using hushazvillany_backend.Data;
using hushazvillany_backend.Models;
using hushazvillany_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hushazvillany_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : Controller
    {

        private readonly AppDbContext _appDbContext;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IEmailService _emailService;
        public MessageController(AppDbContext appDbContext, IHostEnvironment hostEnviroment, IEmailService emailService)
        {
            _appDbContext = appDbContext;
            _hostEnvironment = hostEnviroment;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> PostNewMessage(Messages message)
        {          
            _appDbContext.Messages.Add(message);
            await _appDbContext.SaveChangesAsync();

            _emailService.SendEmail(message.Email, "Automatikus válasz", "Ez egy automatikusan generált üzenet! Az átlag válaszidő 24 óra ehhez kérjük türelmedet, üzenetedet megkaptuk és hamarosan válaszolunk..", "Ez egy automatikusan generált üzenet! Az átlag válaszidő 24 óra ehhez kérjük türelmedet, üzenetedet megkaptuk és hamarosan válaszolunk..");

            return Ok(new { success = true, data = new { message = "Sikeresen írtál egy üzenetet az oldalnak." } });
        }

        [Authorize]
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> ReplyMessage(int id, Messages message)
        {
            try
            {
                var messageSource = _appDbContext.Messages.FirstOrDefault(x => x.Id == id);
                if (messageSource == null)
                {
                    return BadRequest("Nincs ilyen üzenet!");
                }

                messageSource.isReplied = true;
                messageSource.ReplyContent = message.ReplyContent;

                await _appDbContext.SaveChangesAsync();
                _emailService.SendEmail(message.Email, "Válasz a húsházvillánytól", message.ReplyContent, message.ReplyContent);


                return Ok(new { success = true, data = new { message = "Sikeresen válaszoltál egy üzenetre!" } });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }

        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllMessages()
        {
            return Ok(_appDbContext.Messages);
        }
    }
}
