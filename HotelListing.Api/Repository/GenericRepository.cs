using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly HotelListingDbContext dbContext;
        private readonly IMapper mapper;

        public GenericRepository(HotelListingDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }
        public async Task<T> AddAsync(T entity)
        {
            await dbContext.AddAsync(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetAsync(id);
            if (entity is null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }
            dbContext.Set<T>().Remove(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistAsync(int id)
        {
            var entity =await GetAsync(id);
            return entity != null;
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await dbContext.Set<T>().ToListAsync();
        }

        public async Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters)
        {
            var totalSize = await dbContext.Set<T>().CountAsync();
            var items = await dbContext.Set<T>()
                .Skip(queryParameters.StartIndex)
                .Take(queryParameters.PageSize)
                .ProjectTo<TResult>(mapper.ConfigurationProvider)
                .ToListAsync();
            return new PagedResult<TResult>
            {
                Items=items,
                PageNumber=queryParameters.PageNumber,
                RecordNumber=queryParameters.PageSize,
                TotalCount=totalSize
            };
        }

        public async Task<T?> GetAsync(int? id)
        {
            if (id is null)
            {
                return null;
            }
            var entity = await dbContext.Set<T>().FindAsync(id);
            if (entity is null)
            {
                return null;
            }
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            dbContext.Update(entity);
            await dbContext.SaveChangesAsync();
        }
    }
}
