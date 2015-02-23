angular.module("policyEditorApp", [])
    .controller("policyListPage", ['$scope', '$http', function ($scope, $http) {
        $scope.loading = true;

        $http.get("/api/policy").success(function (list) {
            $scope.policyList = list;

            $scope.loading = false;
        });

        $scope.updateReportTime = function (policy) {
            $http.put("/api/policy", policy
    		).success(function () {
    		    console.log("success");
    		}).error(function () {
    		    console.log("error");
    		});
        };
    }])
    .controller("policyEditPage", ["$scope", "$http", function ($scope, $http) {
        $scope.loading = true;

        $http.get("/api/policy/" + projectId).success(function (policy) {
            $scope.policy = policy;

            $scope.sourceControlType = !policy.sourceControlOptions ? "none" :
                                       policy.sourceControlOptions.repoOwner && policy.sourceControlOptions.repo ? "GitHub" :
                                       policy.sourceControlOptions.repo ? "SVN" : "none";

            $scope.user = policy.userOptions ? policy.userOptions[0] : null;

            $scope.month = policy.monthlyOptions ? policy.monthlyOptions[0] : null;

            $scope.loading = false;
        });

        $scope.updatePolicy = function (policy) {
            $http.put("/api/policy/" + policy.projectId, policy
    		).success(function () {
    		    console.log("success");
    		}).error(function () {
    		    console.log("error");
    		});
        }
    }]);