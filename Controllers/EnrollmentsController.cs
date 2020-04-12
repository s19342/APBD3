using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using APBD3.DTOs.Requests;
using APBD3.DTOs.Responses;
using APBD3.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APBD3.Controllers
{
    [ApiController]
    [Route("api/enrollment")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;
        public EnrollmentsController(IStudentsDbService dbService)
        {
            _dbService = dbService;
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
                return BadRequest();
            }

            return this.StatusCode(201, promoteStudentResponse);
        }
    }
}
