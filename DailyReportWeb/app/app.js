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
            .when('/app/welcome', {
                templateUrl: 'app/app.html',
                controller: 'AppCtrl'
            })
            .when('/', {
                redirectTo: '/app/welcome'
            })
            .otherwise({
                redirectTo: '/'
            });
    }])
    .controller("AppCtrl", ['$scope', '$http', '$location', 'breadcrumbs', function ($scope, $http, $location, breadcrumbs) {
        $scope.isAuth = isAuth;
        $scope.isPlugin = isPlugin;
        $scope.breadcrumbs = breadcrumbs;
        $scope.child = {};

        (function jsRegex() {
            for (var key in regex)
                regex[key] = new RegExp(regex[key]);

            $scope.regex = regex;
        })();

        var getAccurateTimeZoneSuggestion = function (result) {
            // this is a hack so that the time zone id will not be mistaken as being part of the URL
            var timeZoneId = result.time_zone.replace(/\//g, '-');
            $http.get("/api/timezone/" + timeZoneId)
                 .success(function (response) {
                     $scope.timeZoneList = response.timeZoneList;
                     $scope.timeZone = response.suggestedTimeZone;
                 })
                .error(function () {
                    getEstimatedTimeZoneSuggestion();
                });
        };

        var getEstimatedTimeZoneSuggestion = function () {
            $http.get("/api/timezone")
                 .success(function (list) {
                     $scope.timeZoneList = list;
                     // represents the difference in minutes between UTC and the user's time zone (eg. for +2 a time zone it will be -120)
                     // this method does not take into account daylight saving  
                     var timeZoneOffset = new Date().getTimezoneOffset();
                     var closestTimeZone = $.grep($scope.timeZoneList, function (element) { return element.utcOffset == -timeZoneOffset })[0];
                     if (closestTimeZone)
                         $scope.timeZone = closestTimeZone.id;
                 });
        }

        $scope.getTimeZone = (function () {
            $http.get("https://api.ipify.org?format=json")
                 .success(function (result) {
                     $http.get("https://freegeoip.net/json/" + result.ip)
                          .success(function (result) {
                              if (result.time_zone)
                                  getAccurateTimeZoneSuggestion(result);
                              else
                                  getEstimatedTimeZoneSuggestion();
                          })
                          .error(function () {
                              getEstimatedTimeZoneSuggestion();
                          })
                 })
                 .error(function () {
                     getEstimatedTimeZoneSuggestion();
                 });
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
        return function ($scope) {
            if ($scope.$last) {
                setTimeout(function () {
                    $(".form-notification").on("click", function () {
                        $(this).prev().focus();
                    });
                }, 0);
            }
        };
    });