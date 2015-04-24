'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/welcome', {
            templateUrl: 'app/welcome.html',
            controller: 'WelcomeController'
        });
    }])
    .controller("WelcomeController", ["$scope", "$http", "$location", function ($scope, $http, $location) {
        if ($scope.$root.isAuth) {
            $location.path('/app/instances/0/projects');
            return;
        }

        $("body").attr("data-page", "welcome");
        $scope.$parent.child = $scope;

        var iOS = /(iPad|iPhone|iPod)/g.test(navigator.userAgent);
        if (iOS)
            $('.full-page').height(window.innerHeight);
    }]);