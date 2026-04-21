using System.Collections.Generic;
using APBD_Cw5.DTOs.Requests;
using APBD_Cw5.DTOs.Responses;

namespace APBD_Cw5.Services
{
    public interface IStudentsDbService
    {
        IEnumerable<StudentResponse> GetStudents();
        EnrollmentResponse EnrollStudent(EnrollStudentRequest request);
        EnrollmentResponse PromoteStudents(PromoteStudentsRequest request);
    }
}