using APBD_Cw5.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Cw5.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsDbService _service;

        public StudentsController(IStudentsDbService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok(_service.GetStudents());
        }
    }
}