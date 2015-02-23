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

        $http.get("/api/policy/" + projectId).success(function (report) {
            $scope.report = report;

            $scope.sourceControlType = !report.policy.sourceControlOptions ? "none" :
                                       report.policy.sourceControlOptions.repoOwner && report.policy.sourceControlOptions.repo ? "GitHub" :
                                       report.policy.sourceControlOptions.repo ? "SVN" : "none";

            $scope.user = report.policy.userOptions ? report.policy.userOptions[0] : null;

            $scope.month = report.policy.monthlyOptions ? report.policy.monthlyOptions[0] : null;

            $scope.loading = false;
        });

        $scope.updateReport = function (report) {
            $http.put("/api/policy/" + report.projectId, report
    		).success(function () {
    		    console.log("success");
    		}).error(function () {
    		    console.log("error");
    		});
        }
    }]);