'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/contact', {
            templateUrl: 'app/contact.html',
            controller: 'ContactController'
        });
    }])
    .controller("ContactController", ["$scope", "$http", function ($scope, $http) {
        $("body").attr("data-page", "contact");
        $scope.$parent.child = $scope;
    }]);