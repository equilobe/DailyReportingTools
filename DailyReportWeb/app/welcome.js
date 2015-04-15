'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/welcome', {
            templateUrl: 'app/welcome.html',
            controller: 'WelcomeController'
        });
    }])
    .controller("WelcomeController", ["$scope", "$http", "$location", function ($scope, $http, $location) {
        $("body").attr("data-page", "welcome");
        $scope.$parent.child = $scope;

        if ($scope.$root.isAuth) {
            $location.url('/app/instances');
            return;
        }

        var iOS = /(iPad|iPhone|iPod)/g.test(navigator.userAgent);
        if (iOS)
            $('.full-page').height(window.innerHeight);
    }]);