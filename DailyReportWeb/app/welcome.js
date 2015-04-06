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
    }]);