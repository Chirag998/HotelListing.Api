using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Country;
using AutoMapper;
using HotelListing.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using HotelListing.Api.Models;
using HotelListing.Api.Exceptions;

namespace HotelListing.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesController> logger;

        public CountriesController(HotelListingDbContext context,IMapper mapper,ICountriesRepository countriesRepository,
            ILogger<CountriesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _countriesRepository = countriesRepository;
            this.logger = logger;
        }

        // GET: api/Countries
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            logger.LogInformation("Getting all countries");
            var countries = await _countriesRepository.GetAllAsync();
            var records = _mapper.Map<IEnumerable<GetCountryDto>>(countries);
            return Ok(records);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetPagedCountries([FromQuery] QueryParameters queryParameters)
        {
            var pagedCountriesResult = await _countriesRepository.GetAllAsync<GetCountryDto>(queryParameters);
            return Ok(pagedCountriesResult);
        }
        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            var country = await _countriesRepository.GetCountryWithHotels(id);

            if (country == null)
            {
                throw new NotFoundException(nameof(GetCountry),id);
            }
            var countryDto = _mapper.Map<CountryDto>(country);

            return Ok(countryDto);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.CountryId)
            {
                return BadRequest("Invalid Record Id");
            }
            var countryToBeUpdated = await _countriesRepository.GetAsync(id);
            if (countryToBeUpdated == null)
            {
                return NotFound($"Country with the Id: {id} doesn't exist");
            }
            _mapper.Map(updateCountryDto, countryToBeUpdated);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountryDto)
        {
            var country = _mapper.Map<Country>(createCountryDto);
            await _countriesRepository.AddAsync(country);
            return CreatedAtAction("GetCountry", new { id = country.CountryId }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            await _countriesRepository.DeleteAsync(id);
            return NoContent();
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.Exist(id);
        }
    }
}
