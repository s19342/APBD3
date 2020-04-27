using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APBD3.DTOs.Requests;
using APBD3.DTOs.Responses;
using APBD3.Models;

namespace APBD3.Services
{
    public interface IStudentsDbService
    {
        public IEnumerable<Student> GetStudents();
        public List<Enrollment> GetStudent(string index);

        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);

        public PromoteStudentResponse PromoteStudents(PromoteStudentRequest promoteStudentRequest);

        public Student GetStudentByIndex(string index);

        public void SaveLogData(string method, string path, string body, string query);

        public LoginAttemptResponse checkLogin(string login);

        public void RecordToken(Token token);

        public Token validateToken(string requestToken);
    }
}
