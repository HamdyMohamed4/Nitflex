using InfrastructureLayer.UserModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class UserWatchlist : BaseTable
    {
        // Foreign Key to ApplicationUser
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        // ContentId is now Guid
        [MaxLength(20)]
        public ContentType ContentType { get; set; }
        public Guid ContentId { get; set; } // References Movie.Id or TVShow.Id/Episode.Id
    }

}
