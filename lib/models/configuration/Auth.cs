namespace lib.models.configuration
{
    public class Auth
    {
        public string UserIdJwtClaim { get; set; } = default!;
        public Oidc Oidc { get; set; } = default!;
    }
}