using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authentication.Web.Models;
using Authentication.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Web.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var roels = await _roleManager.Roles.ToListAsync();
            return View(roels);
        }

        public IActionResult AddRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(RoleAddViewModel roleAddViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(roleAddViewModel);
            }

            var role = new IdentityRole
            {
                Name = roleAddViewModel.RoleName
            };

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(roleAddViewModel);
        }

        public async Task<IActionResult> EditRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return RedirectToAction("Index");
            }

            var roleEditViewModel = new RoleEditViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
                Users = new List<string>()
            };

            var users = await _userManager.Users.ToListAsync();
            foreach (var u in users)
            {
                if (await _userManager.IsInRoleAsync(u, role.Name))
                {
                    roleEditViewModel.Users.Add(u.UserName);
                }
            }
            return View(roleEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(RoleEditViewModel roleEditViewModel)
        {
            var role = await _roleManager.FindByIdAsync(roleEditViewModel.Id);
            if (role != null)
            {
                role.Name = roleEditViewModel.RoleName;
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, "更新角色时出错");

                return View(roleEditViewModel);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, "删除角色时出错");
            }
            ModelState.AddModelError(string.Empty, "没找到该角色");
            return View("Index", await _roleManager.Roles.ToListAsync());
        }

        public async Task<IActionResult> AddUserToRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return RedirectToAction("Idenx");
            }

            var vm = new UserRoleViewModel
            {
                RoleId = role.Id,
            };

            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                if (!await _userManager.IsInRoleAsync(user, role.Name))
                {
                    vm.Users.Add(user);
                }
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToRole(UserRoleViewModel userRoleViewModel)
        {
            var role = await _roleManager.FindByIdAsync(userRoleViewModel.RoleId);
            var user = await _userManager.FindByIdAsync(userRoleViewModel.UserId);
            if (user != null && role != null)
            {
                var result = await _userManager.AddToRoleAsync(user, role.Name);
                if (result.Succeeded)
                {
                    return RedirectToAction("EditRole", new { id = role.Id });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(userRoleViewModel);
            }

            ModelState.AddModelError(string.Empty, "用户或角色未找到");
            return View(userRoleViewModel);
        }

        public async Task<IActionResult> DeleteUserFromRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return RedirectToAction("Index");
            }

            var vm = new UserRoleViewModel
            {
                RoleId = role.Id
            };

            var users = await _userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    vm.Users.Add(user);
                }
            }

            return View(vm);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserFromRole(UserRoleViewModel userRoleViewModel)
        {
            var user = await _userManager.FindByIdAsync(userRoleViewModel.UserId);
            var role = await _roleManager.FindByIdAsync(userRoleViewModel.RoleId);

            if (user != null && role != null)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, role.Name);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("EditRole", new { id = role.Id });
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(userRoleViewModel);
                }

                ModelState.AddModelError(string.Empty, "用户不在角色里");
                return View(userRoleViewModel);
            }

            ModelState.AddModelError(string.Empty, "用户或角色未找到");
            return View(userRoleViewModel);
        }

    }

}
