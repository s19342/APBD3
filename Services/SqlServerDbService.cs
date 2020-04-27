using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using APBD3.DTOs.Requests;
using APBD3.DTOs.Responses;
using APBD3.Models;
namespace APBD3.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        public PromoteStudentResponse PromoteStudents(PromoteStudentRequest promoteStudentRequest)
        {
            using (var connection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19342;Integrated Security=True"))
            {
                connection.Open();
                var tran = connection.BeginTransaction();

                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.Transaction = tran;

                    command.CommandText = @"select * from 
                                            Enrollment e inner join Studies s 
                                            On e.IdStudy=s.IdStudy 
                                            And s.Name=@Name                                            
                                            AND e.Semester=@Semester";
                    command.Parameters.AddWithValue("@Name", promoteStudentRequest.Studies);
                    command.Parameters.AddWithValue("@Semester", promoteStudentRequest.Semester);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            reader.Close();
                            tran.Rollback();
                            return null;
                        }
                    }
                }

                using (var command1 = new SqlCommand("promoteStudent", connection))
                {
                    command1.Connection = connection;
                    command1.Transaction = tran;

                    command1.CommandType = System.Data.CommandType.StoredProcedure;

                    command1.CommandText = "promoteStudent";

                    command1.Parameters.AddWithValue("@studiesGiven", promoteStudentRequest.Studies);
                    command1.Parameters.AddWithValue("@semesterGiven", promoteStudentRequest.Semester);

                    using(var reader = command1.ExecuteReader())
                    {
                        reader.Read();

                        PromoteStudentResponse promoteStudentResponse = new PromoteStudentResponse
                        {
                            IdEnrollment = int.Parse(reader["IdEnrollment"].ToString()),
                            Semester = int.Parse(reader["Semester"].ToString()),
                            IdStudy = int.Parse(reader["IdStudy"].ToString()),
                            StartDate = DateTime.Parse(reader["StartDate"].ToString())
                        };
                        reader.Close();

                        tran.Commit();
                        return promoteStudentResponse;

                    }                                         
                }
            }
        }

        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            using (var connection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19342;Integrated Security=True"))
            {
                connection.Open();
                int idStudy = 1;
                int enrollmentID = 1;
                
                var tran = connection.BeginTransaction();

                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.Transaction = tran;
                    command.CommandText = @"Select * from Studies s 
                                            where s.Name=@Name";
                    command.Parameters.AddWithValue("@Name", request.Studies);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            reader.Close();
                            tran.Rollback();
                            connection.Close();
                            return null;
                        }
                        idStudy = int.Parse(reader["IdStudy"].ToString());
                    }
                }

                using (var command1 = new SqlCommand())
                {
                    command1.Connection = connection;
                    command1.Transaction = tran;
                    command1.CommandText = @"Select * from Enrollment e 
                                            where e.IdStudy=@StudyID AND e.Semester=1";
                    command1.Parameters.AddWithValue("@StudyID", idStudy);

                    using (var reader1 = command1.ExecuteReader())
                    {
                        if (reader1.Read())
                        {
                            enrollmentID = int.Parse(reader1["IdEnrollment"].ToString());
                        }
                        else
                        {
                            reader1.Close();

                            using (var command2 = new SqlCommand())
                            {
                                command2.Connection = connection;
                                command2.Transaction = tran;
                                command2.CommandText = "Select MAX(e.IdEnrollment) as IdEnrollment From Enrollment e";

                                using (var reader2 = command2.ExecuteReader())
                                {
                                    if (reader2.Read())
                                    {
                                        enrollmentID = (int.Parse(reader2["IdEnrollment"].ToString())) + 1;
                                    }
                                    reader2.Close();
                                }

                                using (var command3 = new SqlCommand())
                                {
                                    command3.Connection = connection;
                                    command3.Transaction = tran;
                                    command3.CommandText = @"Insert into Enrollment(IdEnrollment, Semester, IdStudy, StartDate)
                                                Values(@IdEnrollment, @Semester, @IdStudy, @StartDate)";
                                    command3.Parameters.AddWithValue("@IdEnrollment", enrollmentID);
                                    command3.Parameters.AddWithValue("@Semester", 1);
                                    command3.Parameters.AddWithValue("@IdStudy", idStudy);
                                    command3.Parameters.AddWithValue("@StartDate", DateTime.Now.ToString("yyyy-MM-dd"));

                                    command3.ExecuteNonQuery();
                                }

                            }
                        }
                    }
                }

                using (var command4 = new SqlCommand())
                {
                    command4.Connection = connection;
                    command4.Transaction = tran;
                    command4.CommandText = @"Select * From Student s
                                            Where s.IndexNumber=@IndexNumber";
                    command4.Parameters.AddWithValue("@IndexNumber", request.IndexNumber);

                    using (var reader3 = command4.ExecuteReader())
                    {
                        if (reader3.Read())
                        {
                            reader3.Close();
                            tran.Rollback();
                            connection.Close();
                            return null;
                        }
                    }
                }

                using (var command5 = new SqlCommand())
                {
                    command5.Connection = connection;
                    command5.Transaction = tran;
                    command5.CommandText = @"Insert into Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment)
                                            Values(@IndexNumber1, @FirstName, @LastName, @BirthDate, @IdEnrollment1)";
                    command5.Parameters.AddWithValue("IndexNumber1", request.IndexNumber);
                    command5.Parameters.AddWithValue("@FirstName", request.FirstName);
                    command5.Parameters.AddWithValue("@LastName", request.LastName);
                    command5.Parameters.AddWithValue("@BirthDate", DateTime.ParseExact(request.BirthDate, "dd.MM.yyyy", null));
                    command5.Parameters.AddWithValue("@IdEnrollment1", enrollmentID);

                    command5.ExecuteNonQuery();
                }

                tran.Commit();

                using (var command6 = new SqlCommand())
                {
                    command6.Connection = connection;
                    command6.CommandText = @"Select * From Enrollment e
                                            Where e.IdEnrollment=@IdEnrollment2";
                    command6.Parameters.AddWithValue("@IdEnrollment2", enrollmentID);

                    var enrollStudentResponse = new EnrollStudentResponse();

                    using (var reader4 = command6.ExecuteReader())
                    {
                        while (reader4.Read())
                        {
                            enrollStudentResponse.IdEnrollment = int.Parse(reader4["IdEnrollment"].ToString());
                            enrollStudentResponse.Semester = int.Parse(reader4["Semester"].ToString());
                            enrollStudentResponse.IdStudy = int.Parse(reader4["IdStudy"].ToString());
                            enrollStudentResponse.StartDate = DateTime.Parse(reader4["StartDate"].ToString());
                        }
                    }

                    return enrollStudentResponse;
                }
            }
        }

        public List<Enrollment> GetStudent(string index)
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
                    using (var reader = command.ExecuteReader())
                    {
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
                }

                return listOfEnrollments;
            }
        }

        public IEnumerable<Student> GetStudents()
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
                                            join Studies st on st.IdStudy = e.IdStudy;";
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
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

                }
            }
            return listOfStudents;
        }

        public Student GetStudentByIndex(string index)
        {
            Student st = null;

            using (var connection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19342;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"select s.FirstName, s.LastName, s.BirthDate, st.Name as Studies, e.Semester
                                            from Student s
                                            join Enrollment e on e.IdEnrollment = s.IdEnrollment
                                            join Studies st on st.IdStudy = e.IdStudy
                                            where s.IndexNumber=@index;";
                    command.Parameters.AddWithValue("@index", index);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            st = new Student
                            {
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                DateOfBirth = DateTime.Parse(reader["BirthDate"].ToString()),
                                Studies = reader["Studies"].ToString(),
                                Semester = int.Parse(reader["Semester"].ToString())
                            };
                        }
                    }
                }
            }

            return st;
        }

        public void SaveLogData(string method, string path, string body, string query)
        {
            string filePath = "requestsLog.txt";
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine($"HTTP Method: {method}");
                sw.WriteLine($"Endpoint path: {path}");
                sw.WriteLine($"Body of request: {body}");
                sw.WriteLine($"Query string: {query}");
                sw.WriteLine();
            }
        }

        public LoginAttemptResponse checkLogin(string login)
        {
            LoginAttemptResponse loginAttemptResponse = null;

            using (var connection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19342;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"select s.StdPassword, s.Salt, s.FirstName
                                            from Student s
                                            where s.IndexNumber = @login;";
                    command.Parameters.AddWithValue("@login", login);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            loginAttemptResponse = new LoginAttemptResponse
                            {
                                Hash = reader["StdPassword"].ToString(),
                                Salt = reader["Salt"].ToString(),
                                FirstName = reader["FirstName"].ToString()
                            };                      
                        }
                    }
                }
            }

            return loginAttemptResponse;
        }

        public void RecordToken(Token token)
        {
            using (var connection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19342;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"INSERT INTO RefreshToken(TokenId, UserLogin, FirstName)
                                            VALUES(@tokenId, @userLogin, @firstName);";
                    command.Parameters.AddWithValue("@tokenId", token.TokenString);
                    command.Parameters.AddWithValue("@userLogin", token.NameIdentifier);
                    command.Parameters.AddWithValue("@firstName", token.FirstName);
                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public Token validateToken(string requestToken)
        {
            using (var connection = new SqlConnection(@"Data Source=db-mssql;Initial Catalog=s19342;Integrated Security=True"))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"Select r.TokenId, r.UserLogin, r.FirstName
                                            from RefreshToken r 
                                            where r.TokenId=@tokenGiven;";
                    command.Parameters.AddWithValue("@tokenGiven", requestToken);

                    connection.Open();

                    var reader = command.ExecuteReader();

                    if(!reader.Read())
                    {
                        return null;
                    }

                    var token = new Token
                    {
                        TokenString = reader["TokenId"].ToString(),
                        NameIdentifier = reader["UserLogin"].ToString(),
                        FirstName = reader["FirstName"].ToString()
                    };

                    return token;
                }
            }
        }

    }
}
