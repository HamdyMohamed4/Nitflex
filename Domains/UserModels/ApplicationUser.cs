using Domains;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InfrastructureLayer.UserModels
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public ICollection<UserProfile> Profiles { get; set; } = new List<UserProfile>();


    }
}
