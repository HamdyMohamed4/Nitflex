using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class NotificationDto
    {
        public Guid ProfileId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "info", "warning", "error"
        public bool IsRead { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
