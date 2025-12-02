using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChallengeAPI.Models
{
    public class PizzaType
    {
        [Key]
        public string Id { get; set; }  // pizza_type_id

        [Required]
        public string Name { get; set; }

        public string Category { get; set; }

        public string Ingredients { get; set; }  // comma-separated

        // Navigation property
        public ICollection<Pizza> Pizzas { get; set; } = new List<Pizza>();
    }
}
