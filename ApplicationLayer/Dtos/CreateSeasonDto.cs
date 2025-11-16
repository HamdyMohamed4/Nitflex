using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class CreateSeasonDto:BaseDto
    {
        public int SeasonNumber { get; set; }
    }
}
