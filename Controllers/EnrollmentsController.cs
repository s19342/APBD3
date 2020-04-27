using System;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using APBD3.DTOs.Requests;
using APBD3.DTOs.Responses;
using APBD3.Handlers;
using APBD3.Models;
using APBD3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace APBD3.Controllers
{
    [ApiController]
    [Route("api/enrollment")]
    [Authorize(Roles = "employee")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;

        public IConfiguration Configuration { get; set; }

        public EnrollmentsController(IStudentsDbService dbService, IConfiguration configuration)
        {
            _dbService = dbService;
            Configuration = configuration;
        }

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            EnrollStudentResponse enrollStudentResponse = _dbService.EnrollStudent(request);

            if(enrollStudentResponse == null)
            {
                return BadRequest();
            }

            return this.StatusCode(201, enrollStudentResponse);
                
        }

        [HttpPost("promotions")]
        public IActionResult PromoteStudents(PromoteStudentRequest promoteStudentRequest)
        {
            PromoteStudentResponse promoteStudentResponse = _dbService.PromoteStudents(promoteStudentRequest);

            if(promoteStudentResponse == null)
            {
                return NotFound();
            }

            return this.StatusCode(201, promoteStudentResponse);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(LoginRequestDto request)
        {
            LoginAttemptResponse loginAttemptResponse = _dbService.checkLogin(request.Login);

            if(loginAttemptResponse == null)
            {
                return NotFound("That index number does not exist in the database");
            }

            if(!AuthHandler.Validate(request.Password, loginAttemptResponse.Salt, loginAttemptResponse.Hash))
            {
                return BadRequest("Incorrect Password");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.Login),
                new Claim(ClaimTypes.Name, loginAttemptResponse.FirstName),
                new Claim(ClaimTypes.Role, "employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "employee",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var refreshToken = Guid.NewGuid();

            string refreshTokenString = refreshToken.ToString();

            var tokenCreated = new Token
            {
                TokenString = refreshTokenString,
                NameIdentifier = request.Login,
                FirstName = loginAttemptResponse.FirstName
            };

            _dbService.RecordToken(tokenCreated);

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });
        }

        [HttpPost("refresh-token/{token}")]
        [AllowAnonymous]
        public IActionResult RefreshToken(string requestToken)
        {
            var givenToken = _dbService.validateToken(requestToken);

            if(givenToken == null)
            {
                return BadRequest("Incorrect token");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, givenToken.NameIdentifier),
                new Claim(ClaimTypes.Name, givenToken.FirstName),
                new Claim(ClaimTypes.Role, "employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "employee",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var refreshToken = Guid.NewGuid();

            string refreshTokenString = refreshToken.ToString();

            var tokenCreated = new Token
            {
                TokenString = refreshTokenString,
                NameIdentifier = givenToken.NameIdentifier,
                FirstName = givenToken.FirstName
            };

            _dbService.RecordToken(tokenCreated);

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });
        }
    }
}
