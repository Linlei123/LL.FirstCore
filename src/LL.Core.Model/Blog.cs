using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LL.Core.Model
{
    [Table("Blog")]
    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }
    }
}
