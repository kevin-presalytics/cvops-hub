using System.Threading.Tasks;
using lib.services;
using lib.models.db;
using Serilog;

namespace mqtt_controller.workers
{
    public class WorkspaceEventWorker : ChannelListener<PlatformEvent>
    {
        private readonly IStorageService _storageService;
        public WorkspaceEventWorker(IStorageService storageService, ILogger logger) : base(logger)
        {
            _storageService = storageService;
        }

        public override async Task HandleMessage(PlatformEvent platformEvent)
        {
            switch (platformEvent.EventType) {
                case PlatformEventTypes.WorkspaceCreated:
                    await CreateWorkspace(platformEvent);
                    break;
                case PlatformEventTypes.WorkspaceDeleted:
                    await DeleteWorkspace(platformEvent);
                    break;
                default:
                    break;
            }
        }

        private async Task CreateWorkspace(PlatformEvent platformEvent)
        {
            await _storageService.CreateBucket(platformEvent.WorkspaceId.ToString());
        }

        private async Task DeleteWorkspace(PlatformEvent platformEvent)
        {
            await _storageService.DeleteBucket(platformEvent.WorkspaceId.ToString());
        }


    }
}