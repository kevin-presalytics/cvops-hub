using lib.models.db;
using System;
using System.Text.Json;
using lib.services.auth;
using dto = lib.models.dto;
using System.Collections.Generic;


namespace tests.fixtures
{
    public static class StaticFixtures
    {
        public static readonly User TestViewer;
        public static readonly User TestOwner;
        public static readonly Device TestDevice;
        public static readonly Workspace TestWorkspace;
        public static readonly WorkspaceUser TestWorkspaceViewer;
        public static readonly WorkspaceUser TestWorkspaceOwner;
        public static readonly dto.SecureDeviceCredentials TestDeviceCredentials;
        static StaticFixtures() {

            TestViewer = new User() {
                Id = Guid.NewGuid(),
                Email = "TestUser@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            TestOwner = new User() {
                Id = Guid.NewGuid(),
                Email = "testowner@test.com",
                JwtSubject = Guid.NewGuid().ToString()
            };

            TestWorkspace = new Workspace() {
                Id = Guid.NewGuid(),
                Name = "Test Workspace",
                Description = "Test Workspace Description",
            };

            TestWorkspaceViewer = new WorkspaceUser() {
                User = TestViewer,
                UserId = TestViewer.Id,
                Workspace = TestWorkspace,
                WorkspaceId = TestWorkspace.Id,
                WorkspaceUserRole = WorkspaceUserRole.Viewer
            };

                        
            TestWorkspaceOwner = new WorkspaceUser() {
                User = TestOwner,
                UserId = TestOwner.Id,
                Workspace = TestWorkspace,
                WorkspaceId = TestWorkspace.Id,
                WorkspaceUserRole = WorkspaceUserRole.Owner
            };

            TestWorkspace.WorkspaceUsers = new List<WorkspaceUser>() { TestWorkspaceViewer, TestWorkspaceOwner };

            DeviceKeyGenerator keyGenerator = new DeviceKeyGenerator(); 

            TestDeviceCredentials = keyGenerator.GenerateKey();

            TestDevice = new Device() {
                Id = Guid.NewGuid(),
                Description = "Test Description",
                Name = "Device Name",
                DeviceInfo = JsonDocument.Parse("{}"),
                Salt = TestDeviceCredentials.Salt,
                Hash = TestDeviceCredentials.Hash,
                Workspace = TestWorkspace,
                WorkspaceId = TestWorkspace.Id
            };

            TestWorkspace.Devices = new List<Device>() { TestDevice };
        }

    }
}