angular
    .module("app", ['ngRoute'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/', {
                redirectTo: '/home/list'
            })
            .otherwise({
                redirectTo: '/'
            });
    }])
    .directive('ngRepeat', function () {
        return function ($scope, $element, $attrs) {
            if ($scope.$last) setTimeout(function () {
                $(".form-notification").on("click", function () {
                    $(this).prev().focus();
                });
            }, 0);
        };
    });