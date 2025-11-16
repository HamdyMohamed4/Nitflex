using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public enum AgeRating
    {
        AllAges,            // Safe for all ages
        FamilyGuidance,     // Some scenes may require parental guidance
        Teens13Plus,        // Not recommended for children under 13
        Mature17Plus,       // Contains strong language or violence - 17+
        AdultsOnly          // Adults only - 18+
    }
}
