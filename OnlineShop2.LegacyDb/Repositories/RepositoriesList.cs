using MySql.Data.MySqlClient;
using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Repositories
{
    public interface IGeneralRepositoryLegacy<T> where T: class
    {
        void SetConnectionString(string connectionString);
        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<int> AddAsync(T entity);
        Task<IReadOnlyCollection<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }

    public interface ISupplierRepositoryLegacy: IGeneralRepositoryLegacy<SupplierLegacy> { }
    public interface IGoodGroupRepositoryLegacy: IGeneralRepositoryLegacy<GoodGroupLegacy> { }

    public interface IArrivalRepositoryLegacy : IGeneralRepositoryLegacy<ArrivalLegacy> 
    {
        Task<IEnumerable<ArrivalLegacy>> GetArrivalWithDate(DateTime with);
    }
}
