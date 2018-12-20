using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Microsoft.AspNetCore.Cors;
using ZHWB.Infrastructure;
using ZHWB.Infrastructure.MessageQueue;
using ZHWB.Domain.Models;
using ZHWB.Domain.Repositories;
using ZHWB.Infrastructure.Cache;
namespace ZHWB.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class ValuesController : ControllerBase
    {
        private IUserRepository _userRepository;
        private IRabitMQHandler _rabitmq;
        public ValuesController(IUserRepository userRep,IRabitMQHandler rabitMQ){
            _userRepository=userRep;
            _rabitmq=rabitMQ;
        }
        [HttpGet]
        public ActionResult<string> GetJson(string asd){
            return null;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "val1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
