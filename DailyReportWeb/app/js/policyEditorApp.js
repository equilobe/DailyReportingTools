angular.module("policyEditorApp", [])
    .controller("policyListPage", ['$scope', '$http', function ($scope, $http) {
        $http.get("/api/policy").success(function (list) {
            $scope.policyList = list;
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
        $http.get("/api/policy/" + projectId).success(function (report) {
            $scope.report = report;
            $scope.sourceControlType = !report.policy.sourceControlOptions ? "none" :
                                       report.policy.sourceControlOptions.repoOwner && report.policy.sourceControlOptions.repo ? "GitHub" :
                                       report.policy.sourceControlOptions.repo ? "SVN" : "none";
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