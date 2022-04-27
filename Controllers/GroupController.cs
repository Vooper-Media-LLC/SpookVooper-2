using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using SV2.Database.Models.Users;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Permissions;
using System.Diagnostics;

namespace SV2.Controllers
{
    public class GroupController : Controller
    {
        private readonly ILogger<GroupController> _logger;
        
        [TempData]
        public string StatusMessage { get; set; }

        public GroupController(ILogger<GroupController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult View(string id)
        {
            Group? group = Group.Find(id);
            return View(group);
        }

        public IActionResult Create()
        {
            User? user = UserManager.GetUser(HttpContext);

            if (user is null) 
            {
                return Redirect("/account/login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Group model)
        {
            User? user = UserManager.GetUser(HttpContext);

            if (user is null) 
            {
                return Redirect("/account/login");
            }

            model.Name = model.Name.Trim();

            if (DBCache.GetAll<Group>().Any(x => x.Name == model.Name)) {
                StatusMessage = $"Error: Name {model.Name} is already taken!";
                return Redirect($"/group/create");
            }

            Group group = new Group(model.Name, user.Id);
            group.Description = model.Description;
            group.GroupType = model.GroupType;
            group.DistrictId = model.DistrictId;
            group.Image_Url = model.Image_Url;
            group.OwnerId = user.Id;

            await DBCache.Put<Group>(group.Id, group);
            await VooperDB.Instance.Groups.AddAsync(group);
            await VooperDB.Instance.SaveChangesAsync();

            return Redirect($"/group/view/{group.Id}");
        }

        public IActionResult Edit(string id)
        {
            Group? group = Group.Find(id);
            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Group model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            User? user = UserManager.GetUser(HttpContext);

            if (user is null) 
            {
                return Redirect("/account/login");
            }

            Group prevgroup = Group.Find(model.Id)!;

            if (prevgroup == null)
            {
                StatusMessage = $"Error: Group {model.Name} does not exist!";
                return RedirectToAction("Index", controllerName: "Home");
            }

            if (model.Name != prevgroup.Name) 
            {
                if (DBCache.GetAll<Group>().Any(x => x.Name == model.Name)) {
                    StatusMessage = $"Error: Name {model.Name} is already taken!";
                    return Redirect($"/group/edit/{prevgroup.Id}");
                }
            }

            if (!prevgroup.HasPermission(user, GroupPermissions.Edit))
            {
                StatusMessage = $"Error: You lack permission to edit this Group!";
                return Redirect($"/group/edit/{prevgroup.Id}");
            }

            if (prevgroup.GroupType != model.GroupType)
            {
                StatusMessage = $"Error: Group Type cannot be changed!";
                return Redirect($"/group/edit/{prevgroup.Id}");
            }

            if (prevgroup.OwnerId == user.Id)
            {
                prevgroup.Name = model.Name;
                prevgroup.Image_Url = model.Image_Url;
                prevgroup.Open = model.Open;
                prevgroup.DistrictId = model.DistrictId;
                prevgroup.Description = model.Description;
            }

            StatusMessage = $"Successfully edited {prevgroup.Name}!";

            return Redirect($"/group/view/{prevgroup.Id}");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}