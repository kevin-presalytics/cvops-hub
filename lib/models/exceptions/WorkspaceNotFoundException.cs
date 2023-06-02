using System;

namespace lib.models.exceptions
{
    public class WorkspaceNotFoundException : Exception
    {
        public WorkspaceNotFoundException()
        {
        }

        public WorkspaceNotFoundException(string message)
            : base(message)
        {
        }

        public WorkspaceNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}