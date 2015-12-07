using System;
using System.Web;
using System.Web.Security;
using OnlineStore.BuisnessLogic.UserGruop.Contracts;

namespace OnlineStore.BuisnessLogic.UserGruop
{
    public class UserGroup : IUserGroup
    {
        public MembershipUser GetUser(bool canBeAnonymous = false)
        {
            var user = Membership.GetUser();

            if (user != null) return user;

            if (canBeAnonymous) return null;
            throw new NullReferenceException();
        }

        public bool ValidateUser(string login, string password)
        {
            return Membership.ValidateUser(login, password);
        }

        public void LogOut(HttpResponseBase response, HttpSessionStateBase sessionState)
        {
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetExpires(DateTime.Now);

            FormsAuthentication.SignOut();
            sessionState.Abandon();

            response.Redirect(@"/");
        }

        public void SetRoleForUser(string userName, string roleName = "Admin")
        {
            Roles.AddUserToRole(userName, roleName);
        }

        public bool CheckIsUserIsAdmin(string userName)
        {
            return Roles.IsUserInRole(userName, "Admin");
        }
    }
}