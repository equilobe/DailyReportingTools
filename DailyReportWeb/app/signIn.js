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
            $scope.form.$setPristine();

            $http.post("/api/account/login", $scope.form)
                .success(function (response) {
                    if (response.success) {
                        $scope.$root.isAuth = true;
                        $location.path('/app/instances/0/projects');
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