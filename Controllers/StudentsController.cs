using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using APBD3.DAL;
using APBD3.Models;
using APBD3.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsDbService _dbService;
        public StudentsController(IStudentsDbService dbService)
        {
            _dbService = dbService;
        }
        [HttpGet]
        public IActionResult GetStudent()
        {
            return Ok(_dbService.GetStudents());
        }

        [HttpGet("{index}")]
        public IActionResult GetStudent(string index)
        {
            var listOfEnrollments = new List<Enrollment>(_dbService.GetStudent(index));

            if(listOfEnrollments.Count > 0)
            {
                return Ok(listOfEnrollments[0]);        
            }

            return NotFound("Enrollment not found");
        }
    }
}