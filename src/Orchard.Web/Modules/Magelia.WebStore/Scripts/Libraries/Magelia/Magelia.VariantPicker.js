Magelia.VariantPicker = function (container, config) {
    this.container = container;
    this.config = config;
    this.initialize();
};

Magelia.VariantPicker.prototype = {
    initialize: function () {
        this.variantField = $(this.config.variantFieldSelector, this.container);
        this.setEventHandler();
    },

    setEventHandler: function () {
        this.variantField.change($.proxy(this.variantChanged, this));
    },

    variantChanged: function () {
        Magelia.Helpers.submitData(this.container, '', false);
    }
};