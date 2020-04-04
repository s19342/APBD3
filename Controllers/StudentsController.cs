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
            var listOfStudents = new List<Student>();

            using (var connection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19342;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"select s.FirstName, s.LastName, s.BirthDate, st.Name as Studies, e.Semester
                                            from Student s
                                            join Enrollment e on e.IdEnrollment = s.IdEnrollment
                                            join Studies st on st.IdStudy = e.IdStudy
                                            where s.IndexNumber = @index;";
                    command.Parameters.AddWithValue("@index", index);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
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

                if (listOfStudents.Count == 0)
                {
                    return NotFound("The student does not exist");
                }

                return Ok(listOfStudents);
            }
           
        }
    }
}