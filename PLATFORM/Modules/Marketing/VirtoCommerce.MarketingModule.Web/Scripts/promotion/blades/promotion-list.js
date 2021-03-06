﻿angular.module('virtoCommerce.marketingModule')
.controller('virtoCommerce.marketingModule.promotionListController', ['$scope', 'virtoCommerce.marketingModule.promotions', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', function ($scope, promotions, bladeNavigationService, dialogService) {
    var selectedNode = null;
    //pagination settings
    $scope.pageSettings = {};
    $scope.pageSettings.totalItems = 0;
    $scope.pageSettings.currentPage = 1;
    $scope.pageSettings.numPages = 5;
    $scope.pageSettings.itemsPerPageCount = 20;

    $scope.filter = { searchKeyword: undefined };

    $scope.blade.refresh = function () {
        $scope.blade.isLoading = true;
        $scope.blade.selectedAll = false;

        promotions.search({
            respGroup: 'withPromotions',
            keyword: $scope.filter.searchKeyword,
            start: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
            count: $scope.pageSettings.itemsPerPageCount
        }, function (data) {
            $scope.blade.isLoading = false;

            $scope.pageSettings.totalItems = angular.isDefined(data.totalCount) ? data.totalCount : 0;
            $scope.blade.currentEntities = data.promotions;

            if (selectedNode != null) {
                //select the node in the new list
                angular.forEach($scope.blade.currentEntities, function (node) {
                    if (selectedNode.id === node.id) {
                        selectedNode = node;
                    }
                });
            }
        }, function (error) {
            bladeNavigationService.setError('Error ' + error.status, $scope.blade);
        });
    };

    $scope.selectNode = function (node) {
        selectedNode = node;
        $scope.selectedNodeId = selectedNode.id;

        var newBlade = {
            id: 'listItemChild',
            currentEntityId: selectedNode.id,
            title: selectedNode.name,
            subtitle: $scope.blade.subtitle,
            controller: 'virtoCommerce.marketingModule.promotionDetailController',
            template: 'Modules/$(VirtoCommerce.Marketing)/Scripts/promotion/blades/promotion-detail.tpl.html'
        };

        bladeNavigationService.showBlade(newBlade, $scope.blade);
    };

    $scope.toggleAll = function () {
        angular.forEach($scope.blade.currentEntities, function (item) {
            if (item.type === 'DynamicPromotion') {
                item.selected = $scope.blade.selectedAll;
            }
        });
    };

    function isItemsChecked() {
        return $scope.blade.currentEntities && _.any($scope.blade.currentEntities, function (x) { return x.selected; });
    }

    function deleteChecked() {
        var dialog = {
            id: "confirmDeleteItem",
            title: "Delete confirmation",
            message: "Are you sure you want to delete selected Promitions?",
            callback: function (remove) {
                if (remove) {
                    closeChildrenBlades();

                    var selection = _.where($scope.blade.currentEntities, { selected: true });
                    var itemIds = _.pluck(selection, 'id');
                    promotions.remove({ ids: itemIds }, function (data, headers) {
                        $scope.blade.refresh();
                    }, function (error) {
                        bladeNavigationService.setError('Error ' + error.status, $scope.blade);
                    });
                }
            }
        }
        dialogService.showConfirmationDialog(dialog);
    }

    function closeChildrenBlades() {
        angular.forEach($scope.blade.childrenBlades.slice(), function (child) {
            bladeNavigationService.closeBlade(child);
        });
    }

    $scope.blade.headIcon = 'fa-area-chart';

    $scope.blade.toolbarCommands = [
        {
            name: "Refresh", icon: 'fa fa-refresh',
            executeMethod: function () {
                $scope.blade.refresh();
            },
            canExecuteMethod: function () {
                return true;
            }
        },
        {
            name: "Add", icon: 'fa fa-plus',
            executeMethod: function () {
                closeChildrenBlades();

                var newBlade = {
                    id: 'listItemChild',
                    title: 'New Promotion list',
                    subtitle: $scope.blade.subtitle,
                    isNew: true,
                    controller: 'virtoCommerce.marketingModule.promotionDetailController',
                    template: 'Modules/$(VirtoCommerce.Marketing)/Scripts/promotion/blades/promotion-detail.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, $scope.blade);
            },
            canExecuteMethod: function () {
                return true;
            },
            permission: 'marketing:create'
        },
        {
            name: "Delete", icon: 'fa fa-trash-o',
            executeMethod: function () {
                deleteChecked();
            },
            canExecuteMethod: function () {
                return isItemsChecked();
            },
            permission: 'marketing:delete'
        }
    ];

    $scope.$watch('pageSettings.currentPage', function () {
        $scope.blade.refresh();
    });

    // actions on load
    //No need to call this because page 'pageSettings.currentPage' is watched!!! It would trigger subsequent duplicated req...
    //$scope.blade.refresh();
}]);