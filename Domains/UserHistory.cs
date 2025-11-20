using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class UserHistory : BaseTable
    {
        public Guid ProfileId { get; set; }
        public UserProfile Profile { get; set; } = default!;

        public ContentType ContentType { get; set; }
        public Guid ContentId { get; set; }
        public DateTime LastWatched { get; set; }
        public TimeSpan Position { get; set; } // إضافية لتخزين آخر موقع للمشاهدة
        public TimeSpan Duration { get; set; } // إضافية لتخزين مدة المحتوى

    }
}
