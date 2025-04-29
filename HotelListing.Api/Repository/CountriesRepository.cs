using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repository
{
    public class CountriesRepository : GenericRepository<Country>, ICountriesRepository
    {
        private readonly HotelListingDbContext dbContext;

        public CountriesRepository(HotelListingDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Country?> GetCountryWithHotels(int id)
        {
            return await dbContext.Countries.Include(x => x.Hotels).FirstOrDefaultAsync(x => x.CountryId == id);
        }
    }
}
