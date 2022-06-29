using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Identity.LiteDB.Models;
using MetaModFramework.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MetaModFramework.Controllers
{
    [ApiController, Route("/v1/"), Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration                 _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser>   _userManager;
        private readonly ILogger<AccountController>     _logger;
        
        public AccountController(SignInManager<ApplicationUser> signInManager,
                                 UserManager<ApplicationUser>   userManager,
                                 ILogger<AccountController>     logger,
                                 IConfiguration                 configuration)
        {
            _signInManager = signInManager;
            _userManager   = userManager;
            _logger        = logger;
            _configuration = configuration;
        }
        
        [HttpPost, Route("/v1/Register"), AllowAnonymous]
        public async Task<StatusCodeResult> RegisterAsync(string name, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(password))
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            name     = name.Trim();
            password = password.Trim();
            email    = email.Trim();
            var user   = new ApplicationUser {UserName = name, Email = email};
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            // await this._signInManager.SignInAsync(user, false);
            _logger.LogInformation("User created a new account with password.");
            return new StatusCodeResult(StatusCodes.Status202Accepted);
        }
        
        [HttpPost, Route("/v1/Login"), AllowAnonymous]
        public async Task<IActionResult> LoginAsync(string userName, string password, string audience)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            
            userName = userName.Trim();
            password = password.Trim();
            var result = await _signInManager.PasswordSignInAsync(userName, password, true, false);

            if (!result.Succeeded)
                return Unauthorized();
            
            var user      = await _userManager.FindByNameAsync(userName);
            var userRoles = await _userManager.GetRolesAsync(user);  
  
            var authClaims = new List<Claim>  
                             {  
                                 new(ClaimTypes.Name,             user.UserName),  
                                 new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
                             };
            
            authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));  
  
            var token = new JwtSecurityToken(  
                                             _configuration["JWT:ValidIssuer"],  
                                             audience: audience,
                                             expires: DateTime.Now.AddHours(24),
                                             claims: authClaims,
                                             signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha512)
                                            );
            
            ServiceTransactions.AddUser(user.UserName);
            
            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
        
        [HttpPost, Route("/v1/Logout")]
        public async Task<StatusCodeResult> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return new StatusCodeResult(StatusCodes.Status200OK);
        }
        
    }
}