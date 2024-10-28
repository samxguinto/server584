using Microsoft.EntityFrameworkCore;

namespace myapi.DTO
{
    public class CountryPopulation
    {
        public int Id { get; set; }

        [Unicode(false)]
        public string Name { get; set; } = null!;


        public int Population { get; set; }
    }
}
//Id Name Population