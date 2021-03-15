//  -----------------------------------------------------------------------------
//   Copyright  (c)  Balsamic Solutions, LLC. All rights reserved.
//   THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER
//   EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR
//  -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace BalsamicSolutions.ApiCommon
{
    /// <summary>
    /// Interface for User Manager services
    /// </summary>
    public interface IUserManager
    {
        bool AddGroup(Group group);

        bool AddUser(User user);

        bool AddUsersToGroup(string groupName, IEnumerable<string> userNames);

        bool DeleteGroup(Group group);

        bool DeleteUser(string userName);

        bool DisableUser(string userName);

        bool EnableUser(string userName);

        UserManagerResponse<Group> GetGroup(string groupName);

        UserManagerResponse<Group> GetGroupsForUser(string groupName);

        UserManagerResponse<User> GetUser(string userName);

        UserManagerResponse<User> GetUsersForGroup(string groupName);

        UserManagerResponse<Group> ListGroups(string filter, int limit, string paginationToken);

        UserManagerResponse<User> ListUsers(string filter, int limit, string paginationToken);

        bool RemoveUsersFromGroup(string groupName, IEnumerable<string> userNames);

        bool ResetUserPassword(string userName, string newPassword, bool mustChangePasswordAtNextLogin);

        bool UpdateGroup(Group group);

        bool UpdateUser(User user);
    }
}