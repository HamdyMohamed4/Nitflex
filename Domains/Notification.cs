using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class Notification : BaseTable
    {
        public Guid ProfileId { get; set; } // معرف البروفايل الذي سيتلقى التنبيه
        public string Message { get; set; } = string.Empty; // رسالة التنبيه
        public string Type { get; set; } = string.Empty; // نوع التنبيه مثل "معلومات"، "تحذير"، "خطأ"
        public bool IsRead { get; set; } = false; // هل تمت قراءة التنبيه؟
        public string RedirectUrl { get; set; } = string.Empty; // رابط لتوجيه المستخدم عند الضغط على التنبيه
    }
}

