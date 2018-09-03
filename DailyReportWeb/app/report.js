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
                        var i, j, k;

                        for (i = 0; i < data.length; i++) {
                            for (j = 0; j < data[i].worklogs.length; j++) {
                                data[i].worklogs[j].dayHumanReadable = moment(data[i].worklogs[j].date).format("DD/MMM");

                                data[i].worklogs[j].totalTimeSpent = secondsToReportTime(data[i].worklogs[j].totalTimeSpentInSeconds);

                                for (k = 0; k < data[i].worklogs[j].worklogGroup.length; k++) {
                                    data[i].worklogs[j].worklogGroup[k].totalTimeSpent = secondsToReportTime(data[i].worklogs[j].worklogGroup[k].totalTimeSpentInSeconds);
                                }
                            }
                        }

                        return data;
                    }

                    function secondsToReportTime(timeSpent) {
                        var time = moment.utc(timeSpent * 1000);

                        return timeSpent % 3600 == 0 ? time.format("H[h]") : time.format("H[h] m[m]");
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
