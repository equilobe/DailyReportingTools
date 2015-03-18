'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/signup', { templateUrl: 'app/signUp.html', controller: 'SignUpCtrl' });
    }])
    .controller("SignUpCtrl", ['$scope', '$http', function ($scope, $http) {
        $scope.signUp = function ($scope) {
            $scope.status = "saving";
            $scope.signUpForm.$setPristine();

            $http.post("/api/account/register", $scope.signUpForm)
                .success(function (response) {
                    if (response.success)
                        $scope.status = "success";
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