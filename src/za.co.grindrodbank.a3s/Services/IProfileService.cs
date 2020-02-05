/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public interface IProfileService : ITransactableService
    {
        // User Profile related functions.
        Task<UserProfile> CreateUserProfileAsync(Guid userId, UserProfileSubmit userProfileSubmit, Guid createdById);
        Task<UserProfile> UpdateUserProfileAsync(Guid userId, Guid userProfileId, UserProfileSubmit userProfileSubmit, Guid updatedById);
        Task<List<UserProfile>> GetUserProfileListForUserAsync(Guid userId);
        Task<UserProfile> GetUserProfileByIdAsync(Guid userProfileId);
        // A user profile has a compound key consisting of the user ID and the name of the profile.
        Task<UserProfile> GetUserProfileByNameAsync(Guid userId, string userProfileName);
        Task DeleteUserProfileAsync(Guid userId, Guid userProfileId);
        Task<PaginatedResult<ProfileModel>> GetPaginatedListForUserAsync(Guid userId, int page, int pageSize, bool includeRelations, string filterName, List<KeyValuePair<string, string>> orderBy);
    }
}
