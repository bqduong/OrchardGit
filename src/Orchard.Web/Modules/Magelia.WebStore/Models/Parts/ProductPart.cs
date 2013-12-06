using Magelia.WebStore.Models.Records;
using Orchard.ContentManagement;
using System;

namespace Magelia.WebStore.Models.Parts
{
    public class ProductPart : ContentPart<ProductPartRecord>
    {
        public String CatalogCode
        {
            get
            {
                return this.Record.CatalogCode;
            }
            set
            {
                this.Record.CatalogCode = value;
            }
        }

        public String SKU
        {
            get
            {
                return this.Record.SKU;
            }
            set
            {
                this.Record.SKU = value;
            }
        }

        public Boolean FromUrl
        {
            get
            {
                return this.Record.FromUrl;
            }
            set
            {
                this.Record.FromUrl = value;
            }
        }

        public String CatalogCodeUrlParameterKey
        {
            get
            {
                return this.Record.CatalogCodeUrlParameterKey;
            }
            set
            {
                this.Record.CatalogCodeUrlParameterKey = value;
            }
        }

        public String SKUUrlParameterKey
        {
            get
            {
                return this.Record.SKUUrlParameterKey;
            }
            set
            {
                this.Record.SKUUrlParameterKey = value;
            }
        }

        public Boolean AllowAddToBasket
        {
            get
            {
                return this.Record.AllowAddToBasket;
            }
            set
            {
                this.Record.AllowAddToBasket = value;
            }
        }
    }
}