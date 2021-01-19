using LL.Core.IRepository;
using LL.Core.IServices;
using LL.Core.Model.Models;
using LL.Core.Services.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.Core.Services
{
    public class RequestLogServices : BaseServices<RequestLogEntity>, IRequestLogServices
    {
        public RequestLogServices(IRequestLogRepository postRepository) : base(postRepository)
        {

        }
    }
}
