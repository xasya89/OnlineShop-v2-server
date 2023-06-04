using MySql.Data.MySqlClient;
using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Repositories
{
    public interface IUnitOfWorkLegacy
    {
        void SetConnectionString(string connectionString);
        ISupplierRepositoryLegacy SupplierRepository { get; }
        IGoodReporitoryLegacy GoodRepository { get; }
        IGoodGroupRepositoryLegacy GoodGroupRepository { get; }
        IShiftRepositoryLegacy ShiftRepository { get; }
        ICurrentBalanceRepositoryLegacy CurrentBalance { get; }
        IArrivalRepositoryLegacy ArrivalRepository { get; }
        IWriteofRepositoryLegacy WriteofRepositoryLegacy { get; }
        IRevaluationRepositoryLegacy RevaluationRepositoryLegacy { get; }
    }
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
    public interface IGoodReporitoryLegacy: IGeneralRepositoryLegacy<GoodLegacy>
    {
        Task<int> AddAsync(GoodLegacy entity, int shopLegacyId);
        Task<IReadOnlyCollection<GoodLegacy>> AddRangeAsync(IEnumerable<GoodLegacy> entities, int shopLegacyId);
    }

    public interface IArrivalRepositoryLegacy : IGeneralRepositoryLegacy<ArrivalLegacy> 
    {
        Task<IEnumerable<ArrivalLegacy>> GetArrivalWithDate(DateTime with);
    }
}
