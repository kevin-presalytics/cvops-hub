using tests.fixtures;
using Moq;
using Serilog;

namespace tests.lib.services
{

    // Test class for lib.services.WorkspaceService
    public class WorkspaceServiceTests : DatabaseUnitTest
    {
        private ILogger _logger = new Mock<ILogger>().Object;
        // [Fact]
    //     public async Task GetWorkspace_WithEmptyGuid_ThrowsWorkspaceNotFoundException() {
    //         var mockDeviceService = new Mock<IDeviceService>();
    //         var sut = new WorkspaceService(mockDeviceService.Object, _context);
    //         await Assert.ThrowsAsync<WorkspaceNotFoundException>(async () => {
    //             await sut.GetWorkspace(Guid.Empty);
    //         });
    //     }

    //     [Fact]
    //     public async Task GetWorkspace_WithValidGuid_ReturnsWorkspace() {
    //         var mockDeviceService = new Mock<IDeviceService>();
    //         var workspace = new Workspace() {
    //             Id = Guid.NewGuid(),
    //             Name = "Test Workspace",
    //             Description = "",
    //         };
    //         _context.Workspaces.Add(workspace);
    //         await _context.SaveChangesAsync();
    //         var sut = new WorkspaceService(mockDeviceService.Object, _context);
    //         var result = await sut.GetWorkspace(workspace.Id);
    //         Assert.NotNull(result);
    //         Assert.Equal(workspace.Id, result.Id);
    //     }

    //     [Fact]
    //     public async Task GetWorkspace_WithInvalidGuid_ThrowsWorkspaceNotFoundException() {
    //         var mockDeviceService = new Mock<IDeviceService>();
    //         var sut = new WorkspaceService(mockDeviceService.Object, _context);
    //         await Assert.ThrowsAsync<WorkspaceNotFoundException>(async () => {
    //             await sut.GetWorkspace(Guid.NewGuid());
    //         });
    //     }



    // }
    }
    


}