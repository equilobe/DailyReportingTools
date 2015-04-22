'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/app/instances/:instanceId/projects', {
                templateUrl: 'app/projects.html',
                controller: 'ProjectsController'
            }).when('/app/projects', {
                templateUrl: 'app/projects.html',
                controller: 'ProjectsController'
            });
    }])
    .controller("ProjectsController", ['$scope', '$http', "$location", '$routeParams', function ($scope, $http, $location, $routeParams) {
        $("body").attr("data-page", "projects");
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.get("/api/projects/")
            .success(function (instances) {

                $scope.instance = {};
                if (instances.length != 0) {
                    if (!$routeParams.instanceId) {
                        $scope.instance = instances[0];
                    }
                    else {
                        instances.forEach(function (instance) {
                            if (instance[0].installedInstanceId == $routeParams.instanceId)
                                $scope.instance = instance;
                        });

                        if ($scope.instance[0].installedInstanceId != $routeParams.instanceId)
                            $scope.instance = instances[0];
                    }
                }

                $scope.instances = instances;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.setInstance = function () {
            $scope.instance = this.instance;
        }
    }]);