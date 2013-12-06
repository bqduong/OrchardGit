using Magelia.WebStore.Models.Parts;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;

namespace Magelia.WebStore.Drivers
{
    public class BasketCountPartDriver : ContentPartDriver<BasketCountPart>
    {
        protected override String Prefix
        {
            get
            {
                return "Magelia_WebStore_BasketCount";
            }
        }

        protected override DriverResult Display(BasketCountPart part, String displayType, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_BasketCount",
                () => shapeHelper.Parts_BasketCount(BasketCountPartId: part.Id)
            );
        }

        protected override DriverResult Editor(BasketCountPart part, dynamic shapeHelper)
        {
            return this.ContentShape(
                "Parts_BasketCount_Edit",
                () => shapeHelper.EditorTemplate(
                    Model: part,
                    Prefix: this.Prefix,
                    TemplateName: "Parts/BasketCount"
                )
            );
        }

        protected override DriverResult Editor(BasketCountPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, this.Prefix, null, null);
            return this.Editor(part, shapeHelper);
        }
    }
}