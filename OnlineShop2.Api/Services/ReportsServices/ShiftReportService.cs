using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Models.ReportsModels;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Services.ReportsServices
{
    public class ShiftReportService
    {
        private readonly OnlineShopContext _context;
        private readonly IMapper _mapper;
        public ShiftReportService(OnlineShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;   
        }

        public async Task<IEnumerable<ShiftResponseModel>> GetShifts(int shopId, DateTime with, DateTime by)
        {
            with = DateOnly.FromDateTime(with).ToDateTime(TimeOnly.MinValue);
            by = DateOnly.FromDateTime(by).ToDateTime(TimeOnly.MinValue);
            var shifts = await _context.Shifts.Where(x => x.ShopId == shopId & x.Start >= with & x.Start < by.AddDays(1)).AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<ShiftResponseModel>>(shifts);
        }

        public async Task<IEnumerable<ShiftSummaryResponse>> GetSummary(int shiftId)
        {
            var shiftSummaries = await _context.ShiftSummaries.Where(x => x.ShiftId == shiftId).Include(x => x.Good)
                .AsNoTracking().ToListAsync();

            return _mapper.Map<IEnumerable<ShiftSummaryResponse>>(shiftSummaries);
        }

        public async Task<ShiftWithChecksResponseModel> GetOne(int shiftId)
        {
            var shift = await _context.Shifts.Include(s => s.CheckSells).ThenInclude(c => c.CheckGoods).ThenInclude(g => g.Good)
                .Where(x => x.Id == shiftId).AsNoTracking().FirstAsync();
            return _mapper.Map<ShiftWithChecksResponseModel>(shift);
        }
    }
}
