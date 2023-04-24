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

namespace tests.api.controllers
{

    public class DeviceControllerTest : DatabaseUnitTest
    {
        [Fact]
        public async Task Get_Should_ReturnEmptyList()
        {

            // Act
            DeviceController controller = new DeviceController(_context, new DeviceKeyGenerator(), new AppConfiguration());
            var result = await controller.List(); 
            // Assert
            result.Should().BeOfType<ActionResult<IEnumerable<Device>>>();
            result.Value.Should().NotBeNull();
            if (result.Value != null) {
                result.Value.ToList().Count.Should().Be(0);
            }

        }

        [Fact]
        public async Task Post_Should_CreateRecord()
        {

            // Arrange
            var configuration = new AppConfiguration() {
                MQTT = new MQTT() {
                    Uri = "mqtt://localhost:1883"
                }
            };
            DeviceController controller = new DeviceController(_context, new DeviceKeyGenerator(), configuration);

            // Act
            ActionResult<NewDevice> actionResult = await controller.Post();
            // Assert
            var result = actionResult.Result as CreatedAtActionResult;
            result.Should().NotBeNull();
            #nullable disable
            if (result.Value != null) {
                var value = result.Value as NewDevice;
                value.Id.Should().NotBe(Guid.Empty);
                value.SecretKey.Should().NotBeNullOrEmpty();
                Uri mqttUri = new Uri(configuration.MQTT.Uri);
                value.MqttUri.Should().Be(mqttUri);
            }
            #nullable enable
        }
    }
}