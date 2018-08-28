(function () {
    'use strict';

    angular.module('app')
        .controller("ReportCtrl", ["$scope", "$http", '$routeParams',
            function reportCtrl($scope, $http, $routeParams) {
                var ctrl = this;
                ctrl.instanceId = $routeParams.instanceId;
                ctrl.isLoading = true;
                ctrl.data = {};
                ctrl.actions = {};
                ctrl.pageSize = 3;
                ctrl.pageIndex = 1;

                ctrl.actions.getDashboardData = function () {
                    var filter = {
                        pageSize: ctrl.pageSize,
                        pageIndex: ctrl.pageIndex,
                        instanceId: ctrl.instanceId
                    };

                    ctrl.isLoading = true;

                    $http.get("/api/report/", { params: filter })
                        .success(function (data) {
                            ctrl.data = data
                        })
                        .finally(function () {
                            ctrl.isLoading = false;
                        });
                }

                ctrl.actions.previousPage = function () {
                    ctrl.pageIndex--;

                    ctrl.actions.getDashboardData();
                }

                ctrl.actions.nextPage = function () {
                    ctrl.pageIndex++;

                    ctrl.actions.getDashboardData();
                }

                ctrl.actions.getDashboardData();

                ctrl.actions.updateDashboardData = function () {
                    ctrl.isLoading = true;

                    $http.post("/api/report/" + ctrl.instanceId)
                        .error(function () {
                            console.log("Error updating dashboard");
                        })
                        .finally(function () {
                            ctrl.isLoading = false;
                        })
                }
            }])
})();
