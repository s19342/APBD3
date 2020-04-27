using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD3.DTOs.Responses
{
    public class LoginAttemptResponse
    {
        public string Hash { get; set; }
        public string Salt { get; set; }
        public string FirstName { get; set; }
    }
}
