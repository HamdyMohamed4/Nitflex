using Domains;
using Microsoft.AspNetCore.Identity;

namespace InfrastructureLayer.UserModels
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public int CurrentState { get; set; }
        public UserAccountStatus AccountStatus { get; set; } = UserAccountStatus.Pending;
        public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public ICollection<UserProfile> Profiles { get; set; } = new List<UserProfile>();
    }
}
