using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using lib.models;
using lib.models.db;
using lib.models.dto;
using lib.models.configuration;
using lib.services.auth;

namespace api.controllers
{

    // DeviceController class for api to create api endpoints for devices for lib/models/db/Device.cs model
    // Contains Get Endpoint to list a users devices
    // Contains Get Endpoint to get a single devices details
    // Contains Post Endpoint to create a new device
    // Contains Put Endpoint to update a device
    // contains Delete Endpoint to delete a device
    // Contains a /authorize endpoint to authorize a device connection over mqt
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize]
    public class DeviceController : Controller
    {
        CvopsDbContext _context;
        IDeviceKeyGenerator _keyGenerator;
        AppConfiguration _configuration;
        public DeviceController(
            CvopsDbContext context, 
            IDeviceKeyGenerator keyGenerator, 
            AppConfiguration configuration) : base() 
        { 
            _context = context;
            _keyGenerator = keyGenerator;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> List()
        {
            var devices = await _context.Devices.ToListAsync<Device>();
            return devices;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> Get(Guid id)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(e => e.Id == id);
            if (device == null)
                return NotFound();
            return device;
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