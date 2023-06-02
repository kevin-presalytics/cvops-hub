using lib.models.db;
using lib.models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using dto = lib.models.dto;
using lib.models.exceptions;
using System.Linq;
using lib.extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;


namespace lib.services
{
    public interface IWorkspaceService
    {
        bool IsWorkspaceViewer(Guid WorkspaceId, User user);
        bool IsWorkspaceOwner(Guid WorkspaceId, User user);
        bool IsWorkspaceEditor(Guid WorkspaceId, User user);
        Task<dto.UserWorkspace> GetWorkspace(Guid WorkspaceId, User user);
        Task<Workspace> GetWorkspace(Guid WorkspaceId);
        Task<List<dto.UserWorkspace>> GetWorkspaces(User user);
        Task<dto.UserWorkspace> CreateWorkspace(dto.NewWorkspaceRequestBody body, User user);
        Task<dto.UserWorkspace> UpdateWorkspace(dto.UpdateWorkspaceRequestBody body, User user);
        Task DeleteWorkspace(Guid WorkspaceId);
        Task<PaginatedList<dto.Device>> GetWorkspaceDevices(Guid WorkspaceId, int page, int pageSize);
        Task<dto.NewDevice> CreateWorkspaceDevice(Workspace workspace);
        Task<PaginatedList<dto.User>> GetWorkspaceUsers(Workspace workspace, int page, int pageSize);
        Task<dto.User> AddWorkspaceUser(Workspace workspace, dto.NewUserRequestBody body);
        Task RemoveWorkspaceUser(Guid WorkspaceId, Guid UserId);
    }
    public class WorkspaceService : IWorkspaceService
    {
        private CvopsDbContext _context;
        private IUserService _userService;
        private IDeviceService _deviceService;
        private ILogger _logger;
        public WorkspaceService(
            CvopsDbContext context,
            IUserService userService,
            IDeviceService deviceService,
            ILogger logger
        )
        {
            _context = context;
            _userService = userService;
            _deviceService = deviceService;
            _logger = logger;
        }

        private static WorkspaceUserRole[] _viewerRoles = new WorkspaceUserRole[] {
            WorkspaceUserRole.Owner, 
            WorkspaceUserRole.Editor,
            WorkspaceUserRole.Viewer 
        };

        private static WorkspaceUserRole[] _editorRoles = new WorkspaceUserRole[] {
            WorkspaceUserRole.Owner, 
            WorkspaceUserRole.Editor,
        };


        public async Task<Workspace> GetWorkspace(Guid WorkspaceId)
        {
            Workspace? workspace = await _context.Workspaces.FindAsync(WorkspaceId);
            if (workspace == null) throw new WorkspaceNotFoundException();
            return workspace;
        }

        public async Task<dto.Workspace> UpdateWorkspace(dto.Workspace workspace)
        {
            Workspace _workspace = await GetWorkspace(workspace.Id);
            _workspace.Name = workspace.Name;
            _workspace.Description = workspace.Description;
            _context.Workspaces.Update(_workspace);
            await _context.SaveChangesAsync();
            return workspace;
        }

        public async Task<dto.User> AddWorkspaceUser(Workspace workspace, dto.NewUserRequestBody body)
        {
            User user = await _userService.InviteUser(workspace, body.Email, body.Role);
            return user.ToDto();
        }

        public async Task<List<dto.UserWorkspace>> GetWorkspaces(User user)
        {
            List<Workspace> workspaces = await _context.Workspaces
                .Where(w => w.WorkspaceUsers.Any(wu => wu.User.Id == user.Id))
                .ToListAsync();
            return workspaces.Select(w => w.ToUserWorkspace(user)).ToList();
        }

        public bool IsWorkspaceViewer(Guid WorkspaceId, User user)
        {
            return _context.WorkspaceUsers.Any(wu => wu.WorkspaceId == WorkspaceId && wu.UserId == user.Id && _viewerRoles.Contains(wu.Role));
        }

        public bool IsWorkspaceEditor(Guid WorkspaceId, User user)
        {
            return _context.WorkspaceUsers.Any(wu => wu.WorkspaceId == WorkspaceId && wu.UserId == user.Id && _editorRoles.Contains(wu.Role));
        }

        public bool IsWorkspaceOwner(Guid WorkspaceId, User user)
        {
            return _context.WorkspaceUsers.Any(wu => wu.WorkspaceId == WorkspaceId && wu.UserId == user.Id && wu.Role == WorkspaceUserRole.Owner);
        }

        public async Task DeleteWorkspace(Guid WorkspaceId)
        {
            Workspace workspace = await GetWorkspace(WorkspaceId);
            if (workspace == null) throw new WorkspaceNotFoundException();
            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();
        }

        public Task<dto.UserWorkspace> GetWorkspace(Guid WorkspaceId, User user)
        {
            #pragma warning disable CS8600
            Workspace workspace = _context.Workspaces
                .Include(w => w.WorkspaceUsers)
                .FirstOrDefault(w => w.Id == WorkspaceId);
            #pragma warning restore CS8600
            if (workspace == null) throw new WorkspaceNotFoundException();
            if (!IsWorkspaceViewer(WorkspaceId, user)) throw new Exception("User is not a member of this workspace");
            return Task.FromResult(workspace.ToUserWorkspace(user));
        }

        public async Task<dto.UserWorkspace> CreateWorkspace(dto.NewWorkspaceRequestBody body, User user)
        {
            Workspace newWorkspace = new Workspace
            {
                Name = body.Name,
                Description = body.Description,
                WorkspaceUsers = new List<WorkspaceUser> {
                    new WorkspaceUser {
                        Role = WorkspaceUserRole.Owner,
                        User = user
                    }
                }
            };
            _context.Workspaces.Add(newWorkspace);
            await _context.SaveChangesAsync();
            return newWorkspace.ToUserWorkspace(user);
        }

        public async Task<dto.UserWorkspace> UpdateWorkspace(dto.UpdateWorkspaceRequestBody body, User user)
        {
            #pragma warning disable CS8600
            Workspace workspace = await _context.Workspaces.FindAsync(body.Id);
            #pragma warning restore CS8600
            if (workspace == null) throw new WorkspaceNotFoundException();
            if (body.Name != null && body.Name != string.Empty) workspace.Name = body.Name;
            if (body.Description != null && body.Description != string.Empty) workspace.Description = body.Description;
            _context.Workspaces.Update(workspace);
            if (body.IsDefault) {
                user.DefaultWorkspaceId = workspace.Id;
                _context.Users.Update(user);
            }
            await _context.SaveChangesAsync();
            return workspace.ToUserWorkspace(user);
        }

        public async Task<PaginatedList<dto.Device>> GetWorkspaceDevices(Guid WorkspaceId, int page, int pageSize)
        {
            #pragma warning disable CS8600
            PaginatedList<Device> devices = await _context.Devices
                .Where(d => d.WorkspaceId == WorkspaceId)
                .OrderBy(d => d.Name)
                .PaginateAsync(page, pageSize);
            #pragma warning restore CS8600
            return new PaginatedList<dto.Device>(
                devices.Select(d => d.ToDto()).ToList(),
                devices.Count,
                devices.PageIndex,
                devices.TotalPages
            );
        }

        public async Task<dto.NewDevice> CreateWorkspaceDevice(Workspace workspace)
        {
            return await _deviceService.CreateNewDevice(workspace);   
        }

        public async Task<PaginatedList<dto.User>> GetWorkspaceUsers(Workspace workspace, int page, int pageSize)
        {
             #pragma warning disable CS8600
            PaginatedList<WorkspaceUser> devices = await _context.WorkspaceUsers
                .Include(wu => wu.User)
                .Where(wu => wu.WorkspaceId == workspace.Id)
                .OrderBy(wu => wu.User.Email)
                .PaginateAsync(page, pageSize);
            #pragma warning restore CS8600
            return new PaginatedList<dto.User>(
                devices.Select(u => u.User.ToDto()).ToList(),
                devices.Count,
                devices.PageIndex,
                devices.TotalPages
            );
        }

        public async Task RemoveWorkspaceUser(Guid WorkspaceId, Guid UserId)
        {
            #pragma warning disable CS8600
            WorkspaceUser workspaceUser = await _context.WorkspaceUsers
                .FirstOrDefaultAsync(wu => wu.WorkspaceId == WorkspaceId && wu.UserId == UserId);
            #pragma warning restore CS8600
            if (workspaceUser == null) throw new Exception("User is not a member of this workspace");
            _context.WorkspaceUsers.Remove(workspaceUser);
            await _context.SaveChangesAsync();
        }
    }
}