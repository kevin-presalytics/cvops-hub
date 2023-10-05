using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.controllers;
using FluentAssertions;
using lib.middleware;
using lib.models.db;
using lib.services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Serilog;
using tests.fixtures;
using Xunit;
using dto = lib.models.dto;
using lib.extensions;
using lib.models.exceptions;
using lib.models;

namespace tests.api.controllers
{
    public class WorkspaceControllerTests
    {
        public ILogger _logger = new Mock<ILogger>().Object;

        [Fact]
        public async Task List_WithValidUser_ReturnsList()
        {
            // Arrange
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var workspaceResult = new List<dto.UserWorkspace>() {
                new dto.UserWorkspace() {
                    Id = StaticFixtures.TestWorkspace.Id,
                    Name = StaticFixtures.TestWorkspace.Name,
                    Description = StaticFixtures.TestWorkspace.Description,
                }
            };

            mockWorkspaceService
                .Setup(x => x.GetWorkspaces(StaticFixtures.TestViewer))
                .Returns(Task.FromResult(workspaceResult));


            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));

            // Act
            var actionResult = await controller.List();

            // Assert
            #pragma warning disable CS8600, CS8602, CS8604

            var result = actionResult.Result as OkObjectResult;
            result.Value.Should().BeOfType<List<dto.UserWorkspace>>();
            List<dto.UserWorkspace> value = (List<dto.UserWorkspace>)result.Value;
            value.Should().NotBeNull();
            value.Count.Should().Be(1);
            value[0].Should().BeEquivalentTo(workspaceResult[0]);
            #pragma warning restore CS8600, CS8602, CS8604
        }

        [Fact]
        public async Task List_WithInvalidUser_ReturnsUnauthorized()
        {
            // Arrange
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var invalidUser = new User()
            {
                Id = Guid.NewGuid(),
                Email = "invaliduser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            #pragma warning disable CS8625
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(null));
            #pragma warning restore CS8625
            // Act
            var actionResult = await controller.List();

            // Assert

            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Get_WithValidUser_ReturnsWorkspace()
        {
            // Arrange
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var expectedResult = StaticFixtures.TestWorkspace.ToUserWorkspace(StaticFixtures.TestViewer);

            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestViewer))
                .Returns(Task.FromResult(expectedResult));

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestViewer))
                .Returns(true);
            

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));

            var actionResult = await controller.Get(StaticFixtures.TestWorkspace.Id);

            #pragma warning disable CS8600, CS8602, CS8604

            var result = actionResult.Result as OkObjectResult;
            result.Value.Should().BeOfType<dto.UserWorkspace>();
            dto.UserWorkspace value = (dto.UserWorkspace)result.Value;
            value.Should().NotBeNull();
            value.Should().Be(expectedResult);
            #pragma warning restore CS8600, CS8602, CS8604
        }

        [Fact]
        public async Task Get_WithInvalidUser_ReturnsUnauthorized()
        {
            // Arrange
            var invalidUser = new User() {
                Id = Guid.NewGuid(),
                Email = "invaliduser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            var mockWorkspaceService = new Mock<IWorkspaceService>();

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(StaticFixtures.TestWorkspace.Id, invalidUser))
                .Returns(false);
            

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(invalidUser));

            var actionResult = await controller.Get(StaticFixtures.TestWorkspace.Id);

            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        // This could also return not found, but we don't want to leak information about the workspace
        [Fact]
        public async Task Get_WithInvalidWorkspaceId_ReturnsUnauthorized()
        {
            // Arrange
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var badWorkspaceId = Guid.NewGuid();

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(badWorkspaceId, StaticFixtures.TestViewer))
                .Throws(new WorkspaceNotFoundException());

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));

            var actionResult = await controller.Get(Guid.NewGuid());

            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Post_WithValidUser_ReturnsWorkspace()
        {
            // Arrange
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var expectedResult = StaticFixtures.TestWorkspace.ToUserWorkspace(StaticFixtures.TestViewer);

            dto.NewWorkspaceRequestBody requestBody = new dto.NewWorkspaceRequestBody()
            {
                Name = StaticFixtures.TestWorkspace.Name,
                Description = StaticFixtures.TestWorkspace.Description
            };

            mockWorkspaceService
                .Setup(x => x.CreateWorkspace(requestBody, StaticFixtures.TestViewer))
                .Returns(Task.FromResult(expectedResult));

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));

            var actionResult = await controller.Post(requestBody);

            #pragma warning disable CS8600, CS8602, CS8604

            var result = actionResult.Result as CreatedAtActionResult;
            result.Value.Should().BeOfType<dto.UserWorkspace>();
            dto.UserWorkspace value = (dto.UserWorkspace)result.Value;
            value.Should().NotBeNull();
            value.Should().Be(expectedResult);
        }

        [Fact]
        public async Task Put_WithValidUser_ReturnsWorkspace()
        {
            // Arrange
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var expectedResult = StaticFixtures.TestWorkspace.ToUserWorkspace(StaticFixtures.TestOwner);

            dto.UpdateWorkspaceRequestBody requestBody = new dto.UpdateWorkspaceRequestBody()
            {
                Id = expectedResult.Id,
                Name = StaticFixtures.TestWorkspace.Name,
                Description = StaticFixtures.TestWorkspace.Description
            };

            mockWorkspaceService
                .Setup(x => x.UpdateWorkspace(requestBody, StaticFixtures.TestOwner ))
                .Returns(Task.FromResult(expectedResult));

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceEditor(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestOwner))
                .Returns(true);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));

            var actionResult = await controller.Put(StaticFixtures.TestWorkspace.Id, requestBody);

            #pragma warning disable CS8600, CS8602, CS8604

            var result = actionResult.Result as OkObjectResult;
            result.Value.Should().BeOfType<dto.UserWorkspace>();
            dto.UserWorkspace value = (dto.UserWorkspace)result.Value;
            value.Should().NotBeNull();
            value.Should().Be(expectedResult);
        }

        [Fact]
        public async Task Put_WithInvalidUser_ReturnsUnauthorized()
        {
            var invalidUser = new User() {
                Id = Guid.NewGuid(),
                Email = "invaliduser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            dto.UpdateWorkspaceRequestBody requestBody = new dto.UpdateWorkspaceRequestBody()
            {
                Name = StaticFixtures.TestWorkspace.Name,
                Description = StaticFixtures.TestWorkspace.Description
            };

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceEditor(StaticFixtures.TestWorkspace.Id, invalidUser))
                .Returns(false);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(invalidUser));

            var actionResult = await controller.Put(StaticFixtures.TestWorkspace.Id, requestBody);

            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Put_WithInvalidWorkspaceId_ReturnsUnauthorized()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var badWorkspaceId = Guid.NewGuid();

            dto.UpdateWorkspaceRequestBody requestBody = new dto.UpdateWorkspaceRequestBody()
            {
                Name = StaticFixtures.TestWorkspace.Name,
                Description = StaticFixtures.TestWorkspace.Description
            };

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceEditor(badWorkspaceId, StaticFixtures.TestOwner))
                .Returns(false);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));

            var actionResult = await controller.Put(badWorkspaceId, requestBody);

            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Put_WithMismatchedIds_ReturnsBadRequest() 
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var goodGuid = Guid.NewGuid();
            dto.UpdateWorkspaceRequestBody requestBody = new dto.UpdateWorkspaceRequestBody()
            {
                Id = Guid.NewGuid(), // Mismatched Id in request body
                Name = StaticFixtures.TestWorkspace.Name,
                Description = StaticFixtures.TestWorkspace.Description
            };

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceEditor(goodGuid, StaticFixtures.TestOwner))
                .Returns(true);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));

            var actionResult = await controller.Put(goodGuid, requestBody);

            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_WithValidUserAndId_ReturnsNoContent()
        {
            // Arrange
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestOwner))
                .Returns(true);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));

            var actionResult = await controller.Delete(StaticFixtures.TestWorkspace.Id);

            actionResult.Should().BeOfType<NoContentResult>();
        }

        // Could Return NotFoundResult here. Not sure which is better for an id that doesn't exist
        [Fact]
        public async Task Delete_WithInvalidId_ReturnsUnauthorized()
        {
                        // Arrange
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var badWorkspaceId = Guid.NewGuid();

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(badWorkspaceId, StaticFixtures.TestOwner))
                .Returns(false);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));

            var actionResult = await controller.Delete(StaticFixtures.TestWorkspace.Id);

            actionResult.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Delete_WithInvalidUser_ReturnsUnauthorized()
        {
            var invalidUser = new User() {
                Id = Guid.NewGuid(),
                Email = "invaliduser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            var mockWorkspaceService = new Mock<IWorkspaceService>();

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, invalidUser))
                .Returns(false);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(invalidUser));

            var actionResult = await controller.Delete(StaticFixtures.TestWorkspace.Id);

            actionResult.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task CreateDevice_WithValidUser_ReturnsCreated()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var expectedResult = new dto.NewDevice() {
                Id = StaticFixtures.TestDevice.Id,
                SecretKey = StaticFixtures.TestDeviceCredentials.Key,
                MqttUri = new Uri("mqtt://test.org"),
            };

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceEditor(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestOwner))
                .Returns(true);

            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestWorkspace.Id))
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));

            mockWorkspaceService
                .Setup(x => x.CreateWorkspaceDevice(StaticFixtures.TestWorkspace))
                .Returns(Task.FromResult(expectedResult));

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));

            var actionResult = await controller.CreateDevice(StaticFixtures.TestWorkspace.Id);

            #pragma warning disable CS8600, CS8602, CS8604

            var result = actionResult.Result as CreatedAtActionResult;
            result.Value.Should().BeOfType<dto.NewDevice>();
            dto.NewDevice value = (dto.NewDevice)result.Value;
            value.Should().NotBeNull();
            value.Should().Be(expectedResult);
            #pragma warning restore CS8600, CS8602, CS8604
        }

        [Fact]
        public async Task CreateDevice_WithInvalidUser_ReturnsUnauthorized()
        {
            var invalidUser = new User() {
                Id = Guid.NewGuid(),
                Email = "invaliduser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var expectedResult = new dto.NewDevice() {
                Id = StaticFixtures.TestDevice.Id,
                SecretKey = StaticFixtures.TestDeviceCredentials.Key,
                MqttUri = new Uri("mqtt://test.org"),
            };

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceEditor(StaticFixtures.TestWorkspace.Id, invalidUser))
                .Returns(false);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(invalidUser));

            var actionResult = await controller.CreateDevice(StaticFixtures.TestWorkspace.Id);

            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ListDevices_WithValidUser_ReturnsPaginatedList()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var expectedResult = new PaginatedList<dto.Device>(
                new List<dto.Device>() {
                    StaticFixtures.TestDevice.ToDto()
                }, 1, 1, 10);

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestViewer))
                .Returns(true);

            mockWorkspaceService
                .Setup(x => x.GetWorkspaceDevices(StaticFixtures.TestWorkspace.Id, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(Task.FromResult(expectedResult));
            
            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));

            var actionResult = await controller.ListDevices(StaticFixtures.TestWorkspace.Id);

            #pragma warning disable CS8600, CS8602, CS8604
            actionResult.Result.Should().BeOfType<OkObjectResult>();
            var result = actionResult.Result as OkObjectResult;
            result.Value.Should().BeOfType<PaginatedList<dto.Device>>();
            PaginatedList<dto.Device> value = (PaginatedList<dto.Device>)result.Value;
            value.Should().NotBeNull();
            value[0].Should().Be(expectedResult[0]);
            #pragma warning restore CS8600, CS8602, CS8604
        }


        [Fact]
        public async Task ListDevices_WithInvalidUser_ReturnsUnauthorized()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var invalidUser = new User() {
                Id = Guid.NewGuid(),
                Email = "invaliduser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(StaticFixtures.TestWorkspace.Id, invalidUser))
                .Returns(false);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(invalidUser));

            var actionResult = await controller.ListDevices(StaticFixtures.TestWorkspace.Id);

            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ListUsers_WithValidUser_ReturnsPaginatedList()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var expectedResult = new PaginatedList<dto.User>(
                new List<dto.User>() {
                    StaticFixtures.TestViewer.ToDto(WorkspaceUserRole.Viewer),
                    StaticFixtures.TestOwner.ToDto(WorkspaceUserRole.Owner)
                }, 2, 1, 10);

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestViewer))
                .Returns(true);

            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestWorkspace.Id))
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));

            mockWorkspaceService
                .Setup(x => x.GetWorkspaceUsers(StaticFixtures.TestWorkspace, It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(expectedResult));
            
            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));

            var actionResult = await controller.ListUsers(StaticFixtures.TestWorkspace.Id);

            #pragma warning disable CS8600, CS8602, CS8604
            actionResult.Result.Should().BeOfType<OkObjectResult>();
            var result = actionResult.Result as OkObjectResult;
            result.Value.Should().BeOfType<PaginatedList<dto.User>>();
            PaginatedList<dto.User> value = (PaginatedList<dto.User>)result.Value;
            value.TotalCount.Should().Be(2);
            value.Count.Should().Be(2);
            value.Should().NotBeNull();
            value[0].Should().Be(expectedResult[0]);
            value[1].Should().Be(expectedResult[1]);
            #pragma warning restore CS8600, CS8602, CS8604
        }

        [Fact]
        public async Task ListUsers_WithInvalidUser_ReturnsUnauthorized()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var invalidUser = new User() {
                Id = Guid.NewGuid(),
                Email = "invaliduser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceViewer(StaticFixtures.TestWorkspace.Id, invalidUser))
                .Returns(false);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(invalidUser));

            var actionResult = await controller.ListUsers(StaticFixtures.TestWorkspace.Id);

            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task AddUser_WithWorkspaceOwner_ReturnsOk()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var body = new dto.NewUserRequestBody() {
                Email = StaticFixtures.TestViewer.Email,
                Role = WorkspaceUserRole.Viewer
            };

            var expectedResult = new dto.User() {
                Id = StaticFixtures.TestViewer.Id,
                Email = StaticFixtures.TestViewer.Email,
            };

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestOwner))
                .Returns(true);

            mockWorkspaceService
                .Setup(x => x.GetWorkspace(StaticFixtures.TestWorkspace.Id)) 
                .Returns(Task.FromResult(StaticFixtures.TestWorkspace));  

            mockWorkspaceService
                .Setup(x => x.AddWorkspaceUser(StaticFixtures.TestWorkspace, body))
                .Returns(Task.FromResult(expectedResult));

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));

            var actionResult = await controller.AddUser(StaticFixtures.TestWorkspace.Id, body);

            #pragma warning disable CS8600, CS8602, CS8604
            actionResult.Result.Should().BeOfType<CreatedAtActionResult>();
            var result = actionResult.Result as CreatedAtActionResult;
            result.Value.Should().BeOfType<dto.User>();
            dto.User value = (dto.User)result.Value;
            value.Should().Be(expectedResult);
            #pragma warning restore CS8600, CS8602, CS8604
        }


        [Fact]
        public async Task AddUser_WithWorkspaceViewer_ReturnsUnauthorized()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var body = new dto.NewUserRequestBody() {
                Email = StaticFixtures.TestViewer.Email,
                Role = WorkspaceUserRole.Viewer
            };

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestViewer))
                .Returns(false);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));

            var actionResult = await controller.AddUser(StaticFixtures.TestWorkspace.Id, body);

            actionResult.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task RemoveUser_WithWorkspaceOwner_ReturnsNoContent()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var deleteUserId = Guid.NewGuid();

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestOwner))
                .Returns(true);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));

            var actionResult = await controller.RemoveUser(StaticFixtures.TestWorkspace.Id, deleteUserId);

            actionResult.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task RemoveUser_WithWorkspaceOwnerAndNonExistentUser_ReturnsNotFound()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var deleteUserId = Guid.NewGuid();

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestOwner))
                .Returns(true);
            
            mockWorkspaceService
                .Setup(x => x.RemoveWorkspaceUser(StaticFixtures.TestWorkspace.Id, deleteUserId))
                .Throws(new UserNotFoundException());

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestOwner));

            var actionResult = await controller.RemoveUser(StaticFixtures.TestWorkspace.Id, deleteUserId);

            actionResult.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task RemoveUser_WithViewerUser_ReturnsForbidden()
        {
            var mockWorkspaceService = new Mock<IWorkspaceService>();

            var deleteUserId = Guid.NewGuid();

            mockWorkspaceService
                .Setup(x => x.IsWorkspaceOwner(StaticFixtures.TestWorkspace.Id, StaticFixtures.TestViewer))
                .Returns(false);

            var controller = new WorkspacesController(mockWorkspaceService.Object, _logger);
            controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set<IRequestUserFeature>(new RequestUserFeature(StaticFixtures.TestViewer));

            var actionResult = await controller.RemoveUser(StaticFixtures.TestWorkspace.Id, deleteUserId);

            actionResult.Should().BeOfType<ForbidResult>();
        }


    }
}