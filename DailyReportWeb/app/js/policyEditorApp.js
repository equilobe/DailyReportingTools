angular.module("policyEditorApp", [])
    .controller("policyListPage", ['$scope', '$http', function ($scope, $http) {
    	$http.get("/api/policy").success(function (list) {
    		$scope.policyList = list;
    	})
    }])
	.directive('onLastRepeat', function () {
		return function (scope, element, attrs) {
			if (scope.$last) setTimeout(function () {
				$("input").on("keyup", function () {
					var selectedInput = $(this);

					if (selectedInput.hasClass("ng-invalid")) {
						selectedInput.parent().addClass("has-error")
							.find('.reportTime-notification').show();
					}
					else {
						selectedInput.parent().removeClass("has-error")
							.find('.reportTime-notification').hide();;
					}
				});

				$(".reportTime-notification").on("click", function () {
					$(this).prev().focus();
				});
			}, 1);
		};
	});