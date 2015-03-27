'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/instances', {
            label: 'Instances',
            templateUrl: 'app/instances.html',
            controller: 'InstancesCtrl'
        });
    }])
    .controller("InstancesCtrl", ['$scope', '$http', function ($scope, $http) {
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.get("/api/instances")
            .success(function (list) {
                $scope.instances = list;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.addInstance = function ($scope) {
            $scope.status = 'saving';
            $scope.form.$setPristine();

            $http.post("/api/instances/", $scope.form
                ).success(function (list) {
                    $scope.$parent.instances = list;
                    $scope.addingInstance = false;
                    $scope.form = null;
                    $scope.status = "success";
                    console.log("success");
                }).error(function () {
                    $scope.status = "error";
                    console.log("error");
                });
        };
    }]);