using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Models
{
    internal record DocumentHistoryLegacy(int Id, int DocumentId, HistoryTypeDocLegacy DocumentType, HistoryStatusLegacy Status, bool Processed);

    internal enum HistoryTypeDocLegacy
    {
        ShiftStart = 0,
        ShiftStop = 1,
        Check = 2,
        Arrival = 3,
        WriteOf = 4,
        Revaluation = 5,
        Stocktacking = 6,
    }
    internal enum HistoryStatusLegacy
    {
        New = 0,
        Update = 1,
        Delete = 2
    }
}
