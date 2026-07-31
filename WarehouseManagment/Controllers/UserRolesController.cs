using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Constants;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireAdministrator)]
    public class UserRolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogService _auditLogService;

        public UserRolesController(
            UserManager<ApplicationUser> userManager,
            IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(x => x.UserName)
                .ToListAsync();

            var model = new List<UserRoleListModel>();

            foreach (var user in users)
            {
                model.Add(new UserRoleListModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = (await _userManager.GetRolesAsync(user))
                        .Where(x => ApplicationRoles.All.Contains(x))
                        .ToList()
                });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var model = await BuildEditModelAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserRoleEditModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return NotFound();
            }

            var allRoles = ApplicationRoles.All
                .OrderBy(x => x)
                .ToList();

            model.SelectedRoles = model.SelectedRoles
                .Where(x => allRoles.Contains(x))
                .Distinct()
                .ToList();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();

            if (rolesToRemove.Contains(ApplicationRoles.Administrator))
            {
                var administratorUsers = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Administrator);

                if (administratorUsers.Count <= 1)
                {
                    ModelState.AddModelError(string.Empty, "Не може да бъде премахната последната Administrator роля.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.AllRoles = allRoles;
                model.AssignedRoles = currentRoles;
                model.UserName = user.UserName;
                model.Email = user.Email;
                return View(model);
            }

            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);

                if (!addResult.Succeeded)
                {
                    AddIdentityErrors(addResult);
                    model.AllRoles = allRoles;
                    model.AssignedRoles = currentRoles;
                    model.UserName = user.UserName;
                    model.Email = user.Email;
                    return View(model);
                }
            }

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                if (!removeResult.Succeeded)
                {
                    AddIdentityErrors(removeResult);
                    model.AllRoles = allRoles;
                    model.AssignedRoles = await _userManager.GetRolesAsync(user);
                    model.UserName = user.UserName;
                    model.Email = user.Email;
                    return View(model);
                }
            }

            await AuditRoleChangesAsync(user, currentRoles, model.SelectedRoles, rolesToAdd, rolesToRemove);

            TempData["UserRolesMessage"] = "Ролите са обновени успешно.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<UserRoleEditModel?> BuildEditModelAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return null;
            }

            var assignedRoles = await _userManager.GetRolesAsync(user);
            var allRoles = ApplicationRoles.All
                .OrderBy(x => x)
                .ToList();

            return new UserRoleEditModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                AllRoles = allRoles,
                AssignedRoles = assignedRoles,
                SelectedRoles = assignedRoles.ToList()
            };
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task AuditRoleChangesAsync(ApplicationUser user, IList<string> oldRoles, IList<string> newRoles, IList<string> addedRoles, IList<string> removedRoles)
        {
            if (!addedRoles.Any() && !removedRoles.Any())
            {
                return;
            }

            await _auditLogService.SaveStandaloneAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Update,
                EntityType = "UserRoles",
                EntityId = null,
                Description = $"Променени роли на потребител {user.UserName}.",
                OldValues = JsonSerializer.Serialize(new { UserId = user.Id, UserName = user.UserName, Roles = oldRoles }),
                NewValues = JsonSerializer.Serialize(new { UserId = user.Id, UserName = user.UserName, Roles = newRoles, Added = addedRoles, Removed = removedRoles })
            });
        }
    }
}
