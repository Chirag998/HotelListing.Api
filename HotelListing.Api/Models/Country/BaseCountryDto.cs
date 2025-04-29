using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Models.Country
{
    public class BaseCountryDto
    {

        [Required]
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}
