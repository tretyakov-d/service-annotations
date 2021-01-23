using Microsoft.AspNetCore.Mvc;

namespace WebApiExample.Api
{
    [ApiController]
    public class MyController
    {
        readonly MyService _service;

        public MyController(MyService service) => _service = service;

        [HttpGet("/")]
        public MyResponse Get() => _service.CreateResponse();
    }
}