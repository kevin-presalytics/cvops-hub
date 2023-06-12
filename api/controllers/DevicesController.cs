using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using lib.models.db;
using dto = lib.models.dto;
using lib.middleware;
using System.Net.Mime;
using lib.extensions;
using lib.services;
using lib.models.exceptions;
using Serilog;

namespace api.controllers
{
    [Authorize]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        IDeviceService _deviceService;
        IWorkspaceService _workspaceService;
        ILogger _logger;
        public DevicesController(IDeviceService deviceService, IWorkspaceService workspaceService, ILogger logger) { 
            _deviceService = deviceService;
            _workspaceService = workspaceService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<dto.Device>> Get(Guid id)
        {
            try {
                # pragma warning disable CS8600
                // Get Request User from Jwt Token
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                # pragma warning restore CS8600
                // Get Device, 404 if missing
                Device device = await _deviceService.GetDevice(id);
                if (device == null)
                    return NotFound();
                // Get Device's workspace to test for access permissions
                Workspace workspace = await _workspaceService.GetWorkspace(device.WorkspaceId);
                // Logic for resource-based access controller
                if (userFeature == null || userFeature.User == null || !_workspaceService.IsWorkspaceViewer(workspace.Id, userFeature.User))
                    return Unauthorized();
                // Return DeviceDTO
                return Ok(device.ToDto());
            } catch (DeviceNotFoundException) {
                // Handles NotFound in child services
                return NotFound();
            } catch (Exception ex) {
                // Handles other errors
                // Note: Po
                _logger.Error(ex, "Error in DevicesController.Get");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<dto.Device>> Put(
            Guid id,
            [FromBody] dto.UpdateDeviceBody body
        ) {
            try {
                // Return BadRequest for Invalid Data submission
                if (body.Id != id)
                    return BadRequest("Primary Key in Body does not match Url Path");
                # pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                # pragma warning restore CS8600
                Device device = await _deviceService.GetDevice(id);
                if (device == null)
                    return NotFound();
                if (userFeature == null || userFeature.User == null || !_workspaceService.IsWorkspaceOwner(device.WorkspaceId, userFeature.User))
                    return Unauthorized();
                if (body.Name != null) device.Name = body.Name;
                if (body.Description != null) device.Description = body.Description;
                await _deviceService.UpdateDevice(device);
                return Ok(device.ToDto());
            } catch (DeviceNotFoundException) {
                return NotFound();
            } catch (Exception ex) {
                _logger.Error(ex, "Error in DevicesController.Put");
                return BadRequest(ex.Message);
            }           
        }

        [HttpDelete("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try {
                # pragma warning disable CS8600
                IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
                # pragma warning restore CS8600
                Device device = await _deviceService.GetDevice(id);
                if (device == null)
                    return NotFound();
                if (userFeature == null || userFeature.User == null || !_workspaceService.IsWorkspaceOwner(device.WorkspaceId, userFeature.User))
                    return Unauthorized();
                await _deviceService.DeleteDevice(device);
                return NoContent();
            } catch (DeviceNotFoundException) {
                return NotFound();
            } catch (Exception ex) {
                _logger.Error(ex, "Error in DevicesController.Delete");
                return BadRequest(ex.Message);
            }
        }
    }
}