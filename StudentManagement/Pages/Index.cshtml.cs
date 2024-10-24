using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Services;

namespace StudentManagement.Pages
{
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment environment;
        private readonly StudentManagement.Services.ApplicationDbContext context;

        public IndexModel(IWebHostEnvironment environment, ApplicationDbContext context)
        {
          this.context = context;
            this.environment = environment;

            // Initialize StudentDto to avoid null reference
            StudentDto = new StudentDto();
        }

        public IList<Student> Student { get;set; } = new List<Student>();

        [BindProperty]
        public StudentDto StudentDto { get; set; }
        public Student Students { get; set; }
        public async Task OnGetAsync()
        {
            if (context.Students != null)
            {
                Student = await context.Students.ToListAsync();
            }
        }

		public async Task<IActionResult> OnPostDeleteAsync([FromBody] int id)
		{
			if (id == 0)
			{
				return BadRequest("Invalid student ID");
			}

			var student = await context.Students.FindAsync(id);
			if (student == null)
			{
				return NotFound($"Student with ID {id} not found");
			}

			context.Students.Remove(student);
			await context.SaveChangesAsync();

			return new JsonResult(new { success = true, message = "Student deleted successfully." });
		}


		public async Task<IActionResult> OnGetDetailsAsync(int id)
        {
            var student = await context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            // Convert byte[] to Base64 string
            string base64Image = "";
            if (student.ImageData != null)
            {
                base64Image = Convert.ToBase64String(student.ImageData);
            }
            return new JsonResult(new
            {
                name = student.Name,
                email = student.Email,
                phone = student.Phone,
                enrollNumber = student.EnrollNumber,
                dateOfAdmission = student.DateOfAdmission.ToString("MM/dd/yyyy"),
                imageDataUrl = base64Image != "" ? $"data:image/png;base64,{base64Image}" : ""
            });
        }



        public string errorMessage = "";
        public string successMessage = "";


       public async Task OnPostAsync()
        {
            if (StudentDto.ImageFile == null)
            {
                ModelState.AddModelError("StudentDto.ImageFile", "The Image file is required");
            }

            if (!ModelState.IsValid)
            {
                errorMessage = "Please provide all fields";
                return;
            }

            // Generate EnrollNumber (YYYYNNNN)
            string currentYear = DateTime.Now.Year.ToString();
            var lastStudent = context.Students
                .Where(s => s.EnrollNumber.StartsWith(currentYear))
                .OrderByDescending(s => s.EnrollNumber)
                .FirstOrDefault();

            int nextNumber = 1; // Default if no student exists for the current year
            if (lastStudent != null)
            {
                string lastSequence = lastStudent.EnrollNumber.Length >= 8
                    ? lastStudent.EnrollNumber.Substring(4)
                    : "0000";

                if (int.TryParse(lastSequence, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            string newEnrollNumber = $"{currentYear}{nextNumber:D4}";

            // Save image as byte array
            using (var memoryStream = new MemoryStream())
            {
                await StudentDto.ImageFile.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray(); // Convert image to byte array

                // Save new student
                Student student = new Student()
                {
                    Name = StudentDto.Name,
                    Email = StudentDto.Email,
                    Phone = StudentDto.Phone,
                    EnrollNumber = newEnrollNumber,
                    ImageData = imageBytes, // Store byte array
                    DateOfAdmission = DateTime.Now,
                };

                context.Students.Add(student);
                await context.SaveChangesAsync(); // Save to database

                // Clear form data
                StudentDto = new StudentDto(); // Reset the entire DTO object
                ModelState.Clear();
                successMessage = $"Student Added Successfully. Enroll Number: {newEnrollNumber}";
                Response.Redirect("/Index");
            }
        }
        public async Task<IActionResult> OnPostEditStudentAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var student = await context.Students.FindAsync(StudentDto.Id);
            if (student == null)
            {
                return NotFound();
            }

            student.Name = StudentDto.Name;
            student.Email = StudentDto.Email;
            student.Phone = StudentDto.Phone;

            // Handle the image upload if a new image is provided
            if (StudentDto.ImageFile != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await StudentDto.ImageFile.CopyToAsync(memoryStream);
                    student.ImageData = memoryStream.ToArray(); // Store the new byte array
                }
            }

            await context.SaveChangesAsync(); // Save changes to the database
            return new JsonResult(new { success = true });
        }

        private bool StudentExists(int id)
        {
            return (context.Students?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }


}
