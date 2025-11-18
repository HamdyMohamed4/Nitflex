using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class SubscriptionPlan : BaseTable
    {
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerMonth { get; set; }

        [MaxLength(50)]
        public string VideoAndSoundQuality { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Resolution { get; set; } = "HD";  // Default Resolution

        public string SupportedDevices { get; set; }  // Devices supported

        public int MaxSimultaneousDevices { get; set; }  // Devices that can watch at the same time

        public int MaxDownloadDevices { get; set; }  // Download devices

        public string? SpatialAudio { get; set; }  // Optional: For Premium plan

        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}