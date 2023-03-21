namespace OnlineShop2.Api.Models.Inventory
{
    public class InventoryAddGoodRequestModel
    {
        public int id { get; set; }
        public int GroupId { get; set; }
        public int GoodId { get; set; }
        public decimal? CountFact { get; set; }
        public InventoryAddGoodRequestState State { get; set; }
    }

    public enum InventoryAddGoodRequestState
    {
        Add=1, Load=0, Edit=2, Delete=3
    }
}
