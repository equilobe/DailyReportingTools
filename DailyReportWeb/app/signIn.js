'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/signin', {
            templateUrl: 'app/signIn.html',
            controller: 'SignInController'
        });
    }])
    .controller("SignInController", ['$scope', '$http', '$location', function ($scope, $http, $location) {
        $("body").attr("data-page", "signin");
        $scope.$parent.child = $scope;

        $scope.signIn = function ($scope) {
            $scope.status = "checking";
            $scope.signInForm.$setPristine();

            $http.post("/api/account/login", $scope.signInForm)
                .success(function (response) {
                    if (response.success) {
                        $scope.$parent.isAuth = true;
                        $location.path('/app/instances');
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