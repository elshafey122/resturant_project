using EcommerceApi.api.Models.identity;
using EcommerceApi.api.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceApi.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _usermanager;
        public AccountController(UserManager<ApplicationUser> usermanager)
        {
            _usermanager = usermanager;
        }
        [HttpPost("register")]
        public async Task <IActionResult> Register(RegisterViewModel useregister)
        {
            if(ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = useregister.UserName;
                user.Email = useregister.Email;
                await _usermanager.AddToRoleAsync(user, "user");
                IdentityResult result = await _usermanager.CreateAsync(user, useregister.Password);
                if(result.Succeeded)
                {
                    await _usermanager.AddToRoleAsync(user,"user");
                    return Ok("added");
                }    
                return BadRequest(result.Errors.FirstOrDefault());
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel userlog)
        {
            if(ModelState.IsValid)
            {
                ApplicationUser user = await _usermanager.FindByNameAsync(userlog.UserName);
                if(user!=null)
                {
                    var userss = await _usermanager.Users.ToListAsync();
                    bool found =await _usermanager.CheckPasswordAsync(user, userlog.Password);
                    
                    if(found)
                    {
                        var myclaims = new List<Claim>();
                        myclaims.Add(new Claim(ClaimTypes.Name, user.UserName));
                        myclaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        myclaims.Add(new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()));

                        var rules = await _usermanager.GetRolesAsync(user);
                        foreach (var rule in rules)
                        {
                            myclaims.Add(new Claim(ClaimTypes.Role, rule));
                        }

                        SecurityKey SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my custom Secret key for authentication"));
                        SigningCredentials mysigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);

                        JwtSecurityToken mytoken = new JwtSecurityToken(
                            issuer: "https://localhost:44372/",
                            audience: "https://localhost:4200/",
                            claims : myclaims,
                            expires: DateTime.Now.AddHours(3),
                            signingCredentials: mysigningCredentials
                        );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(mytoken),
                            expiration = mytoken.ValidTo
                        });
                    }
                }
                return Unauthorized();
            }
            return Unauthorized();
        }
    }
}
