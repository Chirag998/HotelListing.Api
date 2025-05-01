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
using Asp.Versioning;

namespace HotelListing.Api.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/countries")]
    public class CountriesV2Controller : ControllerBase
    {
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesV2Controller> logger;

        public CountriesV2Controller(HotelListingDbContext context,IMapper mapper,ICountriesRepository countriesRepository,
            ILogger<CountriesV2Controller> logger)
        {
            _context = context;
            _mapper = mapper;
            _countriesRepository = countriesRepository;
            this.logger = logger;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<string>>> GetCountries()
        {
            var countryNames = await _countriesRepository.GetCountryNames();
            return Ok(countryNames);
        }
    }
}
