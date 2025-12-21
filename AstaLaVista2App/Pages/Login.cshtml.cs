using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class LoginModel : PageModel
{
    private readonly AuctionDb _db;
    public LoginModel(AuctionDb db) => _db = db;

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string Error { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
    {
        var hash = Helper.Hash(Password);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == Name && u.Password == hash);
        
        if (user == null)
        {
            // Controlla se è il primo utente
        var isFirstUser = !await _db.Users.AnyAsync();

            // Crea utente se non esiste
            user = new User { Name = Name, Password = hash, IsAdmin = isFirstUser };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
        return RedirectToPage("/Index");
    }
}