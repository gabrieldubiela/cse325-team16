using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRentalManager.Services;

// Handles the OAuth callback. We swap the external Google identity for a local JWT
// to bridge the gap between the HTTP context and the Blazor SignalR circuit.
public class ExternalLoginCallbackModel : PageModel
{
    private readonly IJwtService  _jwtService;
    private readonly UserService  _userService;

    public ExternalLoginCallbackModel(IJwtService jwtService, UserService userService)
    {
        _jwtService  = jwtService;
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Retrieve the temporary claim principal established by the Google handler during the handshake.
        var result = await HttpContext.AuthenticateAsync(
            Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded)
            // Handle cases where the user denied access or the provider returned an error.
            return Redirect("/auth?error=login_failed");

        var email    = result.Principal?.FindFirstValue(ClaimTypes.Email);
        var name     = result.Principal?.FindFirstValue(ClaimTypes.Name);
        var googleId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (email == null || googleId == null)
            // Essential identity claims are missing; we cannot proceed without a unique identifier (email/ID).
            return Redirect("/auth?error=no_email");

        name ??= email;

        // Check MongoDB for existing user
        var user = await _userService.FindByEmailAsync(email);
        bool isNewUser = false;

        if (user == null)
        {
            // Auto-register new users to reduce friction, but keep them in a pending state for security.
            user = await _userService.CreateAsync(name, email, googleId);
            isNewUser = true;
        }

        // Enforce manual approval gate before allowing system access.
        if (!user.IsApproved)
        {
            var status = isNewUser ? "pending_new" : "pending";
            return Redirect($"/auth?status={status}");
        }

        // Approved — issue JWT and let them in
        await _userService.UpdateLastLoginAsync(user.Id!);

        var token = _jwtService.GenerateToken(user.Id!, user.Email, user.Name);

        // Persist the JWT in a cookie so it flows to the Blazor circuit initialization (JwtAuthenticationStateProvider).
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure   = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires  = DateTimeOffset.UtcNow.AddHours(8)
        });

        return Redirect("/");
    }
}