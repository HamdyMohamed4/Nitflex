using System;
using System.Collections.Generic;

namespace Domains;

public partial class TbPaymentMethod : BaseTable
{
    public string? MethdAname { get; set; }

    public string? MethodEname { get; set; }

    public double? Commission { get; set; }

    //public virtual ICollection<TbShipment> TbShippments { get; set; } = new List<TbShipment>();z
}
