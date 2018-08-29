(function () {
    'use strict';

    angular.module('app')
        .controller("ReportCtrl", ["$scope", "$http", '$routeParams',
            function reportCtrl($scope, $http, $routeParams) {
                var ctrl = this;
                ctrl.instanceId = $routeParams.instanceId;
                ctrl.data = {};
                ctrl.actions = {};

                ctrl.actions.getDashboardData = function () {
                    ctrl.isLoading = true;

                    $http.get("/api/report/" + ctrl.instanceId)
                        .success(function (data) {
                            ctrl.data = data
                        })
                        .finally(function () {
                            ctrl.isLoading = false;
                        });
                }

                ctrl.actions.getDashboardData();

                ctrl.actions.updateDashboardData = function () {
                    ctrl.isLoading = true;

                    $http.post("/api/report/" + ctrl.instanceId)
                        .error(function () {
                            console.log("Error updating dashboard");
                        })
                        .success(function () {
                            ctrl.actions.getDashboardData();
                        })
                        .finally(function () {
                            ctrl.isLoading = false;
                        });
                }
            }])
})();
