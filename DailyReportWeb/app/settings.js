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
            .success(function (policy) {
                $scope.user = policy.userOptions ? policy.userOptions[0] : null;
                $scope.sourceControlUsername = policy.sourceControlUsernames ? policy.sourceControlUsernames[0] : null;
                $scope.month = policy.monthlyOptions ? policy.monthlyOptions[0] : null;
                $scope.days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

                if (!policy.sourceControlOptions)
                    policy.sourceControlOptions = { type: "None" };

                if (!policy.advancedOptions.additionalWorkflowStatuses)
                    policy.advancedOptions.additionalWorkflowStatuses = [''];

                $scope.policy = policy;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.updatePolicy = function ($scope) {
            $scope.status = 'saving';
            $scope.form.$setPristine();

            $http.post("/api/settings/", $scope.policy
                ).success(function () {
                    $scope.status = "success";
                    console.log("success");
                }).error(function () {
                    $scope.status = "error";
                    console.log("error");
                });
        };

        $scope.updateContributors = function ($scope) {
            $scope.sourceControlStatus = "loading";

            $http.post("/api/sourcecontrol", $scope.policy.sourceControlOptions)
                .success(function (sourceControlUsernames) {
                    $scope.sourceControlStatus = "success";
                    $scope.policy.sourceControlUsernames = sourceControlUsernames;
                })
                .error(function () {
                    $scope.sourceControlStatus = "error";
                    $scope.policy.sourceControlUsernames = null;
                });
        };

        $scope.addSourceControlUsername = function ($scope) {
            if ($scope.user.sourceControlUsernames == null)
                $scope.user.sourceControlUsernames = [];

            $scope.user.sourceControlUsernames.push($scope.sourceControlUsername);
        };

        $scope.removeSourceControlUsername = function ($scope) {
            for (var i = $scope.user.sourceControlUsernames.length - 1; i >= 0; i--)
                if ($scope.user.sourceControlUsernames[i] == $scope.sourceControlUsername) {
                    $scope.user.sourceControlUsernames.splice(i, 1);
                    break;
                }
        };


    }])
    .directive("weekendDays", function () {
        return function ($scope, $elem) {
            if ($scope.policy.advancedOptions.weekendDaysList.indexOf($scope.day) !== -1) {
                $elem[0].checked = true;
            }

            $elem.bind('click', function () {
                var index = $scope.policy.advancedOptions.weekendDaysList.indexOf($scope.day);

                if ($elem[0].checked) {
                    if (index === -1) $scope.policy.advancedOptions.weekendDaysList.push($scope.day);
                }
                else {
                    if (index !== -1) $scope.policy.advancedOptions.weekendDaysList.splice(index, 1);
                }

                $scope.$apply($scope.policy.advancedOptions.weekendDaysList);
                $scope.$apply($scope.form.$setDirty());
            });
        }
    })
    .directive("workflowStatus", function () {
        return function ($scope, $elem) {
            $elem.bind('blur', function () {
                if (!$scope.$last && $elem[0].value == "") {
                    $scope.policy.advancedOptions.additionalWorkflowStatuses.splice($scope.$index, 1);
                }

                if (($scope.$last && $elem[0].value != "") ||
                    !$scope.policy.advancedOptions.additionalWorkflowStatuses.length) {
                    $scope.policy.advancedOptions.additionalWorkflowStatuses.push('');

                }

                $scope.$apply($scope.policy.advancedOptions.additionalWorkflowStatuses);
            });
        }
    });