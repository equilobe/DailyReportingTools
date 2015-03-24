'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/signup', {
            templateUrl: 'app/signUp.html',
            controller: 'SignUpCtrl'
        });
    }])
    .controller("SignUpCtrl", ['$scope', '$http', function ($scope, $http) {
        

        $http.get("http://ipinfo.io/json")
             .then(function (result) {
                 var ip = result.data.ip;
                 return ip;
             })
             .then(function (ip) {
                 return $http.get("http://freegeoip.net/json/" + ip);
             })
            .then(function (result) {
                if (result.data.time_zone != undefined) {
                    //this is a hack so that the time zone id will not be mistaken as being part of the URL
                    var timeZoneId = result.data.time_zone.replace(/\//g, '-'); 
                    $http.get("/api/timezone/" + timeZoneId)
                    .success(function (response) {
                        $scope.timeZoneList = response.timeZoneList;
                        $scope.signUpForm.timeZone = response.suggestedTimeZone;
                    });
                }
                else {
                    $http.get("/api/timezone")
                         .success(function (list) {
                             $scope.timeZoneList = list;
                             var timeZoneOffset = new Date().getTimezoneOffset(); //represents the difference in minutes between UTC and the user's time zone (eg. for +2 a time zone it will be -120)
                             var closestTimeZone = $.grep($scope.timeZoneList, function (element) { return element.utcOffset == -timeZoneOffset })[0];
                             if (closestTimeZone != undefined)
                                 $scope.signUpForm.timeZone = closestTimeZone.id;
                         })
                         .error(function () {
                             $scope.status = "error";
                         });
                }
            });

        $scope.signUp = function ($scope) {
            $scope.status = "saving";
            $scope.signUpForm.$setPristine();

            $http.post("/api/account/register", $scope.signUpForm)
                .success(function (response) {
                    if (response.success) {
                        $scope.status = "success";
                        $scope.message = response.message;
                    }
                    else {
                        $scope.message = response.message;
                        $scope.status = "error";
                    }
                })
                .error(function () {
                    $scope.status = "error";
                });
        };
    }]);