using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public enum UserAccountStatus
    {
        Pending = 0,   // سجل ولكن لم يكمل اختيار الخطة والدفع
        Active = 1,    // مفعل بعد الدفع
        Suspended = 2, // انتهاء الاشتراك أو مشكلة في الدفع
        Canceled = 3   // ألغى الحساب/الاشتراك
    }
}
