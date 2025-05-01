using AutoMapper;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.Api.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper mapper;
        private readonly UserManager<ApiUser> userManager;
        private readonly IConfiguration configuration;

        public AuthManager(IMapper mapper,UserManager<ApiUser> userManager,IConfiguration configuration)
        {
            this.mapper = mapper;
            this.userManager = userManager;
            this.configuration = configuration;
        }

        public async Task<AuthResponseDto?> Login(LoginDto loginDto)
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return null;
            }
            var IsValidUser = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!IsValidUser)
            {
                return null;
            }
            var token = await GenerateToken(user);
            return new AuthResponseDto
            {
                Token=token,
                UserId=user.Id
            };
        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto apiUserDto)
        {
            var user = mapper.Map<ApiUser>(apiUserDto);
            user.UserName = apiUserDto.Email;

            var result = await userManager.CreateAsync(user, apiUserDto.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
            }
            return result.Errors;
        }

        private async Task<string> GenerateToken(ApiUser apiUser)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]));
            var signingCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,apiUser.Id),
                new Claim(ClaimTypes.Email,apiUser.Email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var userRoles = await userManager.GetRolesAsync(apiUser);

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims:authClaims,
                expires:DateTime.Now.AddMinutes(Convert.ToInt32(configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials:signingCredentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
