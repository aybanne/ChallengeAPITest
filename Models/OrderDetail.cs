using System.ComponentModel.DataAnnotations;

namespace ChallengeAPI.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }  // order_details_id

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string PizzaId { get; set; }  // foreign key to Pizza
        public Pizza Pizza { get; set; }

        public int Quantity { get; set; }
    }
}
