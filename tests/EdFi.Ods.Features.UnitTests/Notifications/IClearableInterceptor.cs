// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Castle.DynamicProxy;
using EdFi.Ods.Common.Caching;

namespace EdFi.Ods.Features.UnitTests.Notifications;

public interface IClearableInterceptor : IInterceptor, IClearable {}
