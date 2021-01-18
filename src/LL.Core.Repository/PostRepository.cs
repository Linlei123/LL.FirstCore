using LL.Core.IRepository;
using LL.Core.IRepository.Base;
using LL.Core.Model;
using LL.Core.Repository.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.Core.Repository
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }
    }
}
