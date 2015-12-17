using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineStore.BuisnessLogic.Database.Models
{
    [Table("PersonTable")]
    public class Person
    {
        [Key]
        public string Login { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }

        [Column(TypeName = "image")]
        [MaxLength]
        public byte[] Image { get; set; }
    }
}
