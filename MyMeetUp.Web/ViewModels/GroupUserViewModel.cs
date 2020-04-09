using MyMeetUp.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace MyMeetUp.Web.ViewModels
{
    public class GroupUserViewModel
    {
        public bool IsUserSignedIn;
        public bool IsUserMember;
        public List<GroupMemberProfile> GroupMemberProfiles;

        public bool IsUserGroupCoordinator() {
            return GroupMemberProfiles.Any(gmp => gmp.Name == "COORDINATOR");
        }
    }
}
