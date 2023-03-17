namespace OnlineShop2.Api.Extensions
{
    public static class CookieOptionClass
    {
        public static CookieOptions GetOption(DateTimeOffset expires) => new CookieOptions
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Expires= expires,
        };
    }
}
