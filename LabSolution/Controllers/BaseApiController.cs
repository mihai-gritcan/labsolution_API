using LabSolution.Infrastructure;
using LabSolution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LabSolution.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected void EnsureSuperUserPerformsTheAction()
        {
            var isSuperUserValue = User?.Claims.FirstOrDefault(x => x.Type.Equals(LabSolutionClaimsNames.UserIsSuperUser))?.Value;
            var canManageUsers = isSuperUserValue?.Equals("true", StringComparison.InvariantCultureIgnoreCase) == true;

            if (!canManageUsers)
                throw new CustomException("This user doens't have the rights to manage app users");
        }
    }
}
