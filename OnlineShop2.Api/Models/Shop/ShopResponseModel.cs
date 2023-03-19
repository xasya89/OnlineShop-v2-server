namespace OnlineShop2.Api.Models.Shop
{
    public class ShopResponseModel
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public string OrgName { get; set; }
        public string Inn { get; set; }
        public string? Kpp { get; set; }
        public string Adress { get; set; }
        public int? LegacyDbNum { get; set; }
    }
}
