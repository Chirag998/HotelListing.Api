
using HotelListing.Api.Data;

namespace HotelListing.Api.Contracts
{
    public interface ICountriesRepository : IGenericRepository<Country>
    {
        Task<Country?> GetCountryWithHotels(int id);
        Task<IEnumerable<string>> GetCountryNames();
    }
}
