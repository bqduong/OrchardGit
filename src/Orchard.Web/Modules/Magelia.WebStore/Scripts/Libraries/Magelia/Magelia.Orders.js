Magelia.Orders = function (container) {
    this.container = container;
    this.initialize();
};

Magelia.Orders.prototype = {
    initialize: function () {
        this.setEventHandlers();
    },

    setEventHandlers: function () {
        var refreshView = $.proxy(this.refreshView, this);
        $('a', this.container).each(
            function () {
                var link = $(this);
                var href = link.attr('href');
                link.attr({ href: 'javascript:void(0);' }).click(function () { refreshView(href); });
            }
        );
    },

    refreshView: function (url) {
        $.ajax(
            {
                url: url,
                type: 'get',
                success: $.proxy(
                    function (html) {
                        var content = $.parseHTML(html);
                        var orders = $(content).insertAfter(this.container);
                        Magelia.Helpers.removeWrapper(orders);
                        this.container.remove();
                    },
                    this
                )
            }
        );
    }
};