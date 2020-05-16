using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using MyMeetUp.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMeetUp.Web.ViewComponents
{
    public class GroupMembershipButtonsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private int _groupId;
        private string _userId;

        public GroupMembershipButtonsViewComponent(ApplicationDbContext context) {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int groupId, string userId) {
            GroupUserViewModel groupUserInfo = new GroupUserViewModel();

            if (ValidationsOK(groupId, userId)) {
                groupUserInfo.IsUserSignedIn = true;
                groupUserInfo.GroupId = groupId;
                groupUserInfo.UserId = userId;
                if (await IsUserMemberOfGroupAsync()) {
                    groupUserInfo.IsUserMember = true;
                    groupUserInfo.GroupMemberProfiles = await GetUserProfilesInThisGroupAsync();
                }
            }
            return View(groupUserInfo);
        }

        private bool ValidationsOK(int groupId, string userId) {
            if ((groupId == 0) || (userId is null) || (userId == ""))
                return false;
            
            _groupId = groupId;
            _userId = userId;
            return true;
        }

        private async Task<bool> IsUserMemberOfGroupAsync() {
            return await _context.GroupMembers.AnyAsync(gm => gm.GroupId == _groupId && gm.ApplicationUserId == _userId);
        }

        private async Task<List<GroupMemberProfile>> GetUserProfilesInThisGroupAsync() {
            return  await _context.GroupMembers.Include(gm => gm.GroupMemberProfile).Where(gm => gm.GroupId == _groupId && gm.ApplicationUserId == _userId).Select(gm => gm.GroupMemberProfile).ToListAsync();
        }
    }
}
