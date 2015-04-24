'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/howItWorks', {
            templateUrl: 'app/howItWorks.html',
            controller: 'HowItWorksController'
        });
    }])
    .controller("HowItWorksController", ["$scope", "$http", function ($scope, $http) {
        $("body").attr("data-page", "howItWorks");
        $scope.$parent.child = $scope;
    }]);