using lib.services.mqtt;
using lib.models.db;
using dto = lib.models.dto;

namespace lib.extensions
{
    public static class UserExtensions
    {
        public static dto.User ToDto(this User user) => new dto.User() {
            Id = user.Id,
            Email = user.Email,
        };
    }
}