using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Services;
using OnlineShop2.Dao.DaoModels;

namespace OnlineShop2.Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AuthService _service;
        public LoginController(AuthService service) => _service = service;

        [HttpPost]
        public async Task<IActionResult> post(LoginDao model)
        {
            var user = await _service.Login(model.Login, model.Password);
            if(user == null) 
                return Unauthorized();
            HttpContext.Response.Cookies.Append("refresh", user.RefreshToken, CookieOptionClass.GetOption(DateTimeOffset.Now.AddDays(10)));
            return Ok(user);
        }
    }
}
