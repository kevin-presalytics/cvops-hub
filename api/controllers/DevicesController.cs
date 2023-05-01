using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using lib.models;
using lib.models.db;
using lib.models.dto;
using lib.models.configuration;
using lib.services.auth;
using lib.middleware;
using System.Net.Mime;

namespace api.controllers
{

    // DeviceController class for api to create api endpoints for devices for lib/models/db/Device.cs model
    // Contains Get Endpoint to list a users devices
    // Contains Get Endpoint to get a single devices details
    // Contains Post Endpoint to create a new device
    // Contains Put Endpoint to update a device
    // contains Delete Endpoint to delete a device
    // Contains a /authorize endpoint to authorize a device connection over mqt
    [Authorize]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        CvopsDbContext _context;
        IDeviceKeyGenerator _keyGenerator;
        AppConfiguration _configuration;
        public DevicesController(
            CvopsDbContext context, 
            IDeviceKeyGenerator keyGenerator, 
            AppConfiguration configuration) : base() 
        { 
            _context = context;
            _keyGenerator = keyGenerator;
            _configuration = configuration;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<Device>>> List()
        {
            # pragma warning disable CS8600
            IRequestUserFeature userFeature = HttpContext.Features.Get<IRequestUserFeature>();
            # pragma warning restore CS8600
            if (userFeature == null || userFeature.User == null)
                return Unauthorized();
            var devices = await _context.Devices
                .Where(d => d.Team.Users.Any(u => u.Id == userFeature.User.Id))
                .ToListAsync();
            return Ok(devices);
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<Device>> Get(Guid id)
        {
            # pragma warning disable CS8600
            RequestUserFeature userFeature = HttpContext.Features.Get<RequestUserFeature>();
            # pragma warning restore CS8600
            if (userFeature == null || userFeature.User == null)
                return Unauthorized();
            var device = await _context.Devices
                .Where(d => d.Team.Users.Any(u => u.Id == userFeature.User.Id))
                .FirstOrDefaultAsync(e => e.Id == id);
            if (device == null)
                return NotFound();
            return Ok(device);
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<NewDevice>> Post()
        {
            SecureDeviceCredentials _key = _keyGenerator.GenerateKey();
            Device device = new Device() {
                Salt = _key.Salt,
                Hash = _key.Hash,
            };

            await _context.Devices.AddAsync(device);
            await _context.SaveChangesAsync();
            Uri mqttUri = new Uri(_configuration.MQTT.Uri);
            NewDevice newDevice = new NewDevice() {
                Id = device.Id,
                SecretKey = _key.Key,
                MqttUri = mqttUri,
            };
            return CreatedAtAction(nameof(Get), new { id = device.Id }, newDevice);
        }
    }
}