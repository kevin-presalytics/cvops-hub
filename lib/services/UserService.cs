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
    public interface IUserService {
        Task<User> GetUser(Guid userId);
        Task<User> CreateUser(string jwtToken);
        Task<User> CreateUser(string email, string jwtSubject);
        Task<User> GetOrCreateUser(string jwtToken);
        Task<Workspace> GetOrCreateDefaultWorkspace(User user);
        Task<User> InviteUser(Workspace workspace, string email, WorkspaceUserRole role);

        Task ActivateUser(User user, string jwtToken);
    }

    public class UserService : IUserService {
        private readonly ILogger _logger;
        private readonly CvopsDbContext _context;
        private readonly IUserJwtTokenReader _userJwtTokenReader;
        private readonly IInviteUserService _inviteUserService;

        public UserService(ILogger logger, CvopsDbContext dbContext, IUserJwtTokenReader userJwtTokenReader, IInviteUserService inviteUserService) {
            _logger = logger;
            _context = dbContext;
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
                JwtSubject = jwtSubject                
            };
            await _context.Users.AddAsync(user);
            Workspace Workspace = await CreateDefaultWorkspace(user);
            WorkspaceUser wu = new WorkspaceUser() { 
                User = user,
                Workspace = Workspace,
                Role = WorkspaceUserRole.Owner
            };
            _context.WorkspaceUsers.Add(wu);
            Workspace.WorkspaceUsers = new List<WorkspaceUser>() { wu };
            _context.Workspaces.Update(Workspace);
            await _context.SaveChangesAsync();
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
            Workspace Workspace = new Workspace() { 
                Name = "My Workspace",
                WorkspaceUsers = new List<WorkspaceUser>() { 
                    new WorkspaceUser() { 
                        User = user,
                        Role = WorkspaceUserRole.Owner
                    }},
            };
            await _context.Workspaces.AddAsync(Workspace);
            user.DefaultWorkspace = Workspace;
            _context.Users.Update(user);
            // note must save changes on a later step
            return Workspace;
        }

        public async Task<Workspace> GetOrCreateDefaultWorkspace(User user) {
            if (user.DefaultWorkspace != null) return user.DefaultWorkspace;
            Workspace? defaultWorkspace = await _context.Workspaces.FirstOrDefaultAsync(i => i.Id == user.DefaultWorkspaceId);
            if (defaultWorkspace == null) {
                defaultWorkspace = await CreateDefaultWorkspace(user);
            }
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
                    DefaultWorkspace = workspace,
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
                    Role = role
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
    }
}
