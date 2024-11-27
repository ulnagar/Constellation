(function (factory) {
    if (typeof define === 'function' && define.amd) {
        define(['jquery'], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory(require('jquery'));
    } else {
        factory(window.jQuery);
    }
}(function ($) {
    //$.extend($.summernote.options, {
    //    mailmerge: {
    //        menu: [
    //            [ 'Parent Name', '::parent_name::' ],
    //            [ 'Report Type', '::report_type::' ],
    //            [ 'Report Date', '::report_month::' ]
    //        ]
    //    }
    //});
    $.extend($.summernote.plugins, {
        'mailmerge': function (context) {
            var ui = $.summernote.ui,
                options = context.options;
            context.memo('button.mailmerge', function () {
                var keywordMap = new Map(options.mailmerge.menu);
                var list = Array.from(keywordMap.keys());

                var button = ui.buttonGroup([
                    ui.button({
                        className: 'dropdown-toggle',
                        contents: '<span>Mail Merge</span>',
                        tooltip: 'Mail Merge Fields',
                        data: {
                            toggle: 'dropdown'
                        }
                    }),
                    ui.dropdown({
                        className: 'dropdown-template',
                        items: list,
                        click: function (e) {
                            var $button = $(e.target);
                            var value = $button.data('value');
                            e.preventDefault();

                            context.invoke('editor.insertText', keywordMap.get(value));
                        }
                    })
                ]);
                return button.render();
            });
        }
    });
}));
