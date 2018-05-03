using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

namespace ActiveDirectoryAuth
{
    public class ActiveDirectoryManager
    {
        private readonly string _domain;
        private readonly string _defaultOrganisationalUnit;
        private readonly string _serviceUser;
        private readonly string _servicepassword;
        private readonly PrincipalContext _principalContext;
        #region Variables




        public ActiveDirectoryManager(string domain, string defaultOrganisationalUnit, string serviceUser, string servicepassword)
        {
            _domain = domain;
            _defaultOrganisationalUnit = defaultOrganisationalUnit;
            _serviceUser = serviceUser;
            _servicepassword = servicepassword;
            _principalContext = new PrincipalContext(ContextType.Domain, _domain, _defaultOrganisationalUnit, ContextOptions.Negotiate, _serviceUser, _servicepassword);

        }
        #endregion
        #region Validate Methods

        /// <summary>
        /// Validates the username and password of a given user
        /// </summary>
        /// <param name="sUserName">The username to validate</param>
        /// <param name="sPassword">The password of the username to validate</param>
        /// <returns>Returns True of user is valid</returns>
        public bool ValidateCredentials(string sUserName, string sPassword)
        {
            var oPrincipalContext = GetPrincipalContext();
            return oPrincipalContext.ValidateCredentials(sUserName, sPassword);

        }

        /// <summary>
        /// Checks if the User Account is Expired
        /// </summary>
        /// <param name="sUserName">The username to check</param>
        /// <returns>Returns true if Expired</returns>
        public bool IsUserExpired(string sUserName)
        {
            var oUserPrincipal = GetUser(sUserName);
            return oUserPrincipal.AccountExpirationDate == null;
        }

        /// <summary>
        /// Checks if user exsists on AD
        /// </summary>
        /// <param name="sUserName">The username to check</param>
        /// <returns>Returns true if username Exists</returns>
        public bool IsUserExisiting(string sUserName)
        {
            return GetUser(sUserName) != null;
        }

        /// <summary>
        /// Checks if user accoung is locked
        /// </summary>
        /// <param name="sUserName">The username to check</param>
        /// <returns>Retruns true of Account is locked</returns>
        public bool IsAccountLocked(string sUserName)
        {
            var oUserPrincipal = GetUser(sUserName);
            return oUserPrincipal.IsAccountLockedOut();
        }
        #endregion

        #region Search Methods

        /// <summary>
        /// Gets a certain user on Active Directory
        /// </summary>
        /// <param name="sUserName">The username to get</param>
        /// <returns>Returns the UserPrincipal Object</returns>
        public UserPrincipal GetUser(string sUserName)
        {
            var oPrincipalContext = GetPrincipalContext();

            var oUserPrincipal = UserPrincipal.FindByIdentity(oPrincipalContext, sUserName);
            return oUserPrincipal;
        }

        /// <summary>
        /// Gets a certain group on Active Directory
        /// </summary>
        /// <param name="sGroupName">The group to get</param>
        /// <returns>Returns the GroupPrincipal Object</returns>
        public GroupPrincipal GetGroup(string sGroupName)
        {
            var oPrincipalContext = GetPrincipalContext();

            var oGroupPrincipal = GroupPrincipal.FindByIdentity(oPrincipalContext, sGroupName);
            return oGroupPrincipal;
        }

        #endregion

        #region User Account Methods

        /// <summary>
        /// Sets the user password
        /// </summary>
        /// <param name="sUserName">The username to set</param>
        /// <param name="sNewPassword">The new password to use</param>
        /// <param name="sMessage">Any output messages</param>
        public void SetUserPassword(string sUserName, string sNewPassword, out string sMessage)
        {
            try
            {
                var oUserPrincipal = GetUser(sUserName);
                oUserPrincipal.SetPassword(sNewPassword);
                sMessage = "";
            }
            catch (Exception ex)
            {
                sMessage = ex.Message;
            }

        }

        /// <summary>
        /// Enables a disabled user account
        /// </summary>
        /// <param name="sUserName">The username to enable</param>
        public void EnableUserAccount(string sUserName)
        {
            var oUserPrincipal = GetUser(sUserName);
            oUserPrincipal.Enabled = true;
            oUserPrincipal.Save();
        }

        /// <summary>
        /// Force disbaling of a user account
        /// </summary>
        /// <param name="sUserName">The username to disable</param>
        public void DisableUserAccount(string sUserName)
        {
            var oUserPrincipal = GetUser(sUserName);
            oUserPrincipal.Enabled = false;
            oUserPrincipal.Save();
        }

        /// <summary>
        /// Force expire password of a user
        /// </summary>
        /// <param name="sUserName">The username to expire the password</param>
        public void ExpireUserPassword(string sUserName)
        {
            var oUserPrincipal = GetUser(sUserName);
            oUserPrincipal.ExpirePasswordNow();
            oUserPrincipal.Save();

        }

        /// <summary>
        /// Unlocks a locked user account
        /// </summary>
        /// <param name="sUserName">The username to unlock</param>
        public void UnlockUserAccount(string sUserName)
        {
            var oUserPrincipal = GetUser(sUserName);
            oUserPrincipal.UnlockAccount();
            oUserPrincipal.Save();
        }

        /// <summary>
        /// Creates a new user on Active Directory
        /// </summary>
        /// <param name="sOU">The OU location you want to save your user</param>
        /// <param name="sUserName">The username of the new user</param>
        /// <param name="sPassword">The password of the new user</param>
        /// <param name="sGivenName">The given name of the new user</param>
        /// <param name="sSurname">The surname of the new user</param>
        /// <returns>returns the UserPrincipal object</returns>
        public UserPrincipal CreateNewUser(string sOU, string sUserName, string sPassword, string sGivenName, string sSurname)
        {
            if (!IsUserExisiting(sUserName))
            {
                var oPrincipalContext = GetPrincipalContext(sOU);

                var oUserPrincipal = new UserPrincipal(oPrincipalContext, sUserName, sPassword, true /*Enabled or not*/)
                {
                    UserPrincipalName = sUserName,
                    GivenName = sGivenName,
                    Surname = sSurname
                };

                //User Log on Name
                oUserPrincipal.Save();

                return oUserPrincipal;
            }
            else
            {
                return GetUser(sUserName);
            }
        }

        /// <summary>
        /// Deletes a user in Active Directory
        /// </summary>
        /// <param name="sUserName">The username you want to delete</param>
        /// <returns>Returns true if successfully deleted</returns>
        public bool DeleteUser(string sUserName)
        {
            try
            {
                var oUserPrincipal = GetUser(sUserName);

                oUserPrincipal.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Group Methods

        /// <summary>
        /// Creates a new group in Active Directory
        /// </summary>
        /// <param name="sOU">The OU location you want to save your new Group</param>
        /// <param name="sGroupName">The name of the new group</param>
        /// <param name="sDescription">The description of the new group</param>
        /// <param name="oGroupScope">The scope of the new group</param>
        /// <param name="bSecurityGroup">True is you want this group to be a security group, false if you want this as a distribution group</param>
        /// <returns>Retruns the GroupPrincipal object</returns>
        public GroupPrincipal CreateNewGroup(string sOU, string sGroupName, string sDescription, GroupScope oGroupScope, bool bSecurityGroup)
        {
            var oPrincipalContext = GetPrincipalContext(sOU);

            var oGroupPrincipal = new GroupPrincipal(oPrincipalContext, sGroupName)
            {
                Description = sDescription,
                GroupScope = oGroupScope,
                IsSecurityGroup = bSecurityGroup
            };
            oGroupPrincipal.Save();

            return oGroupPrincipal;
        }

        /// <summary>
        /// Adds the user for a given group
        /// </summary>
        /// <param name="sUserName">The user you want to add to a group</param>
        /// <param name="sGroupName">The group you want the user to be added in</param>
        /// <returns>Returns true if successful</returns>
        public bool AddUserToGroup(string sUserName, string sGroupName)
        {
            try
            {
                var oUserPrincipal = GetUser(sUserName);
                var oGroupPrincipal = GetGroup(sGroupName);
                if (oUserPrincipal != null && oGroupPrincipal != null)
                    if (!IsUserGroupMember(sUserName, sGroupName))
                    {
                        oGroupPrincipal.Members.Add(oUserPrincipal);
                        oGroupPrincipal.Save();
                    }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes user from a given group
        /// </summary>
        /// <param name="sUserName">The user you want to remove from a group</param>
        /// <param name="sGroupName">The group you want the user to be removed from</param>
        /// <returns>Returns true if successful</returns>
        public bool RemoveUserFromGroup(string sUserName, string sGroupName)
        {
            try
            {
                var oUserPrincipal = GetUser(sUserName);
                var oGroupPrincipal = GetGroup(sGroupName);
                if (oUserPrincipal != null && oGroupPrincipal != null)
                    if (IsUserGroupMember(sUserName, sGroupName))
                    {
                        oGroupPrincipal.Members.Remove(oUserPrincipal);
                        oGroupPrincipal.Save();
                    }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if user is a member of a given group
        /// </summary>
        /// <param name="sUserName">The user you want to validate</param>
        /// <param name="sGroupName">The group you want to check the membership of the user</param>
        /// <returns>Returns true if user is a group member</returns>
        public bool IsUserGroupMember(string sUserName, string sGroupName)
        {
            var oUserPrincipal = GetUser(sUserName);
            var oGroupPrincipal = GetGroup(sGroupName);

            if (oUserPrincipal != null && oGroupPrincipal != null)
                return oGroupPrincipal.Members.Contains(oUserPrincipal);
            else
                return false;
        }

        /// <summary>
        /// Gets a list of the users group memberships
        /// </summary>
        /// <param name="sUserName">The user you want to get the group memberships</param>
        /// <returns>Returns an arraylist of group memberships</returns>
        public IEnumerable<string> GetUserGroups(string sUserName)
        {
            var myItems = new List<string>();
            var oUserPrincipal = GetUser(sUserName);

            var oPrincipalSearchResult = oUserPrincipal.GetGroups();

            foreach (var oResult in oPrincipalSearchResult) myItems.Add(oResult.Name);
            return myItems;
        }

        /// <summary>
        /// Gets a list of the users authorization groups
        /// </summary>
        /// <param name="sUserName">The user you want to get authorization groups</param>
        /// <returns>Returns an arraylist of group authorization memberships</returns>
        public IEnumerable<string> GetUserAuthorizationGroups(string sUserName)
        {
            var myItems = new List<string>();
            var oUserPrincipal = GetUser(sUserName);

            var oPrincipalSearchResult = oUserPrincipal.GetAuthorizationGroups();

            foreach (var oResult in oPrincipalSearchResult) myItems.Add(oResult.Name);
            return myItems;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the base principal context
        /// </summary>
        /// <returns>Retruns the PrincipalContext object</returns>
        public PrincipalContext GetPrincipalContext()
        {
            return _principalContext;
        }

        /// <summary>
        /// Gets the principal context on specified OU
        /// </summary>
        /// <param name="sOU">The OU you want your Principal Context to run on</param>
        /// <returns>Retruns the PrincipalContext object</returns>
        public PrincipalContext GetPrincipalContext(string sOU)
        {
            var oPrincipalContext = new PrincipalContext(ContextType.Domain, _domain, sOU, ContextOptions.Negotiate, _serviceUser, _servicepassword);
            return oPrincipalContext;
        }

        #endregion

    }
}