Magelia.Payment = function (container) {
    this.container = container;
    this.initialize();
};

Magelia.Payment.prototype = {
    initialize: function () {
        this.orderButton = $('.mag-order', this.form);
        this.setEventHandlers();
    },

    setEventHandlers: function () {
        this.orderButton.click($.proxy(this.triggerOrder, this));
    },

    triggerOrder: function () {
        this.container.trigger('order');
    }
};