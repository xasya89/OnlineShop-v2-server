using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.CurrentBalance;
using OnlineShop2.Api.Services;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Controllers.CurrentBalance
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodCurrentBalanceController : ControllerBase
    {
        private readonly CurrentBalanceService _service;
        private readonly IMapper _mapper;
        public GoodCurrentBalanceController(CurrentBalanceService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("/api/{shopId}/currentbalance")]
        public async Task<IEnumerable<CurrentBalanceResponseModel>> Get(int shopId, [FromQuery] bool skipDeleted, [FromQuery(Name = "groups")] int[] groups, [FromQuery] int[] suppliers) 
            => await _service.GetBalance(shopId, skipDeleted, groups, suppliers);
    }
}
