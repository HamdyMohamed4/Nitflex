using Domains;
using Microsoft.AspNetCore.Identity;

namespace InfrastructureLayer.UserModels
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? Name { get; set; } = string.Empty;
        public int CurrentState { get; set; } = 0;
        public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public ICollection<UserProfile> Profiles { get; set; } = new List<UserProfile>();
    }
}
