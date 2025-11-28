using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TmdbMovieDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string Release_Date { get; set; }
        public double Vote_Average { get; set; }
        public string Poster_Path { get; set; }
        public List<TmdbGenre> Genres { get; set; }
    }
}
