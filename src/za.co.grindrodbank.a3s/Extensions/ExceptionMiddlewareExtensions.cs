/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
﻿using System;
using System.Net;
using za.co.grindrodbank.a3s.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using NLog;
using za.co.grindrodbank.a3s.A3SApiResources;
using System.Linq;

namespace GlobalErrorHandling.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    Console.WriteLine("In exception handler.");
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if (contextFeature == null)
                    {
                        await context.Response.WriteAsync(new ErrorResponse()
                        {
                            Message = "Internal Server Error."
                        }.ToJson());

                        return;
                    }

                    WriteException(contextFeature.Error);

                    // Check for a YAML structure error
                    if (contextFeature.Error is YamlDotNet.Core.SyntaxErrorException || contextFeature.Error is YamlDotNet.Core.YamlException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                        await context.Response.WriteAsync(new ErrorResponse()
                        {
                            Message = "Error De-serialising YAML."
                        }.ToJson());

                        return;
                    }

                    // Check for a Item not found error
                    if (contextFeature.Error is ItemNotFoundException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                        await context.Response.WriteAsync(new ErrorResponse()
                        {
                            Message = contextFeature.Error.Message
                        }.ToJson());

                        return;
                    }

                    // Check for a Item not processable error
                    if (contextFeature.Error is ItemNotProcessableException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

                        await context.Response.WriteAsync(new ErrorResponse()
                        {
                            Message = contextFeature.Error.Message
                        }.ToJson());

                        return;
                    }

                    // Check for a Invalid Format Exception error
                    if (contextFeature.Error is InvalidFormatException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                        await context.Response.WriteAsync(new ErrorResponse()
                        {
                            Message = contextFeature.Error.Message
                        }.ToJson());

                        return;
                    }

                    // Check for a ItemNotProcessableException - This is not really an exception, just an always roll back dry run.
                    if (contextFeature.Error is SecurityContractDryRunException)
                    {
                        Console.WriteLine("Processing Security contract validation exception.");
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        var validationErrorResult = new SecurityContractValidationResult
                        {
                            Message = "No Errors Detected - Security contract OK."
                        };

                        if (((SecurityContractDryRunException)contextFeature.Error).validationErrors.Any())
                        {
                            validationErrorResult.Message = "Application of Security Contract Throws Errors.";

                            foreach (var validationError in ((SecurityContractDryRunException)contextFeature.Error).validationErrors)
                            {
                                validationErrorResult.ValidationErrors.Add(new SecurityContractValidationError
                                {
                                    ErrorType = "Contract Error",
                                    Message = validationError
                                });
                            }
                        }

                        await context.Response.WriteAsync(validationErrorResult.ToJson());

                        return;
                    }

                    // Default 500 Catch All
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    await context.Response.WriteAsync(new ErrorResponse()
                    {
                        Message = "Internal Server Error."
                    }.ToJson());
                });
            });
        }

        public static void WriteException(Exception exception)
        {
            logger.Error(exception);
        }
    }
}