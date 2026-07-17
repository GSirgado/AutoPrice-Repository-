using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoPrice.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Apagar todos os cookies de sessão criados no login/registo.
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("nomeCompleto");
            Response.Cookies.Delete("userId");
            Response.Cookies.Delete("fotoUrl");

            return RedirectToPage("/Index");
        }
    }
}
