using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Mapping
{
    public interface IMapper
    {
        public TDestination Map<TSource, TDestination>();
    }
}
