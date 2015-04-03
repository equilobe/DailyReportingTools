'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/signup', {
            templateUrl: 'app/signUp.html',
            controller: 'SignUpController'
        });
    }])
    .controller("SignUpController", ['$scope', '$http', '$interval', function ($scope, $http, $interval) {
        $scope.$parent.child = $scope;

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