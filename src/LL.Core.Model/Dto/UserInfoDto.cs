using System;
using System.Collections.Generic;
using System.Text;

namespace LL.Core.Model.Dto
{
    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string RealName { get; set; }
        public int Gender { get; set; } = 0;
        public string Mobile { get; set; }
    }
}
