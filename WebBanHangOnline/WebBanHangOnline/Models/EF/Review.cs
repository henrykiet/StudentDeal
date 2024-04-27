using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models.EF
{
    [Table("tb_Review")]
    public class Review
    {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string comment { get; set; }
        public int rating { get; set; }
        public DateTime DateVote { get; set; }
        public string Name { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }

        public virtual Product Product { get; set; }

        public virtual ApplicationUser User { get; set; }

    }
}