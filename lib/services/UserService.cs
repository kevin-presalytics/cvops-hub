using System;
using System.Threading.Tasks;
using lib.models;
using lib.models.db;
using Serilog;
using lib.services.auth;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using lib.models.exceptions;

namespace lib.services
{
    public interface IUserService : IDisposable
    {
        Task<User> GetUser(Guid userId);
        Task<User> CreateUser(string jwtToken);
        Task<User> CreateUser(string email, string jwtSubject);
        Task<User> GetOrCreateUser(string jwtToken);
        Task<User> InviteUser(Workspace workspace, string email, WorkspaceUserRole role);
        Task ActivateUser(User user, string jwtToken);
    }

    public class UserService : IUserService {
        private readonly ILogger _logger;
        private readonly CvopsDbContext _context;
        private readonly IUserJwtTokenReader _userJwtTokenReader;
        private readonly IInviteUserService _inviteUserService;

        public UserService(
            ILogger logger, 
            IDbContextFactory<CvopsDbContext> dbContextFactory, 
            IUserJwtTokenReader userJwtTokenReader, 
            IInviteUserService inviteUserService
        ) {
            _logger = logger;
            _context = dbContextFactory.CreateDbContext();
            _userJwtTokenReader = userJwtTokenReader;
            _inviteUserService = inviteUserService;

        }

        public async Task<User> GetUser(Guid userId) {
            User? user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception($"user not found: {userId}");
            return user;
        }

        public async Task<User> CreateUser(string email, string jwtSubject) {
            User user = new User() { 
                Email = email,
                JwtSubject = jwtSubject,
                Status = UserStatus.Active,
                IsEmailVerified = true,       
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            await CreateDefaultWorkspace(user);
            return user;
        }

        public async Task<User> CreateUser(string jwtToken) {
            List<string> emails = await _userJwtTokenReader.GetEmailsFromJwtAsync(jwtToken);
            string jwtSubject = await _userJwtTokenReader.GetJwtSubjectFromJwtAsync(jwtToken);
            var email = emails.FirstOrDefault();
            if (email == null) throw new Exception("no email found in jwt token");
            User user = await CreateUser(email, jwtSubject);
            return user;
        }

        public async Task<User> GetOrCreateUser(string jwtToken) {
            string jwtSubject = await _userJwtTokenReader.GetJwtSubjectFromJwtAsync(jwtToken);
            User? user = await _context.Users.FirstOrDefaultAsync(i => i.JwtSubject == jwtSubject);
            if (user == null) {
                List<string> emails = await _userJwtTokenReader.GetEmailsFromJwtAsync(jwtToken);
                foreach (string email in emails) {
                    user = await _context.Users.FirstOrDefaultAsync(i => i.Email == email);
                    if (user != null) break;
                }
                if (user == null) {
                    user = await CreateUser(jwtToken);
                }
            }
            return user;
        }

        private async Task<Workspace> CreateDefaultWorkspace(User user) {
            WorkspaceUser wu = new WorkspaceUser() { 
                User = user,
                WorkspaceUserRole = WorkspaceUserRole.Owner
            };
            Workspace workspace = new Workspace() { 
                Name = "My Workspace",
                Description = $"CVOps Workspace for ${user.Email}'s test and personal projects",
                WorkspaceUsers = new List<WorkspaceUser>() { wu }                    
            };
            wu.Workspace = workspace;
            await _context.WorkspaceUsers.AddAsync(wu);
            await _context.Workspaces.AddAsync(workspace);
            #pragma warning disable CS8600
            User userToUpdate = await _context.Users.FindAsync(user.Id); 
            #pragma warning restore CS8600
            if (userToUpdate == null) throw new UserNotFoundException();
            userToUpdate.DefaultWorkspaceId = workspace.Id;
            _context.Users.Update(userToUpdate);
            await _context.SaveChangesAsync();
            return workspace;
        }

        private async Task<Workspace> GetDefaultWorkspace(User user) {
            Workspace? defaultWorkspace = await _context.Workspaces.FirstOrDefaultAsync(i => i.Id == user.DefaultWorkspaceId);
            if (defaultWorkspace == null) throw new WorkspaceNotFoundException("user does not have a default workspace");
            return defaultWorkspace;
        }

        public async Task<User> InviteUser(Workspace workspace, string email, WorkspaceUserRole role = WorkspaceUserRole.Viewer) {
            if (workspace == null) throw new ArgumentNullException(nameof(workspace));
            if (email == null) throw new ArgumentNullException(nameof(email));
            User? user = await _context.Users.FirstOrDefaultAsync(i => i.Email == email);
            if (user == null) {
                bool success = await _inviteUserService.SendInvite(email);
                if (!success) throw new InvalidEmailException($"A mailbox with Email Address '{email}' does not exist.");
                user = new User() { 
                    Email = email,
                    JwtSubject = "",
                    DefaultWorkspaceId = workspace.Id,
                    Status = UserStatus.Pending
                };
                _context.Users.Add(user);
            }
            WorkspaceUser? workspaceUser = await _context.WorkspaceUsers.FirstOrDefaultAsync(i => i.WorkspaceId == workspace.Id && i.UserId == user.Id);
            if (workspaceUser == null) {
                workspaceUser = new WorkspaceUser() { 
                    User = user,
                    Workspace = workspace,
                    WorkspaceUserRole = role
                };
                _context.WorkspaceUsers.Add(workspaceUser);
                workspace.WorkspaceUsers.Add(workspaceUser);
                _context.Workspaces.Update(workspace);
                
            } else {
                throw new Exception($"{email} is already a member of {workspace.Name}");
            }
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task ActivateUser(User user, string jwtToken) {
            if (user.Status == UserStatus.Active) return;
            user.Status = UserStatus.Active;
            user.JwtSubject = await _userJwtTokenReader.GetJwtSubjectFromJwtAsync(jwtToken);
            if (user.Email == null) {
                user.Email = (await _userJwtTokenReader.GetEmailsFromJwtAsync(jwtToken))[0];
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        void IDisposable.Dispose() {
            _context.Dispose();
        }
    }
}
