using System;
using System.Threading.Tasks;

namespace lib.services
{
    public interface IInviteUserService
    {
        Task<bool> SendInvite(string email);
    }

    public class InviteUserService : IInviteUserService
    {
        // Return true for invite sent, false for invite not sent (e.g., unable to send email to address)
        public Task<bool> SendInvite(string email)
        {
            throw new NotImplementedException();
        }
    }
}