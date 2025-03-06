using Microsoft.AspNetCore.Mvc;

namespace CommandService.Controkkers
{
    [Route("api/c/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        public PlatformsController()
        {

        }
        [HttpPost]
        public ActionResult TestInhbound()
        {
            Console.WriteLine("--> Yes connection");
            return Ok("PlatformsController");
        }
    }
}