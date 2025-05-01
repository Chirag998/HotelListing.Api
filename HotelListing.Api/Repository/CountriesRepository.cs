using AutoMapper;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repository
{
    public class CountriesRepository : GenericRepository<Country>, ICountriesRepository
    {
        private readonly HotelListingDbContext dbContext;
        private readonly IMapper mapper;

        public CountriesRepository(HotelListingDbContext dbContext,IMapper mapper) : base(dbContext,mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public async Task<Country?> GetCountryWithHotels(int id)
        {
            return await dbContext.Countries.Include(x => x.Hotels).FirstOrDefaultAsync(x => x.CountryId == id);
        }
    }
}
