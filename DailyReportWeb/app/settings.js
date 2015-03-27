'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/instances/:instanceId/projects/:projectId/settings', {
            label: 'Settings',
            templateUrl: 'app/settings.html',
            controller: 'SettingsCtrl'
        });
    }])
    .controller("SettingsCtrl", ["$scope", "$http", '$routeParams', function ($scope, $http, $routeParams) {
        $scope.$parent.child = $scope;
        $scope.status = "loading";

        $http.get("/api/settings/" + $routeParams.projectId)
            .success(function (policy) {
                $scope.user = policy.userOptions ? policy.userOptions[0] : null;
                $scope.sourceControlUsername = policy.sourceControlUsernames ? policy.sourceControlUsernames[0] : null;
                $scope.month = policy.monthlyOptions ? policy.monthlyOptions[0] : null;

                if (!policy.sourceControlOptions)
                    policy.sourceControlOptions = { type: "None" };

                $scope.policy = policy;
            })
            .finally(function () {
                $scope.status = "loaded";
            });

        $scope.updatePolicy = function ($scope) {
            $scope.status = 'saving';
            $scope.advancedOptionsForm.$setPristine();

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
    }]);