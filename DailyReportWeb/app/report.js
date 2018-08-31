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
                            ctrl.data = formatDates(data);
                        })
                        .finally(function () {
                            ctrl.isLoading = false;
                        });

                    function formatDates(data) {
                        var i, j;
                        for (i = 0; i < data.length; i++) {
                            for (j = 0; j < data[i].worklogs.length; j++) {
                                data[i].worklogs[j].dayHumanReadable = moment(data[i].date).format("DD/MMM");

                                var timeSpent = data[i].worklogs[j].totalTimeSpentInSeconds;
                                var time = moment.utc(timeSpent * 1000);

                                data[i].worklogs[j].totalTimeSpent = timeSpent % 3600 == 0 ? time.format("H[h]") : time.format("H[h] m[m]");
                            }
                        }

                        return data;
                    }
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
