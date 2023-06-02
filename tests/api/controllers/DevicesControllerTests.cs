// Unit Tests for the DeviceController class from api/controllers/DeviceController.cs
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using tests.fixtures;
using api.controllers;
using dto = lib.models.dto;
using lib.models.db;
using lib.middleware;
using lib.services;
using Moq;
using lib.models.exceptions;
using Serilog;


namespace tests.api.controllers
{

    public class DeviceControllerTest : DatabaseUnitTest
    {
        private ILogger logger = new Mock<ILogger>().Object;

        [Fact]
        public async Task Get_WithValidId_ShouldReturnDevice()
        {
            // assume
            var mockDeviceService = new Mock<IDeviceService>();
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            DevicesController controller = new DevicesController(mockDeviceService.Object, mockWorkspaceService.Object, logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));
            
            mockDeviceService
                .Setup(x => x.GetDevice(StaticFixtures.TestDevice.Id))
                .Returns(Task.FromResult(StaticFixtures.TestDevice));
            
            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestDevice.WorkspaceId))
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));
            
            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestViewer))
                .Returns(true);

            // Act
            var actionResult = await controller.Get(StaticFixtures.TestDevice.Id);
            // Assert

            # pragma warning disable CS8600, CS8602, CS8604
            var result = actionResult.Result as OkObjectResult;
            var value = (dto.Device)result.Value;
            value.Should().NotBeNull();
            value.Should().BeOfType<dto.Device>();
            value.Id.Should().Be(StaticFixtures.TestDevice.Id);
            # pragma warning restore CS8600, CS8602, CS8604
        }

        [Fact]
        public async Task Get_WithInvalidId_ShouldReturnNotFound()
        {
            // assume
            var mockDeviceService = new Mock<IDeviceService>();
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            DevicesController controller = new DevicesController(mockDeviceService.Object, mockWorkspaceService.Object, logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));
            
            mockDeviceService
                .Setup(x => x.GetDevice(StaticFixtures.TestDevice.Id))
                .Returns(Task.FromResult(StaticFixtures.TestDevice));
            
            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestDevice.WorkspaceId))
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));
            
            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestViewer))
                .Returns(true);

            // Act
            var actionResult = await controller.Get(Guid.NewGuid());
            
            // Assert
            actionResult.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Get_WithInvalidUser_ShouldReturnUnauthorized()
        {
            // assume
            var mockDeviceService = new Mock<IDeviceService>();
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var otherUser = new User() {
                Id = Guid.NewGuid(),
                Email = "otheruser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            DevicesController controller = new DevicesController(mockDeviceService.Object, mockWorkspaceService.Object, logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(otherUser));

            mockDeviceService
                .Setup(x => x.GetDevice(StaticFixtures.TestDevice.Id))
                .Returns(Task.FromResult(StaticFixtures.TestDevice));
            
            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestDevice.WorkspaceId))
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));
            
            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(StaticFixtures.TestWorkspace.Id, otherUser))
                .Returns(false);

            // Act
            var actionResult = await controller.Get(StaticFixtures.TestDevice.Id);
            
            // Assert
            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Put_WithValidIdAndBody_ShouldReturnUpdatedDevice()
        {
                        // assume
            var mockDeviceService = new Mock<IDeviceService>();
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            DevicesController controller = new DevicesController(mockDeviceService.Object, mockWorkspaceService.Object, logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));
            
            mockDeviceService
                .Setup(x => x.GetDevice(StaticFixtures.TestDevice.Id))
                .Returns(Task.FromResult(StaticFixtures.TestDevice));
            
            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestDevice.WorkspaceId))
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));
            
            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestOwner))
                .Returns(true);

            var body = new dto.UpdateDeviceBody() {
                Id = StaticFixtures.TestDevice.Id,
                Name = "new name",
            };

            // Act
            var actionResult = await controller.Put(StaticFixtures.TestDevice.Id, body);
            // Assert

            # pragma warning disable CS8600, CS8602, CS8604
            var result = actionResult.Result as OkObjectResult;
            var value = (dto.Device)result.Value;
            value.Should().NotBeNull();
            value.Should().BeOfType<dto.Device>();
            value.Id.Should().Be(StaticFixtures.TestDevice.Id);
            # pragma warning restore CS8600, CS8602, CS8604   
        }

        [Fact]
        public async Task Put_WithInvalidUser_ShouldReturnUnAuthorized()
        {
            var mockDeviceService = new Mock<IDeviceService>();
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var otherUser = new User() {
                Id = Guid.NewGuid(),
                Email = "otheruser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            DevicesController controller = new DevicesController(mockDeviceService.Object, mockWorkspaceService.Object, logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(otherUser));

            mockDeviceService
                .Setup(x => x.GetDevice(StaticFixtures.TestDevice.Id))
                .Returns(Task.FromResult(StaticFixtures.TestDevice));
            
            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestDevice.WorkspaceId))
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));
            
            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(StaticFixtures.TestWorkspace.Id, otherUser))
                .Returns(false);

            var body = new dto.UpdateDeviceBody() {
                Id = StaticFixtures.TestDevice.Id,
                Name = "new name",
            };

            // Act
            var actionResult = await controller.Put(StaticFixtures.TestDevice.Id, body);
            
            // Assert
            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Put_WithNonExistentDevice_ShouldReturnNotFound()
        {
            var mockDeviceService = new Mock<IDeviceService>();
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var otherUser = new User() {
                Id = Guid.NewGuid(),
                Email = "otheruser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            DevicesController controller = new DevicesController(mockDeviceService.Object, mockWorkspaceService.Object, logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(otherUser));

            mockDeviceService
                .Setup(x => x.GetDevice(StaticFixtures.TestDevice.Id))
                .Throws(new DeviceNotFoundException());

            var body = new dto.UpdateDeviceBody() {
                Id = StaticFixtures.TestDevice.Id,
                Name = "new name",
            };

            // Act
            var actionResult = await controller.Put(StaticFixtures.TestDevice.Id, body);
            
            // Assert
            actionResult.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Put_WithMismatchedIds_ShouldReturnBadRequest()
        {
            var mockDeviceService = new Mock<IDeviceService>();
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            DevicesController controller = new DevicesController(mockDeviceService.Object, mockWorkspaceService.Object, logger);
            
            var body = new dto.UpdateDeviceBody() {
                Id = StaticFixtures.TestDevice.Id,
                Name = "new name",
            };

            // Act
            var actionResult = await controller.Put(Guid.NewGuid(), body);
            // Assert

            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_WithValidIdAndOwner_ShouldReturnNoContent()
        {
                        // assume
            var mockDeviceService = new Mock<IDeviceService>();
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            DevicesController controller = new DevicesController(mockDeviceService.Object, mockWorkspaceService.Object, logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));
            
            mockDeviceService
                .Setup(x => x.GetDevice(StaticFixtures.TestDevice.Id))
                .Returns(Task.FromResult(StaticFixtures.TestDevice));
            
            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestDevice.WorkspaceId))
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));
            
            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestOwner))
                .Returns(true);

            // Act
            var actionResult = await controller.Delete(StaticFixtures.TestDevice.Id);
            // Assert

            actionResult.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_WithValidIdAndViewer_ShouldReturnUnauthorized()
        {
                        // assume
            var mockDeviceService = new Mock<IDeviceService>();
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            DevicesController controller = new DevicesController(mockDeviceService.Object, mockWorkspaceService.Object, logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));
            
            mockDeviceService
                .Setup(x => x.GetDevice(StaticFixtures.TestDevice.Id))
                .Returns(Task.FromResult(StaticFixtures.TestDevice));
            
            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestDevice.WorkspaceId))
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));
            
            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestViewer))
                .Returns(false);

            // Act
            var actionResult = await controller.Delete(StaticFixtures.TestDevice.Id);
            // Assert

            actionResult.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Delete_WithInvalidIdAndOwner_ShouldReturnNotFound()
        {
                        // assume
            var mockDeviceService = new Mock<IDeviceService>();
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            DevicesController controller = new DevicesController(mockDeviceService.Object, mockWorkspaceService.Object, logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));
            
            mockDeviceService
                .Setup(x => x.GetDevice(StaticFixtures.TestDevice.Id))
                .Returns(Task.FromResult(StaticFixtures.TestDevice));
            
            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestDevice.WorkspaceId))
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));
            
            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestOwner))
                .Returns(true);

            // Act
            var actionResult = await controller.Delete(Guid.NewGuid());
            // Assert

            actionResult.Should().BeOfType<NotFoundResult>();
        }

        
        // [Fact]
        // public async Task Post_Should_CreateRecord()
        // {

        //     // Arrange
        //     var mockDeviceService = new Mock<IDeviceService>();
        //     var newDeviceDto = new dto.NewDevice() {
        //         Id = Guid.NewGuid(),
        //         SecretKey = "secret",
        //         MqttUri = new Uri("mqtt://localhost:1883")
        //     };
        //     var testWorkspace = new Workspace() {
        //         Id = Guid.NewGuid(),
        //         Name = "test workspace",
        //         Description = ""
        //     };
        //     mockDeviceService.Setup(x => 
        //         x.CreateNewDevice(It.IsAny<Workspace>(), It.IsAny<dto.NewDeviceRequestBody>())
        //     ).Returns(Task.FromResult(newDeviceDto));   

        //     DevicesController controller = new DevicesController(mockDeviceService.Object);
        //     controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        //     controller.ControllerContext.HttpContext.Features.Set<IRequestWorkspaceFeature>(new RequestWorkspaceFeature(testWorkspace));

        //     var body = new dto.NewDeviceRequestBody() { WorkspaceId = Guid.NewGuid() };

        //     // Act
        //     ActionResult<dto.NewDevice> actionResult = await controller.Post(body);
        //     // Assert
        //     var result = actionResult.Result as CreatedAtActionResult;
        //     result.Should().NotBeNull();
        //     #nullable disable
        //     if (result.Value != null) {
        //         var value = result.Value as dto.NewDevice;
        //         value.Id.Should().NotBe(Guid.Empty);
        //         value.Id.Should().Be(newDeviceDto.Id);
        //         value.SecretKey.Should().Be(newDeviceDto.SecretKey);;
        //         value.MqttUri.Should().Be(newDeviceDto.MqttUri);
        //     }
        //     #nullable enable
        // }
    }
}