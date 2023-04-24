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
using lib.models.mqtt;

namespace api.controllers
{

    // Authorizes devices on when sending a CONNECT packet to the MQTT broker
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MqttController : Controller
    {
        CvopsDbContext _context;
        IDeviceKeyVerifier _keyVerifier;
        AppConfiguration _configuration;
        public MqttController(
            CvopsDbContext context, 
            IDeviceKeyVerifier keyVerifier, 
            AppConfiguration configuration) : base()
        { 
            _context = context;
            _keyVerifier = keyVerifier;
            _configuration = configuration;
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<MqttAuthResponse>> Post([FromBody] EqmxAuthBody body)
        {
            #pragma warning disable CS8600
            Device device = await _context.Devices.FirstOrDefaultAsync(e => e.Id == new Guid(body.Username));
            #pragma warning restore CS8600
            if (device == null)
                return NotFound();
            if (_keyVerifier.Verify(body.Password, device.Hash, device.Salt))
            {
                MqttAuthResponse response = new MqttAuthResponse() {
                    Result = AuthResultOptions.Allow,
                    IsSuperuser = false,

                };
                return response;
            }
            else
            {
                return Forbid();
            }
        }
    }
}