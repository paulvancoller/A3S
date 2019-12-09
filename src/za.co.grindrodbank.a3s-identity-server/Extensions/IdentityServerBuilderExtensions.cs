/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using za.co.grindrodbank.a3sidentityserver.Exceptions;

namespace za.co.grindrodbank.a3sidentityserver.Extensions
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder LoadSigningCredentialFrom(this IIdentityServerBuilder builder, string path, string password, IHostingEnvironment Environment)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    builder.AddSigningCredential(new X509Certificate2(path, password));
                }
                else
                {
                    throw new KeyMaterialException($"No key material certificate found at path: {path}");
                }
            }
            else
            {

                if (Environment.IsDevelopment())
                {
                    builder.AddDeveloperSigningCredential();
                }
                else
                {
                    throw new KeyMaterialException("need to configure key material");
                }
            }

            return builder;
        }
    }
}
