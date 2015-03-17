angular
    .module("app", ['ngRoute'])
    .config(['$routeProvider', '$locationProvider', function ($routeProvider, $locationProvider) {
        $locationProvider.html5Mode(true);
        $routeProvider
            .when('/', {
                redirectTo: '/app/list'
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