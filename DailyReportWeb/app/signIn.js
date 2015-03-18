'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/signin', { templateUrl: 'app/signIn.html', controller: 'SignInCtrl' });
    }])
    .controller("SignInCtrl", ['$scope', '$http', function ($scope, $http) {
        $scope.signIn = function ($scope) {
            $scope.status = "checking";
            $scope.signInForm.$setPristine();

            $http.post("/api/account/login", $scope.signInForm)
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