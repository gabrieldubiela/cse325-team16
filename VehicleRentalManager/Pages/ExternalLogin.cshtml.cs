using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

// Initiates the OAuth handshake. We use a dedicated page for this to ensure
// the Challenge result is returned within a full page request, avoiding AJAX/SPA routing complexities.
public class ExternalLoginModel : PageModel
{
    public IActionResult OnGet(string returnUrl = "/")
    {
        // Configure the callback URL so the provider knows where to return the user
        // after they approve the application permissions.
        var redirectUrl = Url.Page("/ExternalLoginCallback");
        var properties  = new AuthenticationProperties { RedirectUri = redirectUrl };

        // Delegate to the registered Google handler to manage the redirect to the identity provider.
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
}