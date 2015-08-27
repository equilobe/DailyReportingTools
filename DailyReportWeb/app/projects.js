'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/app/instances/:instanceId/projects', {
                templateUrl: 'app/projects.html',
                controller: 'ProjectsController'
            });
    }])
    .controller("ProjectsController", ['$scope', '$http', "$location", '$routeParams', function ($scope, $http, $location, $routeParams) {
        $("body").attr("data-page", "projects");
        $scope.$parent.child = $scope;
        $scope.status = "loading";
        $scope.serializedInstance = {};

        $http.get("/api/projects/")
            .success(function (instances) {

                $scope.instance = {};
                if (instances) {
                    if ($routeParams.instanceId == 0) {
                        $scope.instance = instances[0];
                        $location.path('/app/instances/' + instances[0].id + '/projects', false);
                    }
                    else {
                        instances.forEach(function (instance) {
                            if (instance.id == $routeParams.instanceId)
                                $scope.instance = instance;
                        });

                        if ($scope.instance.id != $routeParams.instanceId)
                            $scope.instance = instances[0];
                    }
                }

                $scope.instances = instances;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.setInstance = function (instance) {
            $scope.instance = instance;
            $location.path('/app/instances/' + instance.id + '/projects', false);
            if (!instance.isActive) {
                $scope.serializedInstance = {
                    instanceId: instance.id,
                    baseUrl: instance.baseUrl
                };
            }
        }
    }]);