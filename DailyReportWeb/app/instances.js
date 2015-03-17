'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/home/list', { templateUrl: 'app/instances.html', controller: 'InstanceCtrl' });
    }])
    .controller("InstanceCtrl", ['$scope', '$http', function ($scope, $http) {
        $scope.status = "loading";

        $http.get("/api/project").success(function (list) {
            $scope.instances = list;

            $scope.status = "loaded";
        });
    }]);