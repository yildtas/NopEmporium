using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Plugin.Api.DTOs;
using System.Xml;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Services.Media;
using Nop.Plugin.Api.Converters;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Services.Localization;

namespace Nop.Plugin.Api.Services
{
    public class ProductAttributeConverter : IProductAttributeConverter
    {
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        public ProductAttributeConverter(IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IDownloadService downloadService,
            IApiTypeConverter apiTypeConverter,
            ILocalizationService localizationService)
        {
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _downloadService = downloadService;
            _localizationService = localizationService;
        }
        protected virtual string GetProductAttributesXml(Product product, IFormCollection form, List<string> errors)
        {
            var attributesXml = string.Empty;
            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            foreach (var attribute in productAttributes)
            {
                var controlId = $"{"product_attribute_"}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                {
                                    //get quantity entered by customer
                                    var quantity = 1;
                                    var quantityStr = form[$"{"product_attribute_"}{attribute.Id}_{selectedAttributeId}_qty"];
                                    if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                        (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                        errors.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));

                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.ToString()
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                    {
                                        //get quantity entered by customer
                                        var quantity = 1;
                                        var quantityStr = form[$"{"product_attribute_"}{attribute.Id}_{item}_qty"];
                                        if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                            (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                            errors.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));

                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                    }
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                //get quantity entered by customer
                                var quantity = 1;
                                var quantityStr = form[$"{"product_attribute_"}{attribute.Id}_{selectedAttributeId}_qty"];
                                if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                    (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                    errors.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));

                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                            }
                            catch
                            {
                            }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid.TryParse(form[controlId], out Guid downloadGuid);
                            var download = _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }
            return attributesXml;
        }

        public string ConvertToXml(List<ProductItemAttributeDto> attributeDtos, int productId)
        {
            var attributesXml = "";

            if (attributeDtos == null)
                return attributesXml;

            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(productId);
            foreach (var attribute in productAttributes)
            {
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            // there should be only one selected value for this attribute
                            var selectedAttribute = attributeDtos.Where(x => x.Key == attribute.Id).FirstOrDefault();
                            if (selectedAttribute != null)
                            {
                                int selectedAttributeValue;
                                var isInt = int.TryParse(selectedAttribute.Value, out selectedAttributeValue);
                                if (isInt && selectedAttributeValue > 0)
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeValue.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            // there could be more than one selected value for this attribute
                            var selectedAttributes = attributeDtos.Where(x => x.Key == attribute.Id);
                            foreach (var selectedAttribute in selectedAttributes)
                            {
                                int selectedAttributeValue;
                                var isInt = int.TryParse(selectedAttribute.Value, out selectedAttributeValue);
                                if (isInt && selectedAttributeValue > 0)
                                {
                                    // currently there is no support for attribute quantity
                                    var quantity = 1;

                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeValue.ToString(), quantity);
                                }

                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only(already server - side selected) values
                            var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                     attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var selectedAttribute = attributeDtos.Where(x => x.Key == attribute.Id).FirstOrDefault();

                            if (selectedAttribute != null)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttribute.Value);
                            }

                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var selectedAttribute = attributeDtos.Where(x => x.Key == attribute.Id).FirstOrDefault();

                            if (selectedAttribute != null)
                            {
                                DateTime selectedDate;

                                // Since nopCommerce uses this format to keep the date in the database to keep it consisten we will expect the same format to be passed
                                var validDate = DateTime.TryParseExact(selectedAttribute.Value, "D", CultureInfo.CurrentCulture,
                                                       DateTimeStyles.None, out selectedDate);

                                if (validDate)
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedDate.ToString("D"));
                                }
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            var selectedAttribute = attributeDtos.Where(x => x.Key == attribute.Id).FirstOrDefault();

                            if (selectedAttribute != null)
                            {
                                Guid downloadGuid;
                                Guid.TryParse(selectedAttribute.Value, out downloadGuid);
                                var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, download.DownloadGuid.ToString());
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            // No Gift Card attributes support yet

            return attributesXml;
        }

        public List<ProductItemAttributeDto> Parse(string attributesXml)
        {
            var attributeDtos = new List<ProductItemAttributeDto>();
            if (string.IsNullOrEmpty(attributesXml))
                return attributeDtos;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                foreach (XmlNode attributeNode in xmlDoc.SelectNodes(@"//Attributes/ProductAttribute"))
                {
                    if (attributeNode.Attributes != null && attributeNode.Attributes["ID"] != null)
                    {
                        int attributeId;
                        if (int.TryParse(attributeNode.Attributes["ID"].InnerText.Trim(), out attributeId))
                        {
                            foreach (XmlNode attributeValue in attributeNode.SelectNodes("ProductAttributeValue"))
                            {
                                var value = attributeValue.SelectSingleNode("Value").InnerText.Trim();
                                // no support for quantity yet
                                //var quantityNode = attributeValue.SelectSingleNode("Quantity");
                                attributeDtos.Add(new ProductItemAttributeDto { Key = attributeId, Value = value });
                            }
                        }
                    }
                }
            }
            catch { }

            return attributeDtos;
        }
    }
}
