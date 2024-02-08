using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAtrio.Models
{
    public class Job
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Required]
        public string JobName { get; set; } = string.Empty;
        [Required]
        public string CompanyName { get; set; } = string.Empty;
        [Required]
        public bool IsCurrent { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [JsonIgnore]
        public virtual int PersonId { get; set; }
        [JsonIgnore]
        public virtual Person? Person { get; set; }
    }
}
