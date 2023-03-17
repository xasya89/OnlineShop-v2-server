using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Services;

namespace OnlineShop2.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefreshLoginController : ControllerBase
    {
        private readonly AuthService _service;
        public RefreshLoginController(AuthService service) => _service = service;

        [HttpPost]
        public async Task<IActionResult> post()
        {
            string? refreshstr = null;
            HttpContext.Request.Cookies.TryGetValue("refresh", out refreshstr);
            if (refreshstr == null)
                return Unauthorized();
            var user = await _service.RefreshLogin(refreshstr);
            if(user==null)
                return Unauthorized();
            HttpContext.Response.Cookies.Append("refresh", user.RefreshToken, CookieOptionClass.GetOption(DateTimeOffset.Now.AddDays(10)));
            return Ok(user);
        }
    }
}
