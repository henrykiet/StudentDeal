using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_Product")]
    public class Product:CommonAbstract
    {
        public Product()
        {
            this.ProductImage = new HashSet<ProductImage>();
            this.OrderDetails = new HashSet<OrderDetail>();
            this.Reviews = new HashSet<Review>();

        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(250)]
        public string Title { get; set; }
        [Required]
        public bool IsStatus { get; set; }

        [StringLength(50)]
        public string ProductCode { get; set; }
        public string Description { get; set; }

        [AllowHtml]
        public string Detail { get; set; }
        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "EndDate is required.")]
        public DateTime endDate { get; set; }
        [Required(ErrorMessage = "Quantity is required.")]
        public int Quantity { get; set; }
        public int TotalQuantity { get; set; }
        [StringLength(250)]
        public string Image { get; set; }
        public decimal Price { get; set; }
        public decimal? PriceSale { get; set; }
        public int ViewCount { get; set; }
        public bool IsHome { get; set; }
        public bool IsSale { get; set; }
        public bool IsFeature { get; set; }
        public bool IsHot { get; set; }
        public bool IsActive { get; set; }
        public int ProductCategoryId { get; set; }

        [StringLength(250)]
        public string SeoTitle { get; set; }
        [StringLength(500)]
        public string SeoDescription { get; set; }
        [StringLength(250)]
        public string SeoKeywords { get; set; }

        public virtual ProductCategory ProductCategory { get; set; }
        public virtual ICollection<ProductImage> ProductImage { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

    }
}