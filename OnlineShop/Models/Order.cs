using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.Models
{
    public class Order
    {
        [DisplayFormat(DataFormatString = "{0:000000}")]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Total { get; set; }

        [Required]
        public string ReceiverName { get; set; }
        [Required]
        public string ReceiverAdress { get; set; }
        [Required]
        public string ReceiverPhone { get; set; }

        public bool isPaid { get; set; }
        public List<OrderItem> OrderItem { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Amount { get; set; }
        public int SubTotal { get; set; }
    }

    public class CartItem : OrderItem
    {
        public CartItem() { }
        public CartItem(OrderItem order)
        {
            this.OrderId = order.OrderId;
            this.ProductId = order.ProductId;
            this.Amount = order.Amount;
            this.SubTotal = order.SubTotal;
        }

        public Product Product { get; set; }
        public string imageSrc { get; set; }
    }
}
