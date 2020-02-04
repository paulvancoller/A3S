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
    public interface IUserService : ITransactableService
    {
        Task<User> GetByIdAsync(Guid userId, bool includeRelations = false);
        Task<User> UpdateAsync(UserSubmit userSubmit, Guid updatedById);
        Task<User> CreateAsync(UserSubmit userSubmit, Guid createdById);
        Task<List<User>> GetListAsync();
        Task<PaginatedResult<UserModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, string filterUsername, List<KeyValuePair<string, string>> orderBy);
        Task DeleteAsync(Guid userId);
        Task ChangePasswordAsync(UserPasswordChangeSubmit changeSubmit);
    }
}
