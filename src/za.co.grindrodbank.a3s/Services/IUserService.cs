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

namespace za.co.grindrodbank.a3s.Services
{
    public interface IUserService : ITransactableService
    {
        Task<User> GetByIdAsync(Guid userId, bool includeRelations = false);
        Task<User> UpdateAsync(UserSubmit userSubmit, Guid updatedById);
        Task<User> CreateAsync(UserSubmit userSubmit, Guid createdById);
        Task<List<User>> GetListAsync();
        Task DeleteAsync(Guid userId);
        Task ChangePasswordAsync(UserPasswordChangeSubmit changeSubmit);
        // User Profile related functions.
        Task<UserProfile> CreateUserProfileAsync(Guid userId, UserProfileSubmit userProfileSubmit, Guid createdById);
        Task<UserProfile> UpdateUserProfileAsync(Guid userId, UserProfileSubmit userProfileSubmit, Guid upddatedById);
        Task<List<UserProfile>> GetUserProfileListAsync();
        Task<UserProfile> GetUserProfileByIdAsync(Guid userProfileId);
        // A user profile has a compound key consisting of the user ID and the name of the profile.
        Task<UserProfile> GetUserProfileByNameAsync(Guid userId, string userProfileName);
        Task DeleteUserProfileAsync();
    }
}
