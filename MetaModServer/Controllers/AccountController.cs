﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Identity.LiteDB.Models;
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
            this._signInManager = signInManager;
            this._userManager   = userManager;
            this._logger        = logger;
            this._configuration = configuration;
        }
        
        [HttpPost, Route("/v1/Register"), AllowAnonymous]
        public async Task<StatusCodeResult> RegisterAsync(string name, string email, string password)
        {
            name     = name.Trim();
            password = password.Trim();
            email    = email.Trim();
            var user   = new ApplicationUser {UserName = name, Email = email};
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            // await this._signInManager.SignInAsync(user, false);
            this._logger.LogInformation("User created a new account with password.");
            return new StatusCodeResult(StatusCodes.Status202Accepted);
        }
        
        [HttpPost, Route("/v1/Login"), AllowAnonymous]
        public async Task<IActionResult> LoginAsync(string userName, string password, string audience)
        {
            userName = userName.Trim();
            password = password.Trim();
            var result = await _signInManager.PasswordSignInAsync(userName, password, true, false);

            if (!result.Succeeded)
                return this.Unauthorized();
            
            var user      = await this._userManager.FindByNameAsync(userName);
            var userRoles = await this._userManager.GetRolesAsync(user);  
  
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
  
            return this.Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
        
        [HttpPost, Route("/v1/Logout")]
        public async Task<StatusCodeResult> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return new StatusCodeResult(StatusCodes.Status200OK);
        }
        
    }
}