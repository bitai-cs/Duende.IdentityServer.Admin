namespace Skoruba.Duende.IdentityServer.Admin.EntityFramework.Shared.Entities.Identity
{
    public interface IUserWithDomain
    {
        string UserDomain { get; set; }
        string UserName { get; set; }
    }
}
