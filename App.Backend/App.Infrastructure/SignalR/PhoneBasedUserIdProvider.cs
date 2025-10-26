using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace App.Infrastructure.SignalR
{
    public class PhoneBasedUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value;
        }        
    }
}