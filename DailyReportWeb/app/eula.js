'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/eula', {
            templateUrl: 'app/eula.html',
            controller: 'EulaController'
        });
    }])
    .controller("EulaController", ["$scope", "$http", function ($scope, $http) {
        $("body").attr("data-page", "eula");
        $scope.$parent.child = $scope;
    }]);