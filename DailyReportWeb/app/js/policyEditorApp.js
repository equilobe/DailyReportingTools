angular.module("policyEditorApp", [])
    .controller("policyListPage", ['$scope', '$http', function ($scope, $http) {
    	$http.get("/api/policy").success(function (list) {
    		$scope.policyList = list;
    	});

    	$scope.updateReportTime = function (policySelected) {
    		$http.put("/api/policy",
				$scope.policyList.filter(function (policy) {
					return policy == policySelected;
				})[0]
    		).success(function () {
    			console.log("success");
    		}).error(function () {
    			console.log("error");
    		});
    	};
    }]);