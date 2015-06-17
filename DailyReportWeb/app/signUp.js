'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/signup', {
            templateUrl: 'app/signUp.html',
            controller: 'SignUpController'
        });
    }])
    .controller("SignUpController", ['$scope', '$http', '$interval', function ($scope, $http, $interval) {
        $("body").attr("data-page", "signup");
        $scope.$parent.child = $scope;
        $scope.username = "";
        $scope.instanceUrl = "";
        $scope.subscribePhase = false;

        $scope.setTimeZone = $interval(function () {
            if ($scope.timeZone) {
                $scope.child.form.timeZone = $scope.timeZone;
                $interval.cancel($scope.setTimeZone);
            }
        }, 300);

        $scope.signUp = function ($scope) {
            $scope.status = "saving";
            $scope.form.$setPristine();

            $http.post("/api/account/register", $scope.form)
                .success(function (response) {
                    if (!response.hasError) {
                        $scope.username = $scope.form.email;
                        $scope.instanceUrl = $scope.form.baseUrl;
                        $scope.message = response.message;
                        $scope.status = "success";
                        $scope.subscribePhase = true;
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