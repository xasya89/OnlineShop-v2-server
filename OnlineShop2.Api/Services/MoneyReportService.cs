using OnlineShop2.Database;
using OnlineShop2.Api.Models.ReportMessage;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace OnlineShop2.Api.Services
{
    public class MoneyReportService
    {
        private readonly OnlineShopContext _context;
        private readonly IMapper _mapper;
        public MoneyReportService(OnlineShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MoneyReportResponseModel>> Get(int shopId, DateTime with, DateTime by) =>
            _mapper.Map<IEnumerable<MoneyReportResponseModel>>(
                    await _context.MoneyReports.Where(x => x.ShopId == shopId & x.Create >= with & x.Create <= by)
                    .AsNoTracking().ToListAsync()
                );
    }
}
