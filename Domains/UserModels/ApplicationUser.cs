using Domains;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.UserModels
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ICollection<UserWatchlist> WatchlistItems { get; set; } = new List<UserWatchlist>();
        public ICollection<UserRating> Ratings { get; set; } = new List<UserRating>();
        public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();


    }
}
