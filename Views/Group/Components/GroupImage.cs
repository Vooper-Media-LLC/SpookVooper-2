using Microsoft.AspNetCore.Mvc;
using SV2.Database.Models.Groups;
using System.Threading.Tasks;

namespace SV2.Web.Components.Groups
{
    public class GroupImage : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Group group)
        {
            return View(group);
        }
    }
}