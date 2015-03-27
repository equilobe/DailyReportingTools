'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/list', {
            label: 'Instances',
            templateUrl: 'app/instances.html',
            controller: 'InstancesCtrl'
        });
    }])
    .controller("InstancesCtrl", ['$scope', '$http', function ($scope, $http) {
        $scope.status = "loading";

        $http.get("/api/instances")
            .success(function (list) {
                $scope.instances = list;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.addNewInstance = function ($scope) {
            $scope.addingInstance = true;
        };

        $scope.addInstance = function ($scope) {
            $scope.status = 'saving';
            $scope.newInstanceForm.$setPristine();

            $http.post("/api/instances/", $scope.newInstanceForm
                ).success(function (list) {
                    $scope.instances = list;
                    $scope.addingInstance = false;
                    $scope.status = "success";
                    console.log("success");
                }).error(function () {
                    $scope.status = "error";
                    console.log("error");
                });
        };
    }]);