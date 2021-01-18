using System;
using System.Collections.Generic;
using System.Text;

namespace LL.Core.Common.Jwt
{
    public interface IJwtProvider
    {
        string CreateJwtToken(TokenModel model);

        string GetUserId();
    }
}
