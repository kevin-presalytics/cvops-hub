using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Serilog;
using lib.models.mqtt;
using lib.services.mqtt;

namespace api.controllers
{

    // Authorizes devices on when sending a CONNECT packet to the MQTT broker
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class MqttController : Controller
    {
        IMqttHttpAuthenticator _mqttHttpAuthenticator;
        IMqttHttpAuthorizer _mqttHttpAuthorizer;
        ILogger _logger;
        public MqttController(
            IMqttHttpAuthenticator mqttHttpAuthenticator,
            IMqttHttpAuthorizer mqttHttpAuthorizer,
            ILogger logger
        ) : base()
        { 
            _mqttHttpAuthenticator = mqttHttpAuthenticator;
            _mqttHttpAuthorizer = mqttHttpAuthorizer;
            _logger = logger;
        }

        [HttpPost]
        [Route("auth")]
        [Produces("application/json")]
        public async Task<ActionResult<MqttAuthResponse>> Post([FromBody] EqmxAuthBody body)
        {
            _logger.Debug($"MQTT Auth request for {body.Username}");
            var response = await _mqttHttpAuthenticator.Authenticate(body.Username, body.Password);
            _logger.Debug($"MQTT Auth result for {body.Username}: {response.Result}");
            return response;   
        }

        [HttpPost]
        [Route("auth/acl")]
        [Produces("application/json")]
        public async Task<ActionResult<EqmxAuthorizeResponse>> Post([FromBody] EqmxAuthorizeBody body)
        {
            _logger.Debug($"MQTT ACL request for {body.Username}");
            var response = await _mqttHttpAuthorizer.Authorize(body.Username, body.Topic, body.Action);
            _logger.Debug($"MQTT ACL result for {body.Username}: {response.Result}");
            return response;
        }
    }
}