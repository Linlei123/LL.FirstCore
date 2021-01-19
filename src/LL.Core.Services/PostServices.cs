using LL.Core.IRepository;
using LL.Core.IServices;
using LL.Core.Model;
using LL.Core.Services.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.Core.Services
{
    public class PostServices : BaseServices<Post>, IPostServices
    {
        public PostServices(IPostRepository postRepository) : base(postRepository)
        {

        }
    }
}
