using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QBOAuthenticate.Models;
using QBOAuthenticate.Services;
using QBOAuthenticate.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QBOAuthenticate.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]   
    public class AuthenticationController : ControllerBase
    {
        private IUserService _userService;
        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost]
        public IActionResult Authenticate([FromBody]Authenticate model)
        {
            var user = _userService.Authenticate(model);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

    }
}
