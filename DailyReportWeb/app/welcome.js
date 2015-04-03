'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/welcome', {
            templateUrl: 'app/welcome.html',
            controller: 'WelcomeController'
        });
    }])
    .controller("WelcomeController", ["$scope", "$http", "$location", function ($scope, $http, $location) {
        if (isAuth) {
            $location.url('/app/instances');
            return;
        }

        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.get("/api/welcome")
            .success(function (response) {
                $scope.data = response;
            })
            .finally(function () {
                $scope.status = "loaded";
            });
    }]);