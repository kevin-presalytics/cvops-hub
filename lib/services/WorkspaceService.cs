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
using System.Text.Json;
using lib.models.mqtt;
using System.Threading.Channels;

namespace lib.services
{
    public interface IWorkspaceService : IDisposable
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
        Task<PaginatedList<dto.Device>> GetWorkspaceDevices(Guid WorkspaceId, int page, int pageSize, string? search);
        Task<dto.NewDevice> CreateWorkspaceDevice(Workspace workspace);
        Task<PaginatedList<dto.User>> GetWorkspaceUsers(Workspace workspace, int page, int pageSize);
        Task<dto.User> AddWorkspaceUser(Workspace workspace, dto.NewUserRequestBody body);
        Task RemoveWorkspaceUser(Guid WorkspaceId, Guid UserId);
        Task<dto.WorkspaceDetails> GetDetails(Guid WorkspaceId);
    }
    public class WorkspaceService : IWorkspaceService
    {
        private CvopsDbContext _context;
        private IUserService _userService;
        private IDeviceService _deviceService;
        private ILogger _logger;
        private ChannelWriter<MqttPublishMessage> _publishChannel;
        public WorkspaceService(
            IDbContextFactory<CvopsDbContext> contextFactory,
            IUserService userService,
            IDeviceService deviceService,
            ILogger logger, 
            ChannelWriter<MqttPublishMessage> publishChannel
        )
        {
            _context = contextFactory.CreateDbContext();
            _userService = userService;
            _deviceService = deviceService;
            _logger = logger;
            _publishChannel = publishChannel;
        }

        private static readonly WorkspaceUserRole[] _viewerRoles = new WorkspaceUserRole[] {
            WorkspaceUserRole.Owner, 
            WorkspaceUserRole.Editor,
            WorkspaceUserRole.Viewer 
        };

        private static readonly WorkspaceUserRole[] _editorRoles = new WorkspaceUserRole[] {
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
            return new dto.User() {
                Id = user.Id,
                Email = user.Email,
                Role = body.Role
            };
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
            return _context.WorkspaceUsers.Any(wu => wu.WorkspaceId == WorkspaceId && wu.UserId == user.Id && _viewerRoles.Contains(wu.WorkspaceUserRole));
        }

        public bool IsWorkspaceEditor(Guid WorkspaceId, User user)
        {
            return _context.WorkspaceUsers.Any(wu => wu.WorkspaceId == WorkspaceId && wu.UserId == user.Id && _editorRoles.Contains(wu.WorkspaceUserRole));
        }

        public bool IsWorkspaceOwner(Guid WorkspaceId, User user)
        {
            return _context.WorkspaceUsers.Any(wu => wu.WorkspaceId == WorkspaceId && wu.UserId == user.Id && wu.WorkspaceUserRole == WorkspaceUserRole.Owner);
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
                        WorkspaceUserRole = WorkspaceUserRole.Owner,
                        User = user
                    }
                }
            };
            _context.Workspaces.Add(newWorkspace);
            await _context.SaveChangesAsync();
            var createWorkspaceEvent = new PlatformEvent() {
                EventType = PlatformEventTypes.WorkspaceCreated,
                EventData = JsonSerializer.SerializeToDocument(newWorkspace),
                UserId = user.Id,
                WorkspaceId = newWorkspace.Id
            };
            await _publishChannel.WriteAsync(new MqttPublishMessage {
                Topic = $"events/workspace/{newWorkspace.Id}",
                Payload = createWorkspaceEvent
            });
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

        public async Task<PaginatedList<dto.Device>> GetWorkspaceDevices(Guid WorkspaceId, int page, int pageSize, string? search = null)
        {
            #pragma warning disable CS8600
            IQueryable<Device> query = _context.Devices.Where(d => d.WorkspaceId == WorkspaceId);
            if (search != null) {
                // TODO: Research whether this generates via the FromSqlInterpolated method
                // Need to AVOID SQL injection here with the FromSQLRaw method
                query = query.Where(d => 
                    EF.Functions.ILike(d.Name, $"%{search}%") || 
                    EF.Functions.ILike(d.Description, $"%{search}%")
                );
            }

            PaginatedList<Device> devices = await query.OrderBy(d => d.Name).PaginateAsync(page, pageSize);
            #pragma warning restore CS8600
            return new PaginatedList<dto.Device>(
                devices.Select(d => d.ToDto()).ToList(),
                devices.TotalCount,
                devices.PageIndex,
                devices.PageSize
            );
        }

        public async Task<dto.NewDevice> CreateWorkspaceDevice(Workspace workspace)
        {
            return await _deviceService.CreateNewDevice(workspace);   
        }

        public async Task<PaginatedList<dto.User>> GetWorkspaceUsers(Workspace workspace, int page, int pageSize)
        {
            List<dto.User> users = workspace.GetUserDTOs().OrderBy(u => u.Email).ToList();
            return await users.AsQueryable().PaginateAsync(page, pageSize);
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

        void IDisposable.Dispose()
        {
            _context.Dispose();
        }

        public async Task<dto.WorkspaceDetails> GetDetails(Guid workspaceId)
        {
            #pragma warning disable CS8600
            Workspace workspace = await _context.Workspaces
                .Include(w => w.WorkspaceUsers)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);
            #pragma warning restore CS8600
            if (workspace == null) throw new WorkspaceNotFoundException();
            return new dto.WorkspaceDetails() {
                Id = workspace.Id,
                Name = workspace.Name,
                Description = workspace.Description,
                Users = workspace.GetUserDTOs(),
                Devices = workspace.Devices.Select(d => d.ToDto()).ToList(),
                Deployments = workspace.Deployments.Select(d => d.ToDto()).ToList()
            };
        }
    }
}