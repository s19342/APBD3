using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APBD3.Models;

namespace APBD3.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
    }
}
