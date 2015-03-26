angular
    .module("app", ['ngRoute', 'ng-breadcrumbs'])
    .config(['$routeProvider', '$httpProvider', '$locationProvider', function ($routeProvider, $httpProvider, $locationProvider) {
        $locationProvider.html5Mode(true);

        $httpProvider.interceptors.push(['$q', '$location', function ($q, $location) {
            return {
                'responseError': function (response) {
                    if (response.status === 401)
                        $location.url('/app/signin');
                    return $q.reject(response);
                }
            };
        }]);

        $routeProvider
            .when('/', {
                redirectTo: '/app/list'
            })
            .otherwise({
                redirectTo: '/'
            });
    }])
    .controller("AppCtrl", ['$scope', '$http', '$location', 'breadcrumbs', function ($scope, $http, $location, breadcrumbs) {
        $scope.isAuth = isAuth;
        $scope.isPlugin = isPlugin;
        $scope.breadcrumbs = breadcrumbs;

        (function jsRegex() {
            for (var key in regex)
                regex[key] = new RegExp(regex[key]);

            $scope.regex = regex;
        })();

        $scope.signOut = function ($scope) {
            $http.post("/api/account/logout")
                .success(function () {
                    $scope.isAuth = false;
                    $location.path('/app/signin');
                })
                .error(function () {
                    $scope.status = "error";
                });
        };
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