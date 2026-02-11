using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadAndWatch.Models
{
    [Table("FavoriteMovies")]
    public class FavoriteMovie
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Movie")]
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public ICollection<FavoriteMovie> FavoriteMovies { get; set; }

    }
}
