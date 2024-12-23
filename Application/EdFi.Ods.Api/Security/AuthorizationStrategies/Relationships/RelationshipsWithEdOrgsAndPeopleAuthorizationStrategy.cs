// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Common.Models;

namespace EdFi.Ods.Api.Security.AuthorizationStrategies.Relationships
{
    [AuthorizationStrategyName(RelationshipAuthorizationStrategyName)]
    public class RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy
        : RelationshipsAuthorizationStrategyBase
    {
        private const string RelationshipAuthorizationStrategyName = "RelationshipsWithEdOrgsAndPeople";

        public RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy(IDomainModelProvider domainModelProvider)
            : base(domainModelProvider) { }

        protected override string AuthorizationStrategyName
        {
            get => RelationshipAuthorizationStrategyName;
        }

        protected override SubjectEndpoint[] GetAuthorizationSubjectEndpoints(
            IEnumerable<(string name, object value)> authorizationContextTuples)
        {
            return authorizationContextTuples
                .Select(nv => new SubjectEndpoint(nv))
                .ToArray();
        }
    }
}
