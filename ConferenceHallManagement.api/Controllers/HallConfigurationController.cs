using Microsoft.AspNetCore.Mvc;
using BLL_ConferenceHallManagement;

namespace ConferenceHallManagement.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HallConfigurationController : ControllerBase
    {
        private readonly IBLLConferenceHall _bllConferenceHall;

        public HallConfigurationController(IBLLConferenceHall bllConferenceHall)
        {
            _bllConferenceHall = bllConferenceHall;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var halls = await _bllConferenceHall.GetAllConferenceHalls();
                if (halls == null)
                {
                    return NotFound();
                }
                return Ok(halls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}