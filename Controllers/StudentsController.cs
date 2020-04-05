using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using APBD3.DAL;
using APBD3.Models;
using Microsoft.AspNetCore.Mvc;

namespace APBD3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }
        [HttpGet]
        public IActionResult GetStudent()
        {
            var listOfStudents = new List<Student>();

            using(var connection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19342;Integrated Security=True"))
            {
                using(var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"select s.FirstName, s.LastName, s.BirthDate, st.Name as Studies, e.Semester
                                            from Student s
                                            join Enrollment e on e.IdEnrollment = s.IdEnrollment
                                            join Studies st on st.IdStudy = e.IdStudy;";
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        var st = new Student
                        {
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            DateOfBirth = DateTime.Parse(reader["BirthDate"].ToString()),
                            Studies = reader["Studies"].ToString(),
                            Semester = int.Parse(reader["Semester"].ToString())
                        };
                    listOfStudents.Add(st);
                    }

                }
            }
            return Ok(listOfStudents);
        }

        [HttpGet("{index}")]
        public IActionResult GetStudent(string index)
        {
            var listOfEnrollments = new List<Enrollment>();

            using (var connection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19342;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"select e.IdEnrollment, e.Semester, e.IdStudy, e.StartDate
                                            from Student s
                                            join Enrollment e on e.IdEnrollment = s.IdEnrollment
                                            where s.IndexNumber = @index;";
                    command.Parameters.AddWithValue("@index", index);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var enrollment = new Enrollment
                        {
                            IdEnrollment = int.Parse(reader["IdEnrollment"].ToString()),
                            Semester = int.Parse(reader["Semester"].ToString()),
                            IdStudy = int.Parse(reader["IdStudy"].ToString()),
                            StartDate = DateTime.Parse(reader["StartDate"].ToString())
                        };
                        listOfEnrollments.Add(enrollment);
                    }
                }

                if (listOfEnrollments.Count == 0)
                {
                    return NotFound("The enrollment does not exist");
                }

                return Ok(listOfEnrollments);
            }
           
        }
    }
}