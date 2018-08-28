﻿(function () {
    'use strict';

    angular.module('app')
        .controller("ReportCtrl", ["$scope", "$http", '$routeParams',
            function reportCtrl($scope, $http, $routeParams) {
                var ctrl = this;
                ctrl.instanceId = $routeParams.instanceId;
                ctrl.isLoading = true;
                ctrl.data = {};
                ctrl.actions = {};

                var filter = {
                    pageSize: 10,
                    pageIndex: 1,
                    instanceId: ctrl.instanceId
                };

                $http.get("/api/report/", { params: filter })
                    .success(function (data) {
                        ctrl.data = data;
                    })
                    .finally(function () {
                        ctrl.isLoading = false;
                    });

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
