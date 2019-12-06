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
using System.Collections.Generic;

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

                    // Check for a YAML structure error
                    if (contextFeature.Error is YamlDotNet.Core.SyntaxErrorException || contextFeature.Error is YamlDotNet.Core.YamlException)
                    {
                        WriteException(contextFeature.Error);
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                        await context.Response.WriteAsync(new ErrorResponse()
                        {
                            Message = $"Error De-serialising YAML: {contextFeature.Error.Message}"
                        }.ToJson());

                        return;
                    }

                    // Check for a Item not found error
                    if (contextFeature.Error is ItemNotFoundException)
                    {
                        WriteException(contextFeature.Error);
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
                        WriteException(contextFeature.Error);
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
                        WriteException(contextFeature.Error);
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                        await context.Response.WriteAsync(new ErrorResponse()
                        {
                            Message = contextFeature.Error.Message
                        }.ToJson());

                        return;
                    }

                    // Check for a SecurityContractDryRunException - This is not really an exception, just an always roll back dry run.
                    if (contextFeature.Error is SecurityContractDryRunException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;

                        var validationResult = new SecurityContractValidationResult
                        {
                            Message = "No Errors Detected - Security contract OK."
                        };

                        if (((SecurityContractDryRunException)contextFeature.Error).ValidationWarnings.Any())
                        {
                            validationResult.Message = "No Errors Detected - But there are some warnings.";
                            var validationWarningList = new List<SecurityContractValidationWarning>();

                            foreach (var validationWarning in ((SecurityContractDryRunException)contextFeature.Error).ValidationWarnings)
                            {
                                var newWarning = new SecurityContractValidationWarning
                                {
                                    Message = validationWarning
                                };

                                validationWarningList.Add(newWarning);
                            }

                            validationResult.ValidationWarnings = validationWarningList;
                        }

                        if (((SecurityContractDryRunException)contextFeature.Error).ValidationErrors.Any())
                        {
                            validationResult.Message = "There are errors within the security contract.";
                            var validationErrorList = new List<SecurityContractValidationError>();

                            foreach (var validationError in ((SecurityContractDryRunException)contextFeature.Error).ValidationErrors)
                            {
                                var newError = new SecurityContractValidationError
                                {
                                    Message = validationError
                                };

                                validationErrorList.Add(newError);
                            }

                            validationResult.ValidationErrors = validationErrorList;
                        }

                        await context.Response.WriteAsync(validationResult.ToJson());

                        return;
                    }

                    // Default 500 Catch All
                    WriteException(contextFeature.Error);
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