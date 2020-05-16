using Microsoft.AspNetCore.Identity;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace MyMeetUp.Web.Utils
{
    public class IdentityInitializer
    {
        //Creación de roles por defecto
        public static void SeedRoles(RoleManager<IdentityRole> roleManager) {
            CreateVisitorRol(roleManager);
            CreateAdministratorRol(roleManager);
        }

        private static void CreateAdministratorRol(RoleManager<IdentityRole> roleManager) {
            if (!roleManager.RoleExistsAsync(RolesData.Administrator).Result) {
                IdentityRole rol = new IdentityRole();
                rol.Name = RolesData.Administrator;
                var roleResult = roleManager.CreateAsync(rol).Result;
            }
        }

        private static void CreateVisitorRol(RoleManager<IdentityRole> roleManager) {
            if (!roleManager.RoleExistsAsync(RolesData.Visitor).Result) {
                IdentityRole rol = new IdentityRole();
                rol.Name = RolesData.Visitor;
                var roleResult = roleManager.CreateAsync(rol).Result;
            }
        }

        //Creación de usuario por defecto
        public static void SeedUsers(UserManager<ApplicationUser> userManager, ApplicationDbContext applicationDbContext) {
            PopulateAuxiliaryTables(applicationDbContext);
            CreateDefaultVisitorUser(userManager, applicationDbContext);
            CreateDefaultAdministratorUser(userManager, applicationDbContext);
        }

        private static void PopulateAuxiliaryTables(ApplicationDbContext applicationDbContext) {
            PopulateGroupCategoryTable(applicationDbContext);
            PopulateGroupMemberProfileTable(applicationDbContext);
            PopulateEventAttendanceStateTable(applicationDbContext);
            PopulateEventCategoryTable(applicationDbContext);
        }

        public static void PopulateGroupCategoryTable(ApplicationDbContext applicationDbContext) {
            if (GroupCategoryTableIsEmpty(applicationDbContext)) {
                var groupCategories = new List<GroupCategory> {
                    new GroupCategory { Name = "TIC" },
                    new GroupCategory { Name = "ARTE" },
                    new GroupCategory { Name = "IDIOMAS" },
                    new GroupCategory { Name = "COACHING" },
                    new GroupCategory { Name = "DEPORTE" },
                    new GroupCategory { Name = "OCIO" },
                    new GroupCategory { Name = "SALUD" },
                    new GroupCategory { Name = "CIENCIAS" }
                };
                applicationDbContext.GroupCategories.AddRange(groupCategories);
                applicationDbContext.SaveChanges();
            }
        }

        private static bool GroupCategoryTableIsEmpty(ApplicationDbContext applicationDbContext) {
            return applicationDbContext.GroupCategories.Count() == 0;
        }

        public static void PopulateGroupMemberProfileTable(ApplicationDbContext applicationDbContext) {
            if (GroupMemberProfileTableIsEmpty(applicationDbContext)) {
                var groupMemberProfiles = new List<GroupMemberProfile> {
                    new GroupMemberProfile { Name = "COORDINATOR" },
                    new GroupMemberProfile { Name = "MEMBER" }
                };
                applicationDbContext.GroupMemberProfiles.AddRange(groupMemberProfiles);
                applicationDbContext.SaveChanges();
            }
        }
        private static bool GroupMemberProfileTableIsEmpty(ApplicationDbContext applicationDbContext) {
            return applicationDbContext.GroupMemberProfiles.Count() == 0;
        }

        public static void PopulateEventAttendanceStateTable(ApplicationDbContext applicationDbContext) {
            if (EventAttendanceStateTableIsEmpty(applicationDbContext)) {
                var eventAttendancesStates = new List<EventAttendanceState> {
                    new EventAttendanceState { State = "I WILL ATTEND" },
                    new EventAttendanceState { State = "I WILL NOT ATTEND" },
                    new EventAttendanceState { State = "I DID ATTEND" },
                    new EventAttendanceState { State = "I DID NOT ATTEND" },
                    new EventAttendanceState { State = "I LEFT SEAT BEFORE DATE" }
                };
                applicationDbContext.EventAttendanceStates.AddRange(eventAttendancesStates);
                applicationDbContext.SaveChanges();
            }
        }
        private static bool EventAttendanceStateTableIsEmpty(ApplicationDbContext applicationDbContext) {
            return applicationDbContext.EventAttendanceStates.Count() == 0;
        }

        public static void PopulateEventCategoryTable(ApplicationDbContext applicationDbContext) {
            if (EventCategoryTableIsEmpty(applicationDbContext)) {
                var eventCategories = new List<EventCategory> {
                    new EventCategory { Name = "CI/CD DEVOPS" },
                    new EventCategory { Name = "SOLID PROGRAMMING" },
                    new EventCategory { Name = "SOFTWARE DEVELOPMENT" },
                    new EventCategory { Name = "PHOTOGRAPHY" },
                    new EventCategory { Name = "ENGLISH" }
                };
                applicationDbContext.EventCategories.AddRange(eventCategories);
                applicationDbContext.SaveChanges();
            }
        }
        private static bool EventCategoryTableIsEmpty(ApplicationDbContext applicationDbContext) {
            return applicationDbContext.EventCategories.Count() == 0;
        }


        private static void CreateDefaultVisitorUser(UserManager<ApplicationUser> userManager, ApplicationDbContext applicationDbContext) {
            if (userManager.FindByNameAsync(RolesData.Visitor).Result == null) {
                var user = new ApplicationUser();
                user.UserName = RolesData.Visitor;
                user.Name = RolesData.Visitor;
                user.Surname = $"{ RolesData.Visitor} Surname";
                user.DNI = "00000000A";
                user.Email = $"{RolesData.Visitor.ToLower()}@mail.com";
                var userResult = userManager.CreateAsync(user, "1234Test!").Result;
                if (userResult.Succeeded)
                    userManager.AddToRoleAsync(user, RolesData.Visitor).Wait();
            }
        }
        private static void CreateDefaultAdministratorUser(UserManager<ApplicationUser> userManager, ApplicationDbContext applicationDbContext) {
            if (userManager.FindByNameAsync(RolesData.Administrator).Result == null) {
                var user = new ApplicationUser();
                user.UserName = RolesData.Administrator;
                user.Name = RolesData.Administrator;
                user.Surname = $"{ RolesData.Administrator} Surname";
                user.DNI = "00000000B";
                user.Email = $"{RolesData.Administrator.ToLower()}@mail.com";
                var userResult = userManager.CreateAsync(user, "1234Test!").Result;
                if (userResult.Succeeded)
                    userManager.AddToRoleAsync(user, RolesData.Administrator).Wait();
            }
        }
    }
}
