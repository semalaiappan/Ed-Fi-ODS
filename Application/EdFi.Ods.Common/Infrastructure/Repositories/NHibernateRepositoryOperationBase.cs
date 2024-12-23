// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using NHibernate;

namespace EdFi.Ods.Common.Infrastructure.Repositories
{
    /// <summary>
    /// Provides session management for NHibernate-based repository implementations.
    /// </summary>
    public abstract class NHibernateRepositoryOperationBase
    {
        protected readonly ISessionFactory SessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateRepositoryOperationBase"/> class using the specified NHibernate session factory.
        /// </summary>
        /// <param name="sessionFactory"></param>
        protected NHibernateRepositoryOperationBase(ISessionFactory sessionFactory)
        {
            SessionFactory = sessionFactory;
        }

        /// <summary>
        /// Gets the current session (should wrap usage in a new <see cref="SessionScope"/> instance before accessing this property).
        /// </summary>
        protected ISession Session
        {
            get => SessionFactory.GetCurrentSession();
        }
    }
}
