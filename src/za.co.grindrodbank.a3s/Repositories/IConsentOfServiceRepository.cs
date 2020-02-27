/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    /// <summary>
    ///     Consent of service repository
    /// </summary>
    public interface IConsentOfServiceRepository
    {
        /// <summary>
        ///     Get currently used consent
        /// </summary>
        /// <returns></returns>
        Task<ConsentOfServiceModel> GetCurrentConsentAsync();

        /// <summary>
        ///     Update currently used consent
        /// </summary>
        /// <param name="consentOfService"></param>
        /// <returns></returns>
        Task<ConsentOfServiceModel> UpdateCurrentConsentAsync(ConsentOfServiceModel consentOfService);

        /// <summary>
        ///     Get list of permissions to consent by user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<PermissionModel>> GetListOfPermissionsToConsentAsync(string userId);

        /// <summary>
        /// Consent user choice registration
        /// </summary>
        /// <param name="userModel">user data</param>
        /// <param name="permissions">permissions</param>
        /// <returns></returns>
        Task<ConsentOfServiceUserAcceptanceModel> ConsentRegistration(UserModel userModel, List<PermissionModel> permissions);
    }
}