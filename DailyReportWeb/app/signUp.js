'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/signup', {
            templateUrl: 'app/signUp.html',
            controller: 'SignUpCtrl'
        });
    }])
    .controller("SignUpCtrl", ['$scope', '$http', function ($scope, $http) {
        var getAccurateTimeZoneSuggestion = function (result) {
            // this is a hack so that the time zone id will not be mistaken as being part of the URL
            var timeZoneId = result.time_zone.replace(/\//g, '-');
            $http.get("/api/timezone/" + timeZoneId)
                 .success(function (response) {
                     $scope.timeZoneList = response.timeZoneList;
                     $scope.signUpForm.timeZone = response.suggestedTimeZone;
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
                     if (closestTimeZone != undefined)
                         $scope.signUpForm.timeZone = closestTimeZone.id;
                 });
        }

        $http.get("https://api.ipify.org?format=json")
             .success(function (result) {
                 $http.get("https://freegeoip.net/json/" + result.ip)
                      .success(function (result) {
                          if (result.time_zone != undefined)
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