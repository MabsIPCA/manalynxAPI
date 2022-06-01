/*
 * lufer
 * ISI
 * See https://dotnetcorecentral.com/blog/asp-net-core-authorization/
 * */
using Microsoft.AspNetCore.Authorization;
using ManaLynxAPI.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ManaLynxAPI.Authentication
{
    public class AuthorizedRequirement : IAuthorizationRequirement
    {
        public AuthorizedRequirement()
        {
        }
    }

    public class Auth : AuthorizeAttribute
    {
        public Auth(params Roles[] roles)
        {
            foreach(var role in roles)
            {
                base.Roles += role.ToString();
                base.Roles += ",";
            }
        }

        private Roles roleEnum;
        public Roles RoleEnum
        {
            get { return roleEnum; }
            set { roleEnum = value; base.Roles = value.ToString(); }
        }
    }

    //public class AuthorizedRequirementHandler : AuthorizationHandler<AuthorizedRequirement>
    //{
    //    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizedRequirement requirement)
    //    {
    //        if (!context.User.HasClaim(x => x.Type == ClaimTypes.Email))
    //            return Task.CompletedTask;

    //        var emailAddress = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;

    //        if (ManaUser.users.Any(x => x.Email == emailAddress))
    //        {
    //            context.Succeed(requirement);
    //        }

    //        return Task.CompletedTask;
    //    }
    //}

   
}