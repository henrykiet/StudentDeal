using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_Order")]
    public class Order:CommonAbstract
    {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        public Order()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
        }
        public enum PaymentMethod
        {
            Ví = 1
            // Thêm các giá trị enum khác nếu cần
        }
        [Required]
        public string Code { get; set; }
        [Required(ErrorMessage = "Tên khách hàng không để trống")]
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        [Required(ErrorMessage = "Địa chỉ khổng để trống")]
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalAmount { get; set; }
        public int Quantity { get; set; }
        public int TypePayment { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        // nhiều 1 với user
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}