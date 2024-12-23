// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using EdFi.Ods.Common.Security.Authorization;

namespace EdFi.Ods.Api.Security.AuthorizationStrategies.Relationships.Filters;

public class PostgresViewBasedSingleItemAuthorizationQuerySupport : IViewBasedSingleItemAuthorizationQuerySupport
{
    public string GetItemExistenceCheckSql(
        ViewBasedAuthorizationFilterDefinition filterDefinition,
        AuthorizationFilterContext filterContext)
    {
        // If we are processing a view-based authorization with no claim values to be applied
        if (filterContext.ClaimParameterName == null)
        {
            var sb = new StringBuilder();

            sb.Append($"SELECT 1 FROM auth.");
            sb.Append(filterDefinition.ViewName);
            sb.Append(" AS authvw WHERE ");

            for (int i = 0; i < filterDefinition.ViewTargetEndpointNames.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(" AND ");
                }

                sb.Append("authvw.");
                sb.Append(filterDefinition.ViewTargetEndpointNames[i]);
                sb.Append(" = @");
                sb.Append(filterContext.SubjectEndpointNames[i]);
            }

            return sb.ToString();
        }

        // Use literal IN clause approach
        var edOrgIdsList = filterContext.ClaimEndpointValues.Any()
            ? string.Join(',', filterContext.ClaimEndpointValues)
            : "NULL";

        return
            $"SELECT 1 FROM auth.{filterDefinition.ViewName} AS authvw WHERE authvw.{filterDefinition.ViewTargetEndpointName} = @{filterContext.SubjectEndpointName} AND authvw.{filterDefinition.ViewSourceEndpointName} IN ({edOrgIdsList})";  
    }

    public void ApplyEducationOrganizationIdClaimsToCommand(DbCommand cmd, IList<long> claimEducationOrganizationIds)
    {
        // Nothing to do -- using inline IN clause
    }
}
