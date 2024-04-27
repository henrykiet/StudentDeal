using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models
{
    public class ShoppingCart
    {
        public List<ShoppingCartItem> Items { get; set; }
        public float TotalAmount { get; set; } // Thêm thuộc tính TotalAmount

        public ShoppingCart()
        {
            this.Items = new List<ShoppingCartItem>();
            this.TotalAmount = 0;
        }

        public void AddToCart(ShoppingCartItem item, int Quantity)
        {
            var checkExits = Items.FirstOrDefault(x => x.ProductId == item.ProductId);
            if (checkExits != null)
            {
                checkExits.Quantity += Quantity;
                checkExits.TotalPrice = checkExits.Price * checkExits.Quantity;
            }
            else
            {
                Items.Add(item);
            }
            // Cập nhật TotalAmount sau khi thêm sản phẩm vào giỏ hàng
            CalculateTotalAmount();
        }

        public void Remove(int id)
        {
            var checkExits = Items.SingleOrDefault(x => x.ProductId == id);
            if (checkExits != null)
            {
                Items.Remove(checkExits);
            }
            // Cập nhật TotalAmount sau khi xóa sản phẩm khỏi giỏ hàng
            CalculateTotalAmount();
        }

        public void UpdateQuantity(int id, int quantity)
        {
            var checkExits = Items.SingleOrDefault(x => x.ProductId == id);
            if (checkExits != null)
            {
                checkExits.Quantity = quantity;
                checkExits.TotalPrice = checkExits.Price * checkExits.Quantity;
            }
            // Cập nhật TotalAmount sau khi cập nhật số lượng sản phẩm trong giỏ hàng
            CalculateTotalAmount();
        }

        public float GetTotalPrice()
        {
            return Items.Sum(x => x.TotalPrice);
        }

        public int GetTotalQuantity()
        {
            return Items.Sum(x => x.Quantity);
        }

        public void ClearCart()
        {
            Items.Clear();
            TotalAmount = 0; // Đặt TotalAmount về 0 khi xóa toàn bộ giỏ hàng
        }

        private void CalculateTotalAmount()
        {
            TotalAmount = (float)Items.Sum(x => x.TotalPrice);
        }
    }

    public class ShoppingCartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Alias { get; set; }
        public string CategoryName { get; set; }
        public string ProductImg { get; set; }
        public int Quantity { get; set; }
        public float Price { get; set; }
        public float TotalPrice { get; set; }
    }
}
