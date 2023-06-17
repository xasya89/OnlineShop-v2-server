using MySql.Data.MySqlClient;
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
        IStocktackingRepositoryLegacy StocktackingRepositoryLegacy { get; }
        IMoneyReportRepositoryLegacy MoneyReportRepositoryLegacy { get; }
    }

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
            WriteofRepositoryLegacy.SetConnectionString(connectionString);
            RevaluationRepositoryLegacy.SetConnectionString(connectionString);
            StocktackingRepositoryLegacy.SetConnectionString(connectionString);
            MoneyReportRepositoryLegacy.SetConnectionString(connectionString);
        }

        public UnitOfWorkLegacy(ISupplierRepositoryLegacy supplier, 
            IGoodGroupRepositoryLegacy goodGroup, 
            IGoodReporitoryLegacy goodReporitory,
            IShiftRepositoryLegacy shiftRepository,
            ICurrentBalanceRepositoryLegacy currentBalance,
            IArrivalRepositoryLegacy arrival,
            IWriteofRepositoryLegacy writeof, 
            IRevaluationRepositoryLegacy revaluation,
            IStocktackingRepositoryLegacy stocktacking,
            IMoneyReportRepositoryLegacy moneyReport)
        {
            SupplierRepository= supplier;
            GoodGroupRepository = goodGroup;
            GoodRepository = goodReporitory;
            ShiftRepository = shiftRepository;
            CurrentBalance = currentBalance;
            ArrivalRepository = arrival;
            WriteofRepositoryLegacy = writeof;
            RevaluationRepositoryLegacy = revaluation;
            StocktackingRepositoryLegacy = stocktacking;
            MoneyReportRepositoryLegacy = moneyReport;
        }

        public ISupplierRepositoryLegacy SupplierRepository { get; }
        public IGoodGroupRepositoryLegacy GoodGroupRepository { get; }

        public IGoodReporitoryLegacy GoodRepository { get; }

        public IShiftRepositoryLegacy ShiftRepository { get; }
        public ICurrentBalanceRepositoryLegacy CurrentBalance { get; }

        public IArrivalRepositoryLegacy ArrivalRepository { get; }

        public IWriteofRepositoryLegacy WriteofRepositoryLegacy { get; }

        public IRevaluationRepositoryLegacy RevaluationRepositoryLegacy { get; }

        public IStocktackingRepositoryLegacy StocktackingRepositoryLegacy { get; }

        public IMoneyReportRepositoryLegacy MoneyReportRepositoryLegacy { get; }
    }
}
