// Unit Tests for the DeviceController class from api/controllers/DeviceController.cs
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using tests.fixtures;
using api.controllers;
using lib.models;
using lib.models.dto;
using lib.models.db;
using lib.services.auth;
using lib.models.configuration;
using lib.middleware;
using lib.extensions;


namespace tests.api.controllers
{

    public class DeviceControllerTest : DatabaseUnitTest
    {
        [Fact]
        public async Task Get_Should_ReturnEmptyList()
        {
            // assume
            var testUser = new User() { 
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                JwtSubject = Guid.NewGuid().ToString()        
            };
            await _context.Users.AddAsync(testUser);
            var appConfig = new AppConfiguration();
            DevicesController controller = new DevicesController(_context, new DeviceKeyGenerator(), new AppConfiguration());
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(testUser));
            // Act

            var actionResult = await controller.List(); 
            // Assert

            var result = actionResult.Result as OkObjectResult;
            # pragma warning disable CS8602
            result.Value.Should().NotBeNull();
            # pragma warning restore CS8602
            result.Value.Should().BeAssignableTo<IEnumerable<Device>>();
            
            var devices = result.Value as IEnumerable<Device>;
            devices.Should().BeEmpty();

        }

        [Fact]
        public async Task Post_Should_CreateRecord()
        {

            // Arrange
            var configuration = new AppConfiguration() {
                Domain = "test.dev",
                MQTT = new MQTT() {
                    useTls = true,
                    SecurePort = 8883,
                }
            };
            DevicesController controller = new DevicesController(_context, new DeviceKeyGenerator(), configuration);

            // Act
            ActionResult<NewDevice> actionResult = await controller.Post();
            // Assert
            var result = actionResult.Result as CreatedAtActionResult;
            result.Should().NotBeNull();
            #nullable disable
            if (result.Value != null) {
                var value = result.Value as NewDevice;
                value.Id.Should().NotBe(Guid.Empty);
                value.SecretKey.Should().NotBeNullOrEmpty();;
                value.MqttUri.Should().Be(configuration.GetPublicMqttConnectionUrl());
            }
            #nullable enable
        }
    }
}