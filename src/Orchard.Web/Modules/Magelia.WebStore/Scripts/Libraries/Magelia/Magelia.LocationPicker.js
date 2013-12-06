Magelia.LocationPicker = function (container, config) {
    this.container = container;
    this.config = config;
    this.initialize();
};

Magelia.LocationPicker.prototype = {
    initialize: function () {
        this.countryField = $(this.config.countryFieldSelector, this.container);
        this.regionField = $(this.config.regionFieldSelector, this.container);
        this.setEventHandlers();
    },

    setEventHandlers: function () {
        var locationChangedProxy = $.proxy(this.locationChanged, this);
        this.countryField.change(locationChangedProxy);
        this.regionField.change(locationChangedProxy);
        $(Magelia).one('locationChanged', $.proxy(this.updateLocation, this));
    },

    updateLocation: function () {
        if (this.config.locationPickerUrl) {
            $.ajax(
                {
                    type: 'get',
                    url: this.config.locationPickerUrl,
                    success: $.proxy(
                        function (html) {
                            var content = $.parseHTML(html);
                            var locationPicker = $(content).insertAfter(this.container);
                            Magelia.Helpers.removeWrapper(locationPicker);
                            this.container.remove();
                        },
                        this
                    )
                }
            );
        }
    },

    locationChanged: function () {
        Magelia.Helpers.submitData(this.container, this.config.updateLocationUrl, true);
    }
};