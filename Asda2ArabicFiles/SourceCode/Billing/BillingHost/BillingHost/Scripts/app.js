angular.module('logs', ['ui.bootstrap']);
function ApplyDonationCtrl($scope, $http) {
    $scope.model = {
        payerName: '',
        characterName: '',
        transactionId: ''
    };
    $scope.message = '';
    $scope.loading = false;

    $scope.applyDonation = function () {
        $scope.loading = true;
        $http({ method: "GET", url: "../apply_donation/"  + $scope.model.payerName + "/" +$scope.model.transactionId + "/" + $scope.model.characterName}).
                success(function (data) {
                    switch (data) {
                        case 'wrong_transaction_id':
                            $scope.message = 'Wrong transaction id';
                            break;
                        case 'donation_not_founded':
                            $scope.message = 'Donation record not founded. Check name in CashU system and CashU transaction id.';
                            break;
                        case 'already_delivered':
                            $scope.message = 'Donation already delivered';
                            break;
                        case 'character_name_already_seted':
                            $scope.message = 'Character name for this donation already setted';
                            break;
                        case 'ok':
                            $scope.message = 'All ok. Enter game or teleport to any location to recive donation.';
                            break;
                        case 'character_not_found':
                            $scope.message = 'Character with such name not found.';
                            break;
                    }
                    $scope.loading = false;
                }).error(function () {
                    $scope.loading = false;
                });
    };
}
