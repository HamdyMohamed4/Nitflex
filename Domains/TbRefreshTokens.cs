using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class TbRefreshTokens : BaseTable
    {
        public string Token { get; set; }

        public string UserId { get; set; }

        public DateTime Expires { get; set; }
    }
}
