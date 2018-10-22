﻿using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Clients
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public DeleteModel(ApplicationDbContext appContext, IConfigurationDbContext identityContext, UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityContext = identityContext;
            _userManager = userManager;
        }

        public ClientModel Client { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var clientSecret = await _appContext.ClientSecretText
                .Where(secret => user.ClientIds.Contains(secret.ClientId))
                .SingleOrDefaultAsync(secret => secret.ClientId == id);
            if (clientSecret == null)
            {
                return NotFound();
            }

            var client = await _identityContext.Clients
                .Where(c => user.ClientIds.Contains(c.Id))
                .SingleOrDefaultAsync(c => c.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            Client = new ClientModel
            {
                Id = client.Id,
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientSecret = clientSecret.Secret 
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return Page();
            }

            var clientEntity = await _identityContext.Clients.FindAsync(id.Value);
            if (clientEntity != null)
            {
                _identityContext.Clients.Remove(clientEntity);
                await _identityContext.SaveChangesAsync();
            }
            
            var clientSecretEntity = await _appContext.ClientSecretText
                .SingleOrDefaultAsync(secret => secret.ClientId == id.Value);
            if (clientSecretEntity != null)
            {
                _appContext.ClientSecretText.Remove(clientSecretEntity);
                await _appContext.SaveChangesAsync();
            }

            var user = await _userManager.GetUserAsync(User);
            var clientIds = user.ClientIds;
            if (clientIds.Contains(id.Value))
            {
                clientIds.Remove(id.Value);
                user.ClientIds = clientIds;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToPage("./Index");
        }

        public class ClientModel
        {
            public int Id { get; set; }

            [Display(Name = "Client ID")]
            public string ClientId { get; set; }

            [Display(Name = "Name")]
            public string ClientName { get; set; }

            [Display(Name = "Client Secret")]
            public string ClientSecret { get; set; }
        }
    }
}
