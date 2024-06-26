﻿// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Extensions;
using EdFi.Ods.Common.Infrastructure.Pipelines;
using EdFi.Ods.Common.Validation;

namespace EdFi.Ods.Api.Infrastructure.Pipelines.Steps
{
    public class ValidateResourceModel<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : IHasResource<TResourceModel>
        where TResourceModel : IHasETag
        where TEntityModel : class
        where TResult : PipelineResultBase
    {
        private readonly IEnumerable<IResourceValidator> _validators;

        public ValidateResourceModel(IEnumerable<IResourceValidator> validators)
        {
            _validators = validators;
        }

        public Task ExecuteAsync(TContext context, TResult result, CancellationToken cancellationToken)
        {
            var validationResults = _validators.ValidateObject(context.Resource);

            if (!validationResults.IsValid())
            {
                result.ValidationResults ??= new List<ValidationResult>();
                result.ValidationResults.AddRange(validationResults);
            }
            
            return Task.CompletedTask;
        }
    }
}
