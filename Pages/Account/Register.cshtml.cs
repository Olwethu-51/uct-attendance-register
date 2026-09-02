using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UCTAttendanceRegister.Data;
using UCTAttendanceRegister.Models;

namespace UCTAttendanceRegister.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty; // "Student" or "Lecturer"

        // Required only when Role == "Student" — enforced manually below,
        // since [Required] can't be conditional via data annotations alone.
        [Display(Name = "Student Number")]
        public string? StudentNumber { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.Role == "Student" && string.IsNullOrWhiteSpace(Input.StudentNumber))
        {
            ModelState.AddModelError("Input.StudentNumber", "Student number is required for student accounts.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            FullName = Input.FullName
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        await _userManager.AddToRoleAsync(user, Input.Role);

        if (Input.Role == "Student")
        {
            _context.Students.Add(new Student
            {
                StudentNumber = Input.StudentNumber!,
                FullName = Input.FullName,
                Email = Input.Email,
                ApplicationUserId = user.Id
            });
        }
        else
        {
            _context.Lecturers.Add(new Lecturer
            {
                FullName = Input.FullName,
                Email = Input.Email,
                ApplicationUserId = user.Id
            });
        }

        await _context.SaveChangesAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);

        // Redirect to Index for now — will point to the role-specific
        // dashboard once Student/Lecturer Dashboard pages exist.
        return RedirectToPage("/Index");
    }
}