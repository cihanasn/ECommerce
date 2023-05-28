namespace ECommerce.Models.Response
{
    public class CartResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } // UserId özelliği eklendi
        public ICollection<CartLineResponse> CartLines { get; set; }
    }
}
