/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Models;

namespace za.co.grindrodbank.a3s.Repositories
{
    public interface IUserRepository : IPaginatedRepository<UserModel>, ITransactableRepository
    {
        Task<UserModel> GetByIdAsync(Guid userId, bool includeRelations);
        Task<UserModel> GetByUsernameAsync(string username, bool includeRelations);
        Task<UserModel> CreateAsync(UserModel user, string password, bool isPlainTextPassword);
        Task<UserModel> UpdateAsync(UserModel user);
        Task DeleteAsync(UserModel user);
        Task<List<UserModel>> GetListAsync();
        Task<PaginatedResult<UserModel>> GetPaginatedListAsync(int page, int pageSize, bool includeRelations, string filterName, string filterUsername, List<KeyValuePair<string, string>> orderBy);
        Task ChangePassword(Guid userId, string oldPassword, string newPassword);
    }
}
