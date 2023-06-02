using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using lib.models;
using lib.models.db;
using dto = lib.models.dto;
using lib.middleware;
using System.Net.Mime;
using lib.services;
using lib.models.exceptions;
using Serilog;

namespace api.controllers
{

    // DeviceController class for api to create api endpoints for Workspaces for lib/models/db/Device.cs model
    // Contains Get Endpoint to list a users Workspaces
    // Contains Get Endpoint to get a single Workspaces details
    // Contains Post Endpoint to create a new device
    // Contains Put Endpoint to update a device
    // contains Delete Endpoint to delete a device
    // Contains a /authorize endpoint to authorize a device connection over mqt
    [Authorize]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("[controller]")]
    public class WorkspacesController : ControllerBase
    {
        IWorkspaceService _workspaceService;
        ILogger _logger;
        public WorkspacesController(IWorkspaceService workspaceService, ILogger logger)
        {
            _workspaceService = workspaceService;
            _logger = logger;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<List<dto.UserWorkspace>>> List()
        {
            try
            {
                #pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                #pragma warning restore CS8600
                if (userFeature == null || userFeature.User == null)
                    return Unauthorized();
                List<dto.UserWorkspace> workspaces = await _workspaceService.GetWorkspaces(userFeature.User);
                return Ok(workspaces);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error listing workspaces");
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<dto.UserWorkspace>> Get(Guid id)
        {
            try
            {
                #pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                #pragma warning restore CS8600
                if (userFeature == null || userFeature.User == null || !_workspaceService.IsWorkspaceViewer(id, userFeature.User))
                    return Unauthorized();
                dto.UserWorkspace workspace = await _workspaceService.GetWorkspace(id, userFeature.User);
                return Ok(workspace);
            }
            catch (WorkspaceNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting workspace");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<dto.UserWorkspace>> Post(
            [FromBody] dto.NewWorkspaceRequestBody body
        )
        {
            try
            {
                #pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                #pragma warning restore CS8600
                if (userFeature == null || userFeature.User == null)
                    return Unauthorized();
                dto.UserWorkspace workspace = await _workspaceService.CreateWorkspace(body, userFeature.User);
                return CreatedAtAction(nameof(Get), new { id = workspace.Id }, workspace);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating workspace");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<dto.UserWorkspace>> Put(
            Guid id,
            [FromBody] dto.UpdateWorkspaceRequestBody body
        )
        {
            try
            {
                #pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                #pragma warning restore CS8600
                if (userFeature == null || userFeature.User == null || !_workspaceService.IsWorkspaceEditor(id, userFeature.User))
                    return Unauthorized();
                if (id != body.Id)
                    return BadRequest("Id in body does not match id in url");
                dto.UserWorkspace workspace = await _workspaceService.UpdateWorkspace(body, userFeature.User);
                return Ok(workspace);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating workspace");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                #pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                #pragma warning restore CS8600
                if (userFeature == null || userFeature.User == null || !_workspaceService.IsWorkspaceOwner(id, userFeature.User))
                    return Unauthorized();
                await _workspaceService.DeleteWorkspace(id);
                return NoContent();
            }
            catch (WorkspaceNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting workspace");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/devices")]
        [Produces("application/json")]
        public async Task<ActionResult<PaginatedList<dto.Device>>> ListDevices(
            Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                #pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                #pragma warning restore CS8600
                if (userFeature == null || userFeature.User == null || !_workspaceService.IsWorkspaceViewer(id, userFeature.User))
                    return Unauthorized();
                PaginatedList<dto.Device> devices = await _workspaceService.GetWorkspaceDevices(id, page, pageSize);
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error listing workspace devices");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/devices")]
        [Produces("application/json")]
        public async Task<ActionResult<dto.NewDevice>> CreateDevice(
            Guid id
        )
        {
            try
            {
                #pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                #pragma warning restore CS8600
                if (
                    userFeature == null ||
                    userFeature.User == null ||
                    !_workspaceService.IsWorkspaceEditor(id, userFeature.User)
                )
                    return Unauthorized();
                Workspace workspace = await _workspaceService.GetWorkspace(id);
                dto.NewDevice device = await _workspaceService.CreateWorkspaceDevice(workspace);
                return CreatedAtAction(nameof(DevicesController.Get), new { id = device.Id }, device);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating workspace device");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/users")]
        [Produces("application/json")]
        public async Task<ActionResult<List<dto.User>>> ListUsers(
            Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                #pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                #pragma warning restore CS8600
                if (userFeature == null || userFeature.User == null || !_workspaceService.IsWorkspaceViewer(id, userFeature.User))
                    return Unauthorized();
                Workspace workspace = await _workspaceService.GetWorkspace(id);
                PaginatedList<dto.User> users = await _workspaceService.GetWorkspaceUsers(workspace, page, pageSize);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error listing workspace users");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/users")]
        [Produces("application/json")]
        public async Task<ActionResult<dto.User>> AddUser(
            Guid id,
            [FromBody] dto.NewUserRequestBody body
        )
        {
            try
            {
                #pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                #pragma warning restore CS8600
                if (userFeature == null || userFeature.User == null || !_workspaceService.IsWorkspaceOwner(id, userFeature.User))
                    return Unauthorized();
                Workspace workspace = await _workspaceService.GetWorkspace(id);
                dto.User user = await _workspaceService.AddWorkspaceUser(workspace, body);
                return CreatedAtAction(nameof(ListUsers), new { id = user.Id }, user);
            }
            catch(InvalidEmailException)
            {
                return BadRequest("Invalid email");
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding workspace user");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}/users/{userId}")]
        [Produces("application/json")]
        public async Task<IActionResult> RemoveUser(Guid id, Guid userId)
        {
            try
            {
                #pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                #pragma warning restore CS8600
                if (userFeature == null || userFeature.User == null)
                    return Unauthorized();
                if (!_workspaceService.IsWorkspaceOwner(id, userFeature.User) && userFeature.User.Id != userId)
                    return Forbid();
                await _workspaceService.RemoveWorkspaceUser(id, userId);
                return NoContent();
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error removing workspace user");
                return BadRequest(ex.Message);
            }
        }
    }
}