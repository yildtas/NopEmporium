using AutoMapper;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTOs.Categories;
using Nop.Plugin.Api.DTOs.CustomerRoles;
using Nop.Plugin.Api.DTOs.Languages;
using Nop.Plugin.Api.DTOs.ProductAttributes;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Plugin.Api.DTOs.SpecificationAttributes;
using Nop.Plugin.Api.MappingExtensions;

namespace Nop.Plugin.Api
{
    public class ApiMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public ApiMapperConfiguration()
        {

            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryDto, Category>();

            CreateMap<Language, LanguageDto>();

            CreateMap<CustomerRole, CustomerRoleDto>();

            CreateMap<Product, ProductDto>();

            CreateMap<ProductAttributeValue, ProductAttributeValueDto>();

            CreateMap<ProductAttribute, ProductAttributeDto>();

            CreateMap<ProductSpecificationAttribute, ProductSpecificationAttributeDto>();

            CreateMap<SpecificationAttribute, SpecificationAttributeDto>();
            CreateMap<SpecificationAttributeOption, SpecificationAttributeOptionDto>();
        }

        public int Order => 0;

        private new static void CreateMap<TSource, TDestination>()
        {
            AutoMapperApiConfiguration.MapperConfigurationExpression.CreateMap<TSource, TDestination>()
                .IgnoreAllNonExisting();
        }
    }
}
