'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/help', {
            templateUrl: 'app/help.html',
            controller: 'HelpController'
        });
    }])
    .controller("HelpController", ["$scope", "$http", "$location", function ($scope, $http, $location) {
        $("body").attr("data-page", "help");
        $scope.$parent.child = $scope;
        $scope.stauts = "";

        $http.get("api/help")
            .finally(function (result) {
                $scope.status = result.message;
            });

    }]);