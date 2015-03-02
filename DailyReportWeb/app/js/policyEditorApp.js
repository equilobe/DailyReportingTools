angular.module("policyEditorApp", [])
    .controller("policyListPage", ['$scope', '$http', function ($scope, $http) {
        $scope.policyStatus = "loading";

        $http.get("/api/policy").success(function (list) {
            $scope.policyList = list;

            $scope.policyStatus = "loaded";
        });

        $scope.updateReportTime = function ($scope) {
            if ($scope.reportForm.reportTime.$invalid || $scope.reportForm.reportTime.$pristine)
                return;

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

            $http.post("/api/policy/" + $scope.policy.projectId, $scope.policy
                ).success(function () {
                    $scope.policyStatus = "success";
                    console.log("success");
                }).error(function () {
                    $scope.policyStatus = "error";
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
                });
        }

        $scope.addSourceControlUsername = function ($scope) {
            $scope.user.sourceControlUsernames.push($scope.sourceControlUsername);
        }

        $scope.removeSourceControlUsername = function ($scope) {
            for (var i = $scope.user.sourceControlUsernames.length - 1; i >= 0; i--)
                if ($scope.user.sourceControlUsernames[i] == $scope.sourceControlUsername) {
                    $scope.user.sourceControlUsernames.splice(i, 1);
                    break;
                }
        }
    }]);