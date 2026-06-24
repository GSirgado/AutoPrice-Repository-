using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoPrice.Pages
{
    [Authorize]
    public class MensagensModel : PageModel
    {
        public void OnGet() { }
    }
}