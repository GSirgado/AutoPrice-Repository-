using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoPrice.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Apagar os cookies de autenticação
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("nomeCompleto");

            return RedirectToPage("/Index");
        }
    }
}