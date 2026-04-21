using System;
using APBD_Cw5.DTOs.Requests;
using APBD_Cw5.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Cw5.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService _service;

        public EnrollmentsController(IStudentsDbService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult EnrollStudent([FromBody] EnrollStudentRequest request)
        {
            try
            {
                var result = _service.EnrollStudent(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("promotions")]
        public IActionResult PromoteStudents([FromBody] PromoteStudentsRequest request)
        {
            try
            {
                var result = _service.PromoteStudents(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}