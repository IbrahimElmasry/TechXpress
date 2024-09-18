using System.ComponentModel.DataAnnotations;

namespace TechXpress.Entities.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now; // i dont need the user to enter it !

    }
}
