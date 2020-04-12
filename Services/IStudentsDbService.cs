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

        public PromoteStudentResponse PromoteStudents(PromoteStudentRequest promoteStudentRequest);//return boolean or something
    }
}
