﻿using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Server.Exceptions;
using Model.HandlersData;
using Server.Model;

namespace Server.Handlers
{
    public class LogoutHandler : SimpleJsonHandler<EmptyData, LogoutResponse>
    {
        private readonly AuthorizationManager manager;

        public LogoutHandler(AuthorizationManager manager) : base(manager)
        {
            this.manager = manager;
        }

        public override Regex Endpoint => new Regex("^/logout$");
        public override string HttpMethod => "GET";

        public override Task<LogoutResponse> Handle(EmptyData _)
        {
            if (CurrentUser == null)
            {
                throw new HttpException(401);
            }

            manager.DeauthorizeUser(CurrentUser);

            return Task.FromResult(new LogoutResponse
            {
                Message = "bye!"
            });
        }
    }
}
