using System;

namespace lib.models.exceptions
{
    public class DeviceNotFoundException : Exception
    {
        public DeviceNotFoundException()
        {
        }

        public DeviceNotFoundException(string message)
            : base(message)
        {
        }

        public DeviceNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}