using HotelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelListing.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private static List<Hotel> hotels = new List<Hotel>
        {
            new Hotel { Id=1,Name="Taj", Address="Delhi",Rating=5},
            new Hotel { Id=2,Name="Lemontree", Address="Jaipur",Rating=4}

        };
        // GET: api/<HotelsController>
        [HttpGet]
        public ActionResult<IEnumerable<Hotel>> Get()
        {
            return Ok(hotels);
        }

        // GET api/<HotelsController>/5
        [HttpGet("{id}")]
        public ActionResult<Hotel> Get(int id)
        {
            var hotel = hotels.FirstOrDefault(x => x.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }
            return Ok(hotel);
        }

        // POST api/<HotelsController>
        [HttpPost]
        public IActionResult Post([FromBody] Hotel newHotel)
        {
            if (hotels.Any(x => x.Id == newHotel.Id))
            {
                return BadRequest($"Hotel with {newHotel.Id} already exist");
            }
            hotels.Add(newHotel);

            return CreatedAtAction(nameof(Get), new { id = newHotel.Id }, newHotel);
        }

        // PUT api/<HotelsController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Hotel updatedHotel)
        {
            var existingHotel = hotels.FirstOrDefault(x => x.Id == id);
            if (existingHotel == null)
            {
                return NotFound();
            }
            existingHotel.Address = updatedHotel.Address;
            existingHotel.Name = updatedHotel.Name;
            existingHotel.Rating = updatedHotel.Rating;
            return NoContent();            
        }

        // DELETE api/<HotelsController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var hotel = hotels.FirstOrDefault(x => x.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            hotels.Remove(hotel);
            return NoContent();
        }
    }
}
