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
                list.forEach(function (item) {
                    item.hostname = item.baseUrl.substring(item.baseUrl.indexOf('/') + 2, item.baseUrl.length).split('/')[0];
                });

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

        $scope.deleteInstance = function ($scope) {
            if (confirm("Are you sure you want to remove instance ?\n" + $scope.instance.baseUrl) == true) {
                $scope.child.status = "loading";

                $http.delete("/api/instances/" + $scope.instance.id)
                    .success(function (list) {
                        $scope.child.instances = list;
                    })
                    .finally(function () {
                        $scope.child.status = "loaded";
                    });
            }
        };

        $scope.clearInstanceForm = function ($scope) {
            $scope.form.$setPristine();
            $scope.form.baseUrl = "";
            $scope.form.jiraUsername = "";
            $scope.form.jiraPassword = "";
            $scope.status = "";
            $scope.message = "";
            $scope.addingInstance = false;
            $scope.editingInstance = false;
        };

        $scope.saveInstance = function ($scope) {
            $scope.status = 'saving';
            $scope.form.$setPristine();

            var newInstanceHostname = $scope.form.baseUrl.substring($scope.form.baseUrl.indexOf('/') + 2, $scope.form.baseUrl.length).split('/')[0];
            $scope.instances.forEach(function (instance) {
                if (instance.baseUrl.indexOf(newInstanceHostname) != -1) {
                    $scope.message = "Jira server is already present!";
                    $scope.status = "error";
                    return;
                }
            });

            if ($scope.status == "error")
                return;

            $http.post("/api/instances/", $scope.form)
                 .success(function (list) {
                     $scope.$parent.instances = list;
                     $scope.clearInstanceForm($scope);
                     $scope.status = "success";
                 })
                 .error(function () {
                     $scope.message = "Invalid JIRA username or password";
                     $scope.status = "error";
                 });
        };
    }]);