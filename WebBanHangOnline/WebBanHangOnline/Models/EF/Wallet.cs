using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_Wallet")]
    public class Wallet : CommonAbstract
    {
        [Key, ForeignKey("User")]
        public string Id { get; set; }
        public float point { get; set; }
        public bool status { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}