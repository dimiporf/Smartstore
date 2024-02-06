using System;
using System.Threading;
using System.Threading.Tasks;
using MyOrg.HelloWorld.Components;
using MyOrg.HelloWorld.Models;
using Smartstore;
using Smartstore.Core;
using Smartstore.Core.Catalog.Pricing;
using Smartstore.Core.Catalog.Products;
using Smartstore.Core.Common;
using Smartstore.Core.DataExchange;
using Smartstore.Core.DataExchange.Export;
using Smartstore.Core.Localization;
using Smartstore.Core.Platform.DataExchange.Export;
using Smartstore.Core.Widgets;
using Smartstore.Engine.Modularity;

namespace MyOrg.HelloWorld.Providers
{
    [SystemName("MyOrg.HelloWorld.ProductXml")]
    [FriendlyName("HelloWorld XML product feed")]
    [ExportFeatures(Features =
        ExportFeatures.CreatesInitialPublicDeployment |
        ExportFeatures.OffersBrandFallback)]
    public class HelloWorldXmlExportProvider : ExportProviderBase
    {
        private readonly IWorkContext _workContext;

        public HelloWorldXmlExportProvider(IWorkContext workContext)
        {
            _workContext = workContext;
        }
        public static string SystemName => "MyOrg.HelloWorld.ProductXml";

        public override string FileExtension => "XML";

        public Localizer T { get; set; } = NullLocalizer.Instance;

        public override ExportConfigurationInfo ConfigurationInfo => new()
        {
            ConfigurationWidget = new ComponentWidget<HelloWorldConfigurationViewComponent>(),
            ModelType = typeof(ProfileConfigurationModel)
        };

        protected override async Task ExportAsync(ExportExecuteContext context, CancellationToken cancelToken)
        {
            var config = (context.ConfigurationData as ProfileConfigurationModel) ?? new ProfileConfigurationModel();
            var currency = _workContext.WorkingCurrency;

            using var helper = new ExportXmlHelper(context.DataStream);
            var writer = helper.Writer;

            writer.WriteStartDocument();
            writer.WriteStartElement("products");

            while (context.Abort == DataExchangeAbortion.None && await context.DataSegmenter.ReadNextSegmentAsync())
            {
                var segment = await context.DataSegmenter.GetCurrentSegmentAsync();

                foreach (dynamic product in segment)
                {
                    if (context.Abort != DataExchangeAbortion.None)
                    {
                        break;
                    }

                    Product entity = product.Entity;

                    writer.WriteStartElement("product");

                    try
                    {
                        var calculatedPrice = (CalculatedPrice)product._Price;
                        var saving = calculatedPrice.Saving;
                        var productDisplayedPrice = new Money(product.Price, currency);
                        var productDisplayedSavings = new Money(saving.SavingPrice.Amount, currency);

                        writer.WriteElementString("product-name", (string)product.Name);
                        writer.WriteElementString("sku", (string)product.Sku);
                        writer.WriteElementString("price", productDisplayedPrice.ToString());

                        if (saving.HasSaving)
                        {
                            writer.WriteElementString("savings", productDisplayedSavings.ToString());
                        }

                        writer.WriteCData("desc", ((string)product.FullDescription).Truncate(5000));

                        context.RecordsSucceeded++;

                        if (context.RecordsSucceeded >= config.NumberOfExportedRows)
                        {
                            context.Abort = DataExchangeAbortion.Soft;
                        }
                    }
                    catch (OutOfMemoryException ex)
                    {
                        context.RecordOutOfMemoryException(ex, entity.Id, T);
                        context.Abort = DataExchangeAbortion.Hard;
                        throw;
                    }
                    catch (Exception ex)
                    {
                        context.RecordException(ex, entity.Id);
                    }

                    writer.WriteEndElement(); // product
                }
            }

            writer.WriteEndElement(); // products
            writer.WriteEndDocument();
        }
    }
}