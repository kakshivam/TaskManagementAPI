using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs
{
    public class UpdateTaskDto
    {
        [StringLength(200, MinimumLength =1, ErrorMessage ="Title must be between 1 and 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage ="Description should not exceed more than 1000 characters")]
        public string? Description { get; set; }

        public bool? IsCompleted { get; set; }

        [Range(1,3, ErrorMessage ="Priority must be 1(Low),2(Medium),3(High)")]
        public int? Priority { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
