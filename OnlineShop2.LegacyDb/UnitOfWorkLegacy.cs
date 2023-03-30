using MySql.Data.MySqlClient;
using OnlineShop2.LegacyDb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb
{
    public class UnitOfWorkLegacy : IDisposable
    {
        private MySqlConnection _connection;
        private GoodLegacyRepository _goodRepository;
        private GoodCurrentBalanceLegacyRepository _goodCountCurrentRepository;
        private ShiftLegacyRepository _shiftLegacyRepository;
        public GoodLegacyRepository GoodRepository
        {
            get
            {
                if (_goodRepository == null)
                    _goodRepository = new GoodLegacyRepository(_connection);
                return _goodRepository;
            }
        }
        public GoodCurrentBalanceLegacyRepository GoodCountCurrentRepository
        {
            get
            {
                if (_goodCountCurrentRepository == null)
                    _goodCountCurrentRepository = new GoodCurrentBalanceLegacyRepository(_connection);
                return _goodCountCurrentRepository;
            }
        }
        public ShiftLegacyRepository ShiftLegacyRepository
        {
            get
            {
                if (_shiftLegacyRepository == null)
                    _shiftLegacyRepository = new ShiftLegacyRepository(_connection);
                return _shiftLegacyRepository;
            }
        }
        public UnitOfWorkLegacy(string conStr)
        {
            _connection = new MySqlConnection(conStr);
            _connection.Open();
        }
        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
