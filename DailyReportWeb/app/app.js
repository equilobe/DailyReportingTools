angular
    .module("app", ['ngRoute'])
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
                redirectTo: '/app/welcome'
            })
            .when('/app/instances/:instanceId/report/:hash', {
                templateUrl: 'app/report.html',
                controller: 'ReportCtrl as reportCtrl',
                resolve: {
                    report: function ($http, $location, $route) {
                        var params = { instanceId: $route.current.params.instanceId, hash: $route.current.params.hash };

                        $http.get('api/report/isDashboardAvailable', { params: params })
                            .success(function (data) {
                                if (!data)
                                    $location.url('/app/signin');
                            });
                    }
                }
            })
            .when('/app/instances/:instanceId/report', {
                templateUrl: 'app/report.html',
                controller: 'ReportCtrl as reportCtrl',
                resolve: {
                    report: function ($location) {
                        if (!isAuth)
                            $location.url('/app/signin');
                    }
                }
            })
            .otherwise({
                redirectTo: '/'
            });
    }])
    .controller("AppController", ['$scope', '$http', '$location', function ($scope, $http, $location) {
        $scope.$root.isAuth = isAuth;
        $scope.$root.isPlugin = isPlugin;
        $scope.child = {};

        (function jsRegex() {
            for (var key in regex)
                regex[key] = new RegExp(regex[key]);

            $scope.regex = regex;
        })();

        $scope.pageScroll = function () {
            $('body').animate({
                scrollTop: arguments.length ? (typeof (arguments[0]) == "string" ? $(arguments[0]).offset().top :
                                                                                   (arguments[0] == 2 ? $("body").scrollTop() - window.innerHeight : // event.MOUSEUP = 2
                                                                                                        $("body").scrollTop() + window.innerHeight)) : // event.MOUSEDOWN = 1
                                              $("body").scrollTop() + window.innerHeight
            }, 200);

            return false;
        }

        var getAccurateTimeZoneSuggestion = function (result) {
            // this is a hack so that the time zone id will not be mistaken as being part of the URL
            var timeZoneId = result.time_zone.replace(/\//g, '-');
            $http.get("/api/timezone/getTimezoneById", { params: timeZoneId })
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
                    $scope.$root.isAuth = false;
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
    })
    .run(['$route', '$rootScope', '$location', function ($route, $rootScope, $location) {
        var original = $location.path;
        $location.path = function (path, reload) {
            if (reload === false) {
                var lastRoute = $route.current;
                var un = $rootScope.$on('$locationChangeSuccess', function () {
                    $route.current = lastRoute;
                    un();
                });
            }
            return original.apply($location, [path]);
        };
    }]);