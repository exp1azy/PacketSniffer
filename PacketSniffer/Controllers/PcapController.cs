using Microsoft.AspNetCore.Mvc;
using WebSpectre.Shared.Capture;

namespace PacketSniffer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PcapController : ControllerBase
    {
        private readonly PcapAgent _capAgent;

        public PcapController(PcapAgent pcap)
        {
            _capAgent = pcap;
        }

        [HttpGet("status")]
        public IActionResult Status() => Ok(_capAgent.IsSnifferCapturing);

        [HttpGet("devices")]
        public ActionResult<List<PcapDevice>> AvailableDevices()
        {
            try
            {
                var devices = _capAgent.GetDevices();
                return Ok(devices);
            } 
            catch (ApplicationException)
            {
                return NoContent();
            }
        }

        [HttpGet("start")]
        public IActionResult Start([FromQuery] string a) 
        {
            try
            {
                _capAgent.Start(a);
            }
            catch (ApplicationException ex)
            {
                return Forbid(ex.Message);
            }

            return Ok();
        }

        [HttpGet("stop")]
        public IActionResult StopPhysic()
        {
            _capAgent.Stop();

            return Ok();
        }
    }
}
