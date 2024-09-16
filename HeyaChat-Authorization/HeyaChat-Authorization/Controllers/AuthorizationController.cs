using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Users.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

namespace HeyaChat_Authorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private IConfiguration _config;
        private AuthorizationDBContext _context;
        private IUsersRepository _usersRepository;
        
        public AuthorizationController(IConfiguration config, AuthorizationDBContext context, IUsersRepository usersRepository)
        {
            _config = config ?? throw new NullReferenceException(nameof(config));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _usersRepository = usersRepository;
        }

        [HttpPost]
        [Route("Register")]
        public IActionResult Register(RegisterDRO requestObject)
        {
            



            return StatusCode(StatusCodes.Status304NotModified);
        }



    }
}
