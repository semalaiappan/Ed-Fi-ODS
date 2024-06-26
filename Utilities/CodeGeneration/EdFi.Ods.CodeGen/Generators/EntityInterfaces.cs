// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EdFi.Common.Extensions;
using EdFi.Ods.CodeGen.Extensions;
using EdFi.Ods.CodeGen.Models;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Conventions;
using EdFi.Ods.Common.Models.Domain;
using EdFi.Ods.Common.Models.Resource;
using EdFi.Ods.Common.Specifications;

namespace EdFi.Ods.CodeGen.Generators
{
    public class EntityInterfaces : GeneratorBase
    {
        private IPersonEntitySpecification _personEntitySpecification;

        protected override object Build()
        {
            var domainModel = TemplateContext.DomainModelProvider.GetDomainModel();

            _personEntitySpecification = 
                new PersonEntitySpecification(
                    new PersonTypesProvider(
                        new SuppliedDomainModelProvider(domainModel)));

            var resourceClassesToRender = ResourceModelProvider.GetResourceModel()
                .GetAllResources()
                .SelectMany(
                    r => r.AllContainedItemTypesOrSelf.Where(
                        i => TemplateContext.ShouldRenderResourceClass(i)

                             // Don't render artifacts for base class children in the context of derived resources
                             && !i.IsInheritedChildItem()))
                .OrderBy(r => r.Name)
                .ToList();

            var entityInterfacesModel = new
            {
                EntitiesBaseNamespace =
                    EdFiConventions.BuildNamespace(
                        Namespaces.Entities.Common.BaseNamespace,
                        TemplateContext.SchemaProperCaseName),
                HasExtensionDerivedFromEdFiBaseEntity = resourceClassesToRender
                    .Where(TemplateContext.ShouldRenderResourceClass)
                    .Where(a => a.Entity != null)
                    .Select(a => a.Entity)
                    .Any(e => !e.IsEdFiStandardEntity && e.IsDerived && e.BaseEntity.IsEdFiStandardEntity),
                Interfaces = resourceClassesToRender
                    .Where(TemplateContext.ShouldRenderResourceClass)
                    .Select(
                        r => new
                        {
                            Schema = r.FullName.Schema,
                            ModelName = r.Name,
                            AggregateName = r.Name,
                            ImplementedInterfaces = GetImplementedInterfaceString(r),
                            ParentInterfaceName = GetParentInterfaceName(r),
                            ParentClassName = GetParentClassName(r),
                            InheritedIdentifyingProperties = r
                                .IdentifyingProperties
                                // Include names of inherited, non-renamed, identifying properties
                                .Where(IsInheritedNonRenamedIdentifyingProperty)
                                .Select(p => new { p.PropertyName }),
                            IdentifyingProperties = r
                                .IdentifyingProperties

                                // Exclude inherited identifying properties where the property has not been renamed
                                .Where(p => !IsInheritedNonRenamedIdentifyingProperty(p))
                                .OrderBy(p => p.PropertyName)
                                .Select(
                                    p => new
                                    {
                                        IsServerAssigned = p.IsServerAssigned,
                                        IsUniqueId = UniqueIdConventions.IsUniqueId(p.PropertyName)
                                            && _personEntitySpecification.IsPersonEntity(r.Name),
                                        IsLookup = p.IsDescriptorUsage,
                                        CSharpType = p.PropertyType.ToCSharp(false),
                                        PropertyName = p.PropertyName,
                                        CSharpSafePropertyName = p.PropertyName.MakeSafeForCSharpClass(r.Name),
                                        LookupName = p.PropertyName
                                    })
                                .ToList(),
                            IsDerived = r.IsDerived,
                            InheritedNonIdentifyingProperties = r.IsDerived
                                ? r.AllProperties
                                    .Where(p => p.IsInherited && !p.IsIdentifying)
                                    .OrderBy(p => p.PropertyName)
                                    .Where(IsModelInterfaceProperty)
                                    .Select(
                                        p =>
                                            new
                                            {
                                                IsLookup = p.IsDescriptorUsage,
                                                CSharpType = p.PropertyType.ToCSharp(true),
                                                PropertyName = p.PropertyName,
                                                LookupName = p.PropertyName.TrimSuffix("Id")
                                            })
                                    .ToList()
                                : null,
                            NonIdentifyingProperties = r.NonIdentifyingProperties
                                .Where(p => !p.IsInherited)
                                .OrderBy(p => p.PropertyName)
                                .Where(IsModelInterfaceProperty)
                                .Select(
                                    p =>
                                        new
                                        {
                                            IsLookup = p.IsDescriptorUsage,
                                            CSharpType = p.PropertyType.ToCSharp(true),
                                            PropertyName = p.PropertyName,
                                            CSharpSafePropertyName = p.PropertyName.MakeSafeForCSharpClass(r.Name),
                                            LookupName = p.PropertyName.TrimSuffix("Id")
                                        })
                                .ToList(),
                            HasNavigableOneToOnes = r.EmbeddedObjects.Any(),
                            NavigableOneToOnes = r
                                .EmbeddedObjects
                                .Where(eo => !eo.IsInherited)
                                .OrderBy(eo => eo.PropertyName)
                                .Select(
                                    eo
                                        => new
                                        {
                                            ItemTypeName = eo.ObjectType.Name,
                                            PropertyName = eo.PropertyName
                                        })
                                .ToList(),
                            InheritedLists = r.IsDerived
                                ? r.Collections
                                    .Where(c => c.IsInherited)
                                    .OrderBy(c => c.PropertyName)
                                    .Select(
                                        c => new
                                        {
                                            ItemTypeName = c.ItemType.Name,
                                            PropertyName = c.PropertyName
                                        })
                                    .ToList()
                                : null,
                            Lists = r.Collections
                                .Where(c => !c.IsInherited)
                                .OrderBy(c => c.PropertyName)
                                .Select(
                                    c => new
                                    {
                                        ItemTypeName = c.ItemType.Name,
                                        PropertyName = c.PropertyName
                                    })
                                .ToList(),
                            HasDiscriminator = r.Entity?.HasDiscriminator() ?? false,
                            AggregateReferences =
                                r.Entity?.GetAssociationsToReferenceableAggregateRoots()
                                    .OrderBy(a => a.Name)
                                    .Select(
                                        a => new
                                        {
                                            AggregateReferenceName = a.Name,
                                            MappedReferenceDataHasDiscriminator =
                                                a.OtherEntity.HasDiscriminator()
                                        })
                                    .ToList(),
                            HasChildMappingContractMembers = r.EmbeddedObjects.Any() || r.Collections.Any(),
                            MappingContractMembers = GetMappingContractMembers(r),
                            IsExtendable = r.IsExtendable()
                        })
                    .ToList()
            };

            return entityInterfacesModel;
        }

        private static bool IsInheritedNonRenamedIdentifyingProperty(ResourceProperty p) => (
            p.EntityProperty
                ?.IsInheritedIdentifying ==
            true
            && !p
                .EntityProperty
                ?.IsInheritedIdentifyingRenamed ==
            true);

        private static string GetParentClassName(ResourceClassBase resourceClass)
        {
            return (resourceClass as ResourceChildItem)?.Parent.Name;
        }

        private static string GetParentInterfaceName(ResourceClassBase resourceClass)
        {
            string parentClassName = GetParentClassName(resourceClass);

            if (parentClassName == null)
            {
                return null;
            }

            var resourceChildItem = (ResourceChildItem) resourceClass;

            // If this resource class represents an "Entity Extension"
            if (resourceChildItem.Parent.Extensions.Any(
                x => ModelComparers.Resource.Equals(resourceChildItem, x.ObjectType)))
            {
                return $"{EdFiConventions.ProperCaseName}.I{parentClassName}";
            }

            return $"I{parentClassName}";
        }

        private string GetImplementedInterfaceString(ResourceClassBase resourceClass)
        {
            var interfaceStringBuilder = new StringBuilder();

            if (resourceClass.Entity?.IsDerived == true)
            {
                AddInterface(
                    $"{resourceClass.Entity.BaseEntity.SchemaProperCaseName()}.I{resourceClass.Entity.BaseEntity.Name}",
                    interfaceStringBuilder);
            }

            if (resourceClass.Entity?.IsAbstractRequiringNoCompositeId() != true)
            {
                AddInterface("ISynchronizable", interfaceStringBuilder);
                AddInterface("IMappable", interfaceStringBuilder);
            }

            // We want to exclude base concrete classes, descriptors and types from extensions.
            if (resourceClass.IsExtendable())
            {
                AddInterface("IHasExtensions", interfaceStringBuilder);
            }

            if (resourceClass is Resource)
            {
                AddInterface("IHasIdentifier", interfaceStringBuilder);
            }

            if (resourceClass.Properties.Where(p => p.IsIdentifying)
                .Any(p => UniqueIdConventions.IsUniqueId(p.PropertyName) && p.IsLocallyDefined))
            {
                AddInterface("IIdentifiablePerson", interfaceStringBuilder);
            }

            AddInterface("IGetByExample", interfaceStringBuilder);

            return interfaceStringBuilder.ToString();
        }

        private void AddInterface(string interfaceName, StringBuilder interfaceStringBuilder)
        {
            if (interfaceStringBuilder.Length > 0)
            {
                interfaceStringBuilder.Append(", " + interfaceName);
            }
            else
            {
                interfaceStringBuilder.Append(" : " + interfaceName);
            }
        }

        private bool IsModelInterfaceProperty(ResourceProperty p)
        {
            return !new[]
            {
                "Id",
                "LastModifiedDate",
                "CreateDate"
            }.Contains(p.PropertyName);
        }

        private readonly ConcurrentDictionary<FullName, IEnumerable<object>> _mappingContractMembersByFullName = new();

        private IEnumerable<object> GetMappingContractMembers(ResourceClassBase resourceClass)
        {
            return _mappingContractMembersByFullName.GetOrAdd(resourceClass.FullName,
                static (fn, args) =>
                {
                    var rc = args.resourceClass;
                    var personEntitySpecification = args.personEntitySpecification;
                    
                    var properties = rc.AllProperties

                        // Don't include properties that are not synchronizable
                        .Where(p => p.IsSynchronizedProperty())

                        // Don't include identifying properties, with the exception of where UniqueIds are defined
                        .Where(p => !p.IsIdentifying || personEntitySpecification.IsDefiningUniqueId(rc, p))
                        .Select(p => p.PropertyName)

                        // Add embedded object properties
                        .Concat(
                            rc.EmbeddedObjects.Cast<ResourceMemberBase>()
                                .Concat(rc.References)
                                .Concat(rc.Extensions)
                                .Concat(rc.Collections)
                                .Select(rc => rc.PropertyName))
                        .Distinct()
                        .OrderBy(pn => pn)
                        .Select(pn => new
                        {
                            PropertyName = pn,
                            ItemTypeName = null as string,
                            IsCollection = false,
                        });

                    var embeddedObjects = rc.EmbeddedObjects
                        .OrderBy(c => c.ObjectType.Name)
                        .Select(c => new
                        {
                            PropertyName = c.PropertyName,
                            ItemTypeName = c.ObjectType.Name,
                            IsCollection = false,
                        });

                    var collections = rc.Collections
                        .OrderBy(c => c.ItemType.Name)
                        .Select(c => new
                        {
                            PropertyName = c.PropertyName,
                            ItemTypeName = c.ItemType.Name,
                            IsCollection = true,
                        });

                    var members = properties
                        .Concat(embeddedObjects)
                        .Concat(collections)
                        .ToList();
                
                    return members.Select(
                        x =>
                            new
                            {
                                PropertyName = x.PropertyName,
                                ItemTypeName = x.ItemTypeName,
                                IsCollection = x.IsCollection,
                                IsLast = x == members.Last() && !rc.IsExtendable()
                            });
                        }, 
                (resourceClass: resourceClass, personEntitySpecification: _personEntitySpecification));
        }
    }
}
