using InfrastructureLayer.UserModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class UserProfile : BaseTable
    {
        [MaxLength(100)]
        public string ProfileName { get; set; } = string.Empty;

        public Guid UserId { get; set; }        // الربط بالـ User
        public ApplicationUser User { get; set; } = default!;

        // Lock / PIN support (store hashed PIN only)
        public bool IsLocked { get; set; } = false;
        public string? PinHash { get; set; }

        // Kid profile flag
        public bool IsKidProfile { get; set; } = false;

        // الحاجات الشخصية المرتبطة بالبروفايل
        public ICollection<UserWatchlist> WatchlistItems { get; set; } = new List<UserWatchlist>();
        public ICollection<UserRating> Ratings { get; set; } = new List<UserRating>();
        public ICollection<UserHistory> Histories { get; set; } = new List<UserHistory>();
    }

}
