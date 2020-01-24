/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using za.co.grindrodbank.a3s.A3SApiResources;
using za.co.grindrodbank.a3s.Exceptions;
using za.co.grindrodbank.a3s.Extensions;
using za.co.grindrodbank.a3s.Managers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;

namespace za.co.grindrodbank.a3s.Services
{
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly IUserRepository userRepository;
        private readonly CustomUserManager userManager;
        private readonly TokenOptions tokenOptions = new TokenOptions();

        public TwoFactorAuthService(IUserRepository userRepository, CustomUserManager userManager)
        {
            this.userRepository = userRepository;
            this.userManager = userManager;
        }

        public async Task RemoveTwoFactorAuthenticationAsync(Guid userId)
        {
            // Start transactions to allow complete rollback in case of an error
            BeginAllTransactions();

            try
            {
                UserModel userModel = await userRepository.GetByIdAsync(userId, true);
                if (userModel == null)
                    throw new ItemNotFoundException($"User with Id '{userId}' not found while attempting to update a user using this Id.");

                // Remove all two-factor references and tokens
                userModel.TwoFactorEnabled = false;

                if (userModel.UserTokens != null)
                {
                    userModel.UserTokens.RemoveAll(x =>
                            x.Name == tokenOptions.GetAuthenticatorKeyName() ||
                            x.Name == tokenOptions.GetRecoverCodesName()
                        );
                }

                await userRepository.UpdateAsync(userModel);

                // All successful
                CommitAllTransactions();
             }
            catch
            {
                RollbackAllTransactions();
                throw;
            }
        }

        public async Task<ValidationResultResponse> ValidateTwoFactorAuthenticationOTPAsync(TwoFactorAuthOTP twoFactorAuthOTP)
        {
            var response = new ValidationResultResponse()
            {
                Messages = new List<string>()
            };

            UserModel user = await userRepository.GetByIdAsync(twoFactorAuthOTP.UserId, true);
            if (user == null)
                throw new ItemNotFoundException($"User with Id '{twoFactorAuthOTP.UserId}' not found while attempting to validate user OTP.");

            // Confirm that this user has a valid authenticator registered
            if (!userManager.IsAuthenticatorTokenVerified(user))
            {
                response.Success = false;
                return response;
            }

            twoFactorAuthOTP.OTP = twoFactorAuthOTP.OTP.Replace(" ", string.Empty).Replace("-", string.Empty);
            response.Success = await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, twoFactorAuthOTP.OTP);

            return response;
        }

        private void BeginAllTransactions()
        {
            userRepository.InitSharedTransaction();
        }

        private void CommitAllTransactions()
        {
            userRepository.CommitTransaction();
        }

        private void RollbackAllTransactions()
        {
            userRepository.RollbackTransaction();
        }
    }
}
