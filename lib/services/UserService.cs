using System;
using System.Threading.Tasks;
using lib.models;
using lib.models.db;
using Serilog;
using lib.services.auth;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace lib.services
{
    public interface IUserService {
        Task<User> GetUser(Guid userId);

        Task<User> CreateUser(string jwtToken);
        Task<User> CreateUser(string email, string jwtSubject);
        Task<User> GetOrCreateUser(string jwtToken);
    }

    public class UserService : IUserService {
        private readonly ILogger _logger;
        private readonly CvopsDbContext _dbContext;
        private readonly IUserJwtTokenReader _userJwtTokenReader;

        public UserService(ILogger logger, CvopsDbContext dbContext, IUserJwtTokenReader userJwtTokenReader) {
            _logger = logger;
            _dbContext = dbContext;
            _userJwtTokenReader = userJwtTokenReader;
        }

        public async Task<User> GetUser(Guid userId) {
            User? user = await _dbContext.Users.FindAsync(userId);
            if (user == null) throw new Exception($"user not found: {userId}");
            return user;
        }

        public async Task<User> CreateUser(string email, string jwtSubject) {
            User user = new User() { 
                Email = email,
                JwtSubject = jwtSubject                
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> CreateUser(string jwtToken) {
            string email = await _userJwtTokenReader.GetEmailFromJwtAsync(jwtToken);
            string jwtSubject = await _userJwtTokenReader.GetJwtSubjectFromJwtAsync(jwtToken);
            User user = await CreateUser(email, jwtSubject);
            return user;
        }

        public async Task<User> GetOrCreateUser(string jwtToken) {
            string jwtSubject = await _userJwtTokenReader.GetJwtSubjectFromJwtAsync(jwtToken);
            User? user = await _dbContext.Users.FirstOrDefaultAsync(i => i.JwtSubject == jwtSubject);
            if (user == null) {
                user = await CreateUser(jwtToken);
            }
            return user;
        }
    }
}