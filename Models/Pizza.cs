using System.ComponentModel.DataAnnotations;

namespace ChallengeAPI.Models
{
    public class Pizza
    {
        [Key]
        public string Id { get; set; }  // pizza_id

        public string PizzaTypeId { get; set; }
        public PizzaType PizzaType { get; set; }

        public string Size { get; set; }

        public decimal Price { get; set; }
    }
}
