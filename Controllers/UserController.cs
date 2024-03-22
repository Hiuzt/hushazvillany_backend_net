using hushazvillany_backend.Data;
using hushazvillany_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace hushazvillany_backend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly  AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext appDbContext, IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _appDbContext.Users;

            return Ok(users);
        }


        [Authorize]
        [HttpPost]
        [Route("Register")]
        public async Task <IActionResult> Register(Users user)
        {
            if (_appDbContext.Users.Any(u => u.Username == user.Username)) {
                return BadRequest("Ez a felhasználónév már regisztrálva van!");
            }

            if (_appDbContext.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("Ez az E-mail cím már regisztrálva van!");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);


            _appDbContext.Users.Add(user); 
            await _appDbContext.SaveChangesAsync();

            return Ok(new {success = true, data = new {message = "Sikeresen létrehoztál egy felhasználót"}});
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(Users user)
        {
            try
            {
                Users userSource = _appDbContext.Users.First(u => u.Username == user.Username);
             
                if (userSource != null)
                {

                    if (!BCrypt.Net.BCrypt.Verify(user.Password, userSource.Password))
                    {
                        return BadRequest("Ehhez a felhasználóhoz nem ez a jelszó párosul!");
                    }
                } else
                {
                    return BadRequest("Nem létezik ilyen felhasználónév");
                }


                var userToken = CreateToken(userSource);

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(1),
                    HttpOnly = true,
                    Path = "/",
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    
                };

                Response.Cookies.Append("token", userToken, cookieOptions);

                return Ok(new { success = true, data = new { token = userToken } });
            } catch (Exception ex)
            {
                return BadRequest("Szerver hiba:" + ex);
            }
           
        }

        [Authorize]
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var userSource = _appDbContext.Users.FirstOrDefault(x => x.Id == id);
                Console.WriteLine(id);
                if (userSource == null)
                {
                    return BadRequest("Nincs ilyen felhasználó!");
                }

                _appDbContext.Users.Remove(userSource);
                await _appDbContext.SaveChangesAsync();

                return Ok(new { success = true, data = new { message = "A felhasználó sikeresen kitörölve" } });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }

        }

        [Authorize]
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateOnUser(int id, Users user)
        {
            try
            {
                var userSource = _appDbContext.Users.FirstOrDefault(x => x.Id == id);
                if (userSource == null)
                {
                    return BadRequest("Nincs ilyen felhasználó!");
                }

                if (user.Password.Length > 0)
                {
                    userSource.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }
                userSource.Username = user.Username;
                userSource.Email = user.Email; 
                userSource.Name = user.Name;

                await _appDbContext.SaveChangesAsync();

                return Ok(new { success = true, data = new { message = "Egy felhasználó sikeresen szerkesztve lett" } });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }

        }

        [NonAction]
        public string CreateToken(Users user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("id", user.Id.ToString()),
                    new Claim("name", user.Name.ToString()),
                    new Claim("email", user.Email.ToString())
                }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var jwtToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(jwtToken);
        }

        [HttpGet]
        [Route("loggedIn")]

        public IActionResult isLoggedIn()
        {
            var token = Request.Cookies["token"];
            if (token == null)
            {
                return Unauthorized(new { success = true, data = new { loggedIn = false } });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return Ok(new { success = true, data = new { loggedIn = true } });
            } catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
