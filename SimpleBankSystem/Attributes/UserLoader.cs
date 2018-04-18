using Microsoft.AspNetCore.Mvc.Filters;
using SimpleBankSystem.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBankSystem.Attributes
{
    public class UserLoader : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller != null && context.Controller is BaseController)
            {
                var controllerInstance = (context.Controller as BaseController);
                controllerInstance.GetCurrentUser();
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
