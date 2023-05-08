using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Repositories
{
    public class UnitOfWorkLegacy : IUnitOfWorkLegacy
    {
        private string _connectionString;
        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            SupplierRepository.SetConnectionString(connectionString);
            GoodGroupRepository.SetConnectionString(connectionString);
            GoodRepository.SetConnectionString(connectionString);
            ShiftRepository.SetConnectionString(connectionString);
            CurrentBalance.SetConnectionString(connectionString);
            ArrivalRepository.SetConnectionString(connectionString);
        }

        public UnitOfWorkLegacy(ISupplierRepositoryLegacy supplier, 
            IGoodGroupRepositoryLegacy goodGroup, 
            IGoodReporitoryLegacy goodReporitory,
            IShiftRepositoryLegacy shiftRepository,
            ICurrentBalanceRepositoryLegacy currentBalance,
            IArrivalRepositoryLegacy arrival)
        {
            SupplierRepository= supplier;
            GoodGroupRepository = goodGroup;
            GoodRepository = goodReporitory;
            ShiftRepository = shiftRepository;
            CurrentBalance = currentBalance;
            ArrivalRepository = arrival;
        }

        public ISupplierRepositoryLegacy SupplierRepository { get; }
        public IGoodGroupRepositoryLegacy GoodGroupRepository { get; }

        public IGoodReporitoryLegacy GoodRepository { get; }

        public IShiftRepositoryLegacy ShiftRepository { get; }
        public ICurrentBalanceRepositoryLegacy CurrentBalance { get; }

        public IArrivalRepositoryLegacy ArrivalRepository { get; }
    }
}
