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
using System.Security.Cryptography;
using Newtonsoft.Json;
using ZHWB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
namespace ZHWB.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class UsersController : ControllerBase
    {
        IUserRepository _repository;
        IDataCaches<User> _dataCaches;
        IConfiguration Configuration;
        public UsersController(IUserRepository repository, IDataCaches<User> dataCaches,IConfiguration configuration)
        {
            Configuration=configuration;
            _repository = repository;
            _dataCaches = dataCaches;
        }
    }
}