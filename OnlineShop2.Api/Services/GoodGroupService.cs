using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Services.Legacy;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Services
{
    public class GoodGroupService
    {
        private bool ownerGoodForShops;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly OnlineShopContext _context;
        private readonly GoodGroupLegacyService _legacy;

        public GoodGroupService(IConfiguration configuration, IMapper mapper, OnlineShopContext context, GoodGroupLegacyService legacy)
        {
            _configuration = configuration;
            _context = context;
            _legacy = legacy;
            _mapper = mapper;
            ownerGoodForShops = configuration.GetValue<bool>("OwnerGoodForShops");
        }

        public async Task<IEnumerable<GoodGroupCreateRequestModel>> GetAll(int shopId) =>
            _mapper.Map<IEnumerable<GoodGroupCreateRequestModel>>(
                await _context.GoodsGroups.Where(g => !ownerGoodForShops || g.ShopId == shopId).OrderBy(g => g.Name).ToListAsync()
                );

        public async Task<GoodGroupCreateRequestModel> Create(int shopId, GoodGroupCreateRequestModel model)
        {
            var group = _mapper.Map<GoodGroup>(model);
            group.ShopId = shopId;
            _context.Add(group);
            await _context.SaveChangesAsync();
            return _mapper.Map<GoodGroupCreateRequestModel>(group);
        }

        public async Task<GoodGroupCreateRequestModel> Update(GoodGroupCreateRequestModel model)
        {
            var group = await _context.GoodsGroups.FindAsync(model.Id);
            if (group == null) throw new MyServiceException($"Группа с id {model.Id} не найдена");
            _context.ChangeEntityByDTO<GoodGroupCreateRequestModel>(_context.Entry(group), model);
            await _context.SaveChangesAsync();
            return _mapper.Map<GoodGroupCreateRequestModel>(group);
        }

        public async Task Delete(int id)
        {
            var group = await _context.GoodsGroups.FindAsync(id);
            if (group == null) throw new MyServiceException($"Группа с id {id} не найдена");
            _context.Remove(group);
            await _context.SaveChangesAsync();
        }
    }
}
