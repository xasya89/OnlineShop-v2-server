using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Models
{
    internal record DocumentHistoryLegacy(int Id, int DocumentId, HistoryTypeDoc DocumentType, HistoryStatus Status, bool Processed);
    
    internal enum HistoryTypeDoc
    {
        ShiftStart = 0,
        ShiftStop = 1,
        Check = 2,
        Arrival = 3,
        WriteOf = 4,
        Revaluation = 5,
    }
    internal enum HistoryStatus
    {
        New = 0,
        Update = 1,
        Delete = 2
    }
}
