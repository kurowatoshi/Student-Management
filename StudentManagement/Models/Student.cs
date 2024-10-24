using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string Phone { get; set; } = "";

        [Required]
        [StringLength(20)]
        [Display(Name = "Enrollment Number")]
        public string EnrollNumber { get; set; } = "";

        [Required(ErrorMessage = "Date of admission is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Admission")]

        public byte[] ImageData { get; set; } // Store image as byte array
        public DateTime DateOfAdmission { get; set; }
    }
}
