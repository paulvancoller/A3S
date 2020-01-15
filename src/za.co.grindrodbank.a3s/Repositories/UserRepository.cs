/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog;
using za.co.grindrodbank.a3s.Services;

namespace za.co.grindrodbank.a3s.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly A3SContext a3SContext;
        // This is the underlying identity user store manager. Use this, as it inlcudes numerous operations such as password hashing.
        private readonly UserManager<UserModel> identityUserManager;
        private readonly ISafeRandomizerService safeRandomizerService;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public UserRepository(A3SContext a3SContext, UserManager<UserModel> identityUserManager, ISafeRandomizerService safeRandomizerService)
        {
            this.a3SContext = a3SContext;
            this.identityUserManager = identityUserManager;
            this.safeRandomizerService = safeRandomizerService;
        }

        public void InitSharedTransaction()
        {
            if (a3SContext.Database.CurrentTransaction == null)
                a3SContext.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Commit();
        }

        public void RollbackTransaction()
        {
            if (a3SContext.Database.CurrentTransaction != null)
                a3SContext.Database.CurrentTransaction.Rollback();
        }

        public async Task<UserModel> CreateAsync(UserModel user, string password, bool isPlainTextPassword)
        {
            // This will use the indentity user store to create the user. However, this user manager is unaware of roles.
            // Ensure that roles get assigned to the new user.
            IdentityResult result = null;

            // Always treat email as confirmed
            user.EmailConfirmed = true;

            if (isPlainTextPassword && !string.IsNullOrWhiteSpace(password))
                result = await identityUserManager.CreateAsync(user, password);
            else
            {
                user.PasswordHash = safeRandomizerService.RandomString(16);
                result = await identityUserManager.CreateAsync(user);
            }

            if (result.Succeeded)
            {
                // The update async method uses the A3S Context, not the ID Server User manager. This context understands how to persist
                // the additional relations we have added to the UserModel and will persist any roles and teams assigned to the user model.
                UserModel createdUser = await identityUserManager.FindByNameAsync(user.UserName);

                if (createdUser == null)
                    throw new ItemNotFoundException("User creation using Identity Server User store failed!");

                user.Id = createdUser.Id;
                if (!isPlainTextPassword)
                    user.PasswordHash = password;

                await UpdateAsync(user);

                return user;
            }

            string errors = "";
            if(result.Errors != null && result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    errors += $"'{error.Description}'; ";
                }
            } 

            throw new ItemNotProcessableException($"User creation using Identity Server User store failed! Errors: {errors}");
        }

        public async Task DeleteAsync(UserModel user)
        {
            // Delete all relations
            if (user.UserClaims != null && user.UserClaims.Count > 0)
                a3SContext.ApplicationUserClaims.RemoveRange(user.UserClaims);

            if (user.UserRoles != null && user.UserRoles.Count > 0)
                a3SContext.UserRole.RemoveRange(user.UserRoles);

            if (user.UserTeams != null && user.UserTeams.Count > 0)
                a3SContext.UserTeam.RemoveRange(user.UserTeams);


            // Mark user as deleted (rename username and email field to support creating a new record in the future)
            user.IsDeleted = true;
            user.DeletedTime = DateTime.UtcNow;
            user.UserName = $"{user.UserName} - Deleted";
            user.Email = $"{user.Email} - Deleted";
            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.Email.ToUpper();
            await UpdateAsync(user);
            await a3SContext.SaveChangesAsync();
        }

        public async Task<UserModel> GetByIdAsync(Guid userId, bool includeRelations)
        {
            // Note: Identity User ID type is modelled as a string, even though it is a Guid. This is a quirk of .Net Identity and it's DB models.
            if (includeRelations)
            {
                return await a3SContext.User.Where(u => u.Id == userId.ToString())
                                            .Include(u => u.UserRoles)
                                              .ThenInclude(ur => ur.Role)
                                                .ThenInclude(r => r.RoleFunctions)
                                                  .ThenInclude(rf => rf.Function)
                                            .Include(u => u.UserRoles)
                                              .ThenInclude(ur => ur.Role)
                                                .ThenInclude(r => r.ChildRoles)
                                                  .ThenInclude(cr => cr.ChildRole)
                                            .Include(u => u.UserTeams)
                                              .ThenInclude(ut => ut.Team)
                                            .Include(u => u.UserTokens)
                                            .Include(u => u.Profiles)
                                            .FirstOrDefaultAsync();
            }

            return await a3SContext.User.FindAsync(userId.ToString());
        }

        public async Task<UserModel> GetByUsernameAsync(string username, bool includeRelations)
        {
            if (includeRelations)
            {
                return await a3SContext.User.Where(u => u.UserName == username)
                                            .Include(u => u.UserRoles)
                                              .ThenInclude(ur => ur.Role)
                                                .ThenInclude(r => r.RoleFunctions)
                                                  .ThenInclude(rf => rf.Function)
                                            .Include(u => u.UserRoles)
                                              .ThenInclude(ur => ur.Role)
                                                .ThenInclude(r => r.ChildRoles)
                                                  .ThenInclude(cr => cr.ChildRole)
                                            .Include(u => u.UserTeams)
                                              .ThenInclude(ut => ut.Team)
                                            .Include(u => u.UserTokens)
                                            .Include(u => u.Profiles)
                                            .FirstOrDefaultAsync();
            }

            return await a3SContext.User.Where(u => u.UserName == username).FirstOrDefaultAsync();
        }

        public async Task<List<UserModel>> GetListAsync()
        {
            return await a3SContext.User.Where(x => !x.IsDeleted)
                                         .Include(u => u.UserRoles)
                                          .ThenInclude(ur => ur.Role)
                                            .ThenInclude(r => r.RoleFunctions)
                                              .ThenInclude(rf => rf.Function)
                                         .Include(u => u.UserRoles)
                                            .ThenInclude(ur => ur.Role)
                                              .ThenInclude(r => r.ChildRoles)
                                                .ThenInclude(cr => cr.ChildRole)
                                        .Include(u => u.UserTeams)
                                          .ThenInclude(ut => ut.Team)
                                        .Include(u => u.UserTokens)
                                        .Include(u => u.Profiles)
                                        .ToListAsync();
        }

        public async Task<UserModel> UpdateAsync(UserModel user)
        {

            a3SContext.Entry(user).State = EntityState.Modified;
            await a3SContext.SaveChangesAsync();

            return user;
        }

        public async Task ChangePassword(Guid userId, string oldPassword, string newPassword)
        {
            UserModel userModel = await a3SContext.User.FindAsync(userId.ToString());

            if (!await identityUserManager.CheckPasswordAsync(userModel, oldPassword))
                throw new ItemNotFoundException("Invalid userId or password specified.");

            string resetToken = await identityUserManager.GeneratePasswordResetTokenAsync(userModel);
            IdentityResult passwordChangeResult = await identityUserManager.ResetPasswordAsync(userModel, resetToken, newPassword);

            if (!passwordChangeResult.Succeeded)
                throw new OperationFailedException(string.Join(". ", passwordChangeResult.Errors));
        }
    }
}
