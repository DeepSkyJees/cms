﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Models;
using SSCMS.Core.Utils;
using System.Collections.Generic;

namespace SSCMS.Web.Controllers.Admin.Settings.Logs
{
    public partial class LogsUserController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<PageResult<Log>>> Get([FromBody] SearchRequest request)
        {
            if (!await _authManager.HasAppPermissionsAsync(MenuUtils.AppPermissions.SettingsLogsUser))
            {
                return Unauthorized();
            }

            var userId = 0;
            if (!string.IsNullOrEmpty(request.UserName))
            {
                var user = await _userRepository.GetByUserNameAsync(request.UserName);
                if (user == null)
                {
                    return new PageResult<Log>
                    {
                        Items = new List<Log>(),
                        Count = 0,
                    };
                }
                userId = user.Id;
            }

            var count = await _logRepository.GetUserLogsCountAsync(userId, request.Keyword, request.DateFrom, request.DateTo);
            var userLogs = await _logRepository.GetUserLogsAsync(userId, request.Keyword, request.DateFrom, request.DateTo, request.Offset, request.Limit);
            var logs = new List<Log>();

            foreach (var log in userLogs)
            {
                var user = await _userRepository.GetByUserIdAsync(log.UserId);
                if (user == null) continue;

                var userName = _userRepository.GetDisplay(user);
                log.Set("userName", userName);
                log.Set("userGuid", user.Guid);
                logs.Add(log);
            }

            return new PageResult<Log>
            {
                Items = logs,
                Count = count
            };
        }
    }
}
