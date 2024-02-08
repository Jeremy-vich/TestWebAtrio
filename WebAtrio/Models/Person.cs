using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebAtrio.Extensions;

namespace WebAtrio.Models
{
    public class Person
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public List<Job> Jobs { get; set; } = new();

        [NotMapped]
        public int Age => BirthDate.CalculateAge();
        [NotMapped]
        public List<Job> CurrentJobs => Jobs.ToList().FindAll(x => x.IsCurrent).OrderByDescending(x => x.StartDate).ToList();
    }
}
