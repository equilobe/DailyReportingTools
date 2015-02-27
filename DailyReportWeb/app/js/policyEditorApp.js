angular.module("policyEditorApp", [])
    .controller("policyListPage", ['$scope', '$http', function ($scope, $http) {
        $scope.loading = true;

        $http.get("/api/policy").success(function (list) {
            $scope.policyList = list;

            $scope.loading = false;
        });

        $scope.updateReportTime = function (policy) {
            $http.post("/api/policy", policy
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
            if (!policy.sourceControlOptions)
                policy.sourceControlOptions = { type: "None" };

            $scope.user = policy.userOptions ? policy.userOptions[0] : null;
            $scope.month = policy.monthlyOptions ? policy.monthlyOptions[0] : null;

            $scope.policy = policy;

            $scope.loading = false;
        });

        $scope.updatePolicy = function (policy) {
            $http.post("/api/policy/" + policy.projectId, policy
    		).success(function () {
    		    console.log("success");
    		}).error(function () {
    		    console.log("error");
    		});
        }
    }]);