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
                $scope.instance = instances ? ($routeParams.instanceId ? instances[$routeParams.instanceId - 1] : instances[0]) : null;
                instances.push([{ id: 0, baseUrl: "Manage JIRA servers" }]);
                $scope.instances = instances;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.checkNavigation = function () {
            if (this.instance[0].id == 0) {
                $location.url('/app/instances');
                return;
            }
        }
    }]);