using System;

namespace APBD_Cw5.DTOs.Responses
{
    public class StudentResponse
    {
        public string IndexNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Studies { get; set; }
        public int Semester { get; set; }
    }
}