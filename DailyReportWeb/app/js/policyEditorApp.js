angular.module("policyEditorApp", [])
    .controller("policyListPage", ['$scope', '$http', function ($scope, $http) {
        $scope.policyStatus = "loading";

        $http.get("/api/policy").success(function (list) {
            $scope.policyList = list;

            $scope.policyStatus = "loaded";
        });

        $scope.updateReportTime = function ($scope) {
            $http.post("/api/policy", $scope.policy
    		).success(function () {
    		    console.log("success");
    		}).error(function () {
    		    console.log("error");
    		});
        };
    }])
    .controller("policyEditPage", ["$scope", "$http", function ($scope, $http) {
        $scope.policyStatus = "loading";

        $http.get("/api/policy/" + projectId).success(function (policy) {
            if (!policy.sourceControlOptions)
                policy.sourceControlOptions = { type: "None" };

            $scope.user = policy.userOptions ? policy.userOptions[0] : null;
            $scope.sourceControlUsername = policy.sourceControlUsernames ? policy.sourceControlUsernames[0] : null;
            $scope.month = policy.monthlyOptions ? policy.monthlyOptions[0] : null;

            $scope.policy = policy;

            $scope.policyStatus = "loaded";
        });

        $scope.updatePolicy = function ($scope) {
            $http.post("/api/policy/" + $scope.policy.projectId, $scope.policy
                ).success(function () {
                    console.log("success");
                }).error(function () {
                    console.log("error");
                });
        }

        $scope.updateContributors = function ($scope) {
            $scope.sourceControlStatus = "loading";

            $http.post("/api/sourcecontrol", $scope.policy.sourceControlOptions)
                .success(function (sourceControlUsernames) {
                    $scope.sourceControlStatus = "success";

                    $scope.policy.sourceControlUsernames = sourceControlUsernames;
                    $scope.sourceControlUsername = $scope.policy.sourceControlUsernames ? $scope.policy.sourceControlUsernames[0] : null;
                })
                .error(function () {
                    $scope.sourceControlStatus = "error";
                });;
        }
    }]);