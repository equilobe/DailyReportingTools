'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/instances', {
            templateUrl: 'app/instances.html',
            controller: 'InstancesController'
        });
    }])
    .controller("InstancesController", ['$scope', '$http', function ($scope, $http) {
        $("body").attr("data-page", "instances");
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
            $scope.addingInstance = true;
            $scope.form.timeZone = $scope.timeZone;
        };

        $scope.editInstance = function ($scope) {
            $scope.$parent.$parent.editingInstance = true;
            $scope.form.baseUrl = $scope.instance.baseUrl;
            $scope.form.timeZone = $scope.instance.timeZone;
        };

        $scope.clearInstanceForm = function ($scope) {
            $scope.form.$setPristine();
            $scope.form.baseUrl = "";
            $scope.form.jiraUsername = "";
            $scope.form.jiraPassword = "";
            $scope.addingInstance = false;
            $scope.editingInstance = false;
        };

        $scope.saveInstance = function ($scope) {
            $scope.status = 'saving';
            $scope.form.$setPristine();

            $http.post("/api/instances/", $scope.form)
                 .success(function (list) {
                     $scope.$parent.instances = list;
                     $scope.clearInstanceForm($scope);
                     $scope.status = "success";
                 })
                 .error(function () {
                     $scope.status = "error";
                     $scope.message = "Invalid JIRA username or password";
                 });
        };
    }]);