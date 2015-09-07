'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/instances/:instanceId/projects/:projectId/settings', {
            templateUrl: 'app/settings.html',
            controller: 'SettingsController'
        });
    }])
    .controller("SettingsController", ["$scope", "$http", '$routeParams', function ($scope, $http, $routeParams) {
        $("body").attr("data-page", "settings");
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.get("/api/settings/" + $routeParams.projectId)
            .success(function (data) {
                $scope.user = data.userOptions ? data.userOptions[0] : null;
                $scope.sourceControlUsername = data.sourceControlUsernames ? data.sourceControlUsernames[0] : null;
                $scope.month = data.monthlyOptions ? data.monthlyOptions[0] : null;
                $scope.days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

                if (!data.sourceControlOptions)
                    data.sourceControlOptions = { type: "None" };

                if (!data.advancedOptions.additionalWorkflowStatuses || !data.advancedOptions.additionalWorkflowStatuses.length)
                    data.advancedOptions.additionalWorkflowStatuses = [''];

                $scope.data = data;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.saveSettings = function ($scope) {
            $scope.status = 'saving';
            $scope.form.$setPristine();

            $http.post("/api/settings/", $scope.data
                ).success(function () {
                    $scope.status = "success";
                }).error(function () {
                    $scope.status = "error";
                });
        };

        $scope.updateContributors = function ($scope) {
            $scope.sourceControlStatus = "loading";

            $http.post("/api/sourcecontrol", $scope.data.sourceControlOptions)
                .success(function (sourceControlUsernames) {
                    $scope.sourceControlStatus = "success";
                    if (sourceControlUsernames == null)
                        $scope.sourceControlStatus = "error";

                    $scope.data.sourceControlUsernames = sourceControlUsernames;
                })
                .error(function () {
                    $scope.sourceControlStatus = "error";
                    $scope.data.sourceControlUsernames = null;
                });
        };

        $scope.addSourceControlUsername = function ($scope) {
            if ($scope.user.sourceControlUsernames == null)
                $scope.user.sourceControlUsernames = [];

            $scope.user.sourceControlUsernames.push($scope.sourceControlUsername);
            var i = $scope.data.sourceControlUsernames.indexOf($scope.sourceControlUsername);
            if (i != -1)
                $scope.data.sourceControlUsernames.splice(i, 1);
            $scope.sourceControlUsername = $scope.data.sourceControlUsernames[0];
        };

        $scope.removeSourceControlUsername = function ($scope) {
            var i = $scope.user.sourceControlUsernames.indexOf($scope.sourceControlUsername);
            if (i != -1) {
                $scope.user.sourceControlUsernames.splice(i, 1);
                $scope.data.sourceControlUsernames.push($scope.sourceControlUsername);
            }
            $scope.form.$setDirty();
        };

        $scope.editUser = function ($scope) {
            $scope.$parent.editingUser = true;
        };

        $scope.cancelEditingUser = function ($scope) {
            $scope.$parent.editingUser = false;
        };

        $scope.includeAll = function ($scope) {
            var setDirty = false;

            angular.forEach($scope.data.userOptions, function (user) {
                if (user.included == false)
                    setDirty = true;
                user.included = true;
            });

            if (setDirty)
                $scope.form.$setDirty();
        };

        $scope.excludeAll = function ($scope) {
            var setDirty = false;

            angular.forEach($scope.data.userOptions, function (user) {
                if (user.included == true)
                    setDirty = true;
                user.included = false;
            });

            if (setDirty)
                $scope.form.$setDirty();
        };



    }])
    .directive("weekendDays", function () {
        return function ($scope, $elem) {
            if ($scope.data.advancedOptions.weekendDaysList.indexOf($scope.day) !== -1) {
                $elem[0].checked = true;
            }

            $elem.bind('click', function () {
                var index = $scope.data.advancedOptions.weekendDaysList.indexOf($scope.day);

                if ($elem[0].checked) {
                    if (index === -1) $scope.data.advancedOptions.weekendDaysList.push($scope.day);
                }
                else {
                    if (index !== -1) $scope.data.advancedOptions.weekendDaysList.splice(index, 1);
                }

                $scope.$apply($scope.data.advancedOptions.weekendDaysList);
                $scope.$apply($scope.form.$setDirty());
            });
        }
    })
    .directive("workflowStatus", function () {
        return function ($scope, $elem) {
            $elem.bind('blur', function () {
                if (!$scope.$last && $elem[0].value == "") {
                    $scope.data.advancedOptions.additionalWorkflowStatuses.splice($scope.$index, 1);
                }

                if (($scope.$last && $elem[0].value != "") ||
                    !$scope.data.advancedOptions.additionalWorkflowStatuses.length) {
                    $scope.data.advancedOptions.additionalWorkflowStatuses.push('');

                }

                $scope.$apply($scope.data.advancedOptions.additionalWorkflowStatuses);
            });
        }
    });