// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using EdFi.Ods.Common.Models.Domain;

namespace EdFi.Ods.Common.Providers.Queries;

public static class EntityExtensions
{
    private const string BaseAlias = "b";
    private const string StandardAlias = "r";

    /// <summary>
    /// Determines the appropriate table alias for the given aggregate root entity.
    /// </summary>
    /// <param name="aggregateRootEntity">The aggregate root entity to determine the alias for.</param>
    /// <returns>
    /// Returns "b" if the entity is derived, otherwise returns "r".
    /// </returns>
    public static string RootTableAlias(this Entity aggregateRootEntity)
    {
        return aggregateRootEntity.IsDerived ? BaseAlias : StandardAlias;
    }
}