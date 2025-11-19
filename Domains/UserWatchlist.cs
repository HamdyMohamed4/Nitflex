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
        public Guid ProfileId { get; set; }
        public UserProfile Profile { get; set; } = default!;
        public ContentType ContentType { get; set; }
        public Guid ContentId { get; set; }
    }
}
