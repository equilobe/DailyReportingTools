(function () {
    'use strict';

    angular.module('app')
        .controller("ReportCtrl", ["$scope", "$http", '$routeParams', '$location',
            function reportCtrl($scope, $http, $routeParams, $location) {
                var ctrl = this;
                ctrl.instanceId = $routeParams.instanceId;
                ctrl.hash = $routeParams.hash;
                ctrl.data = {};
                ctrl.actions = {};
                ctrl.neatDate = 'dd/MMM';

                ctrl.actions.getDashboardData = function () {
                    ctrl.isLoading = true;

                    $http.get("/api/report?instanceId=" + ctrl.instanceId + "&hash=" + ctrl.hash + "&isAuthenticated=" + isAuth)
                        .success(function (data) {
                            if (!data.isAvailable)
                                $location.url('/app/signin');

                            ctrl.data = data.items;
                        })
                        .finally(function () {
                            ctrl.isLoading = false;
                        });
                }

                ctrl.actions.getDashboardData();

                ctrl.actions.updateDashboardData = function () {
                    ctrl.isLoading = true;
                    ctrl.data = {};

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
        .filter('neatDate', [function () {
            return function (date) {
                return moment(date).format("DD/MMM");
            }
        }])
        .filter('neatFirstDate', [function () {
            return function (date) {
                return moment(date).calendar(null, {
                    lastDay: '[Yesterday]',
                    lastWeek: 'dddd'
                });
            }
        }])
        .filter('neatDuration', [function () {
            return function (seconds) {
                var hours = Math.floor(seconds / 3600),
                    minutes = Math.floor((seconds % 3600) / 60);

                return (minutes == 0) ? hours + 'h' : hours + 'h ' + minutes + 'm'; 
            }
        }])
})();
