/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Services
{
    /// <summary>
    /// </summary>
    public interface IConsentOfServiceService
    {
        /// <summary>
        ///     Get the currently used style
        /// </summary>
        /// <returns></returns>
        Task<ConsentOfService> GetCurrentConsentAsync();

        /// <summary>
        ///     Update the currently used style
        /// </summary>
        /// <param name="consentOfService">consent</param>
        /// <param name="changedById">user id that update consent</param>
        /// <returns></returns>
        Task<bool> UpdateCurrentConsentAsync(ConsentOfService consentOfService, Guid changedById);

        /// <summary>
        ///     Get list of permissions to consent by user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Permission>> GetListOfPermissionsToConsentAsync(Guid userId);
    }
}