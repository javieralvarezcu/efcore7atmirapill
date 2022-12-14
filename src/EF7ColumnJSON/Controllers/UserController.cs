using ConsoleApp1.DataAccess;
using EF7ColumnJSON.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EF7JSONColumns.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private ApplicationDbContext _dbContext;

        public UserController(ILogger<UserController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet(Name = "GetUsers")]
        public async Task<IEnumerable<User>?> Get()
        {
            return await _dbContext.Users
                .Include(a => a.Posts)
                .ThenInclude(post => post.Comments).ToListAsync();
        }

        [HttpGet("{id}", Name = "GetById")]
        public async Task<User?> Get(int id)
        {
            return await _dbContext.Users
                .Include(a => a.Posts)
                .ThenInclude(post => post.Comments)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        [HttpGet("City/{city}", Name = "GetByCity")]
        public async Task<User?> Get(string city)
        {
            return await _dbContext.Users
                .Include(a => a.Posts)
                .ThenInclude(post => post.Comments)
                .FirstOrDefaultAsync(a => a.Address.City == city);
        }
    }
}