using LL.Core.IRepository;
using LL.Core.IRepository.Base;
using LL.Core.Model.Models;
using LL.Core.Repository.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.Core.Repository
{
    public class RequestLogRepository : BaseRepository<RequestLogEntity>, IRequestLogRepository
    {
        public RequestLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }
    }
}
