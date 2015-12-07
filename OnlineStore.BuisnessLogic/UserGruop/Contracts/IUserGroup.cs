using System.Web;
using System.Web.Security;

namespace OnlineStore.BuisnessLogic.UserGruop.Contracts
{
    public interface IUserGroup
    {
        MembershipUser GetUser(bool canBeAnonymous = false);

        bool ValidateUser(string login, string password);

        void LogOut(HttpResponseBase response, HttpSessionStateBase sessionState);

        void SetRoleForUser(string userName, string roleName);

        bool CheckIsUserIsAdmin(string userName);
    }
}