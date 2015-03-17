'use strict';

angular.module('app')
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/app/edit/:projectId', { templateUrl: 'app/policy.html', controller: 'PolicyCtrl' });
    }])
    .controller("PolicyCtrl", ["$scope", "$http", '$routeParams', function ($scope, $http, $routeParams) {
        $scope.policyStatus = "loading";

        $http.get("/api/policy/" + $routeParams.projectId).success(function (policy) {
            $scope.user = policy.userOptions ? policy.userOptions[0] : null;
            $scope.sourceControlUsername = policy.sourceControlUsernames ? policy.sourceControlUsernames[0] : null;
            $scope.month = policy.monthlyOptions ? policy.monthlyOptions[0] : null;

            if (!policy.sourceControlOptions)
                policy.sourceControlOptions = { type: "None" };

            $scope.policy = policy;
            $scope.policyStatus = "loaded";
        });

        $scope.updatePolicy = function ($scope) {
            $scope.policyStatus = 'saving';
            $scope.advancedOptionsForm.$setPristine();

            $http.post("/api/policy/" + $scope.policy.projectId, $scope.policy
                ).success(function () {
                    $scope.policyStatus = "success";
                    console.log("success");
                }).error(function () {
                    $scope.policyStatus = "error";
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