using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChallengeAPI.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }  // order_id

        [Required]
        public DateTime OrderDate { get; set; }  // combine date + time

        // Navigation property
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
