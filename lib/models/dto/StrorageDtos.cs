using System;
using lib.models.mqtt;

namespace lib.models.dto
{
    public enum StorageMessageType {
        PutUrlRequest,
        PutUrlResponse,
        GetUrlRequest,
        GetUrlResponse,
    }
    public class WorkspaceStoragePayload
    {
        public StorageMessageType Type { get; set; }
        public string? Url { get; set;} = default!;
        public string? ObjectName { get; set; } = default!;
    }
    public class WorkspaceStorageMessage : MqttDto<WorkspaceStoragePayload>
    {
    }
}
