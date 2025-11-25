workspace "ECommerce Modular Monolith" "A modular monolith e-commerce system with .NET 8" {

  !identifiers hierarchical

    model {
        customer = person "Customer" "A customer browsing and purchasing products" "Person"
        admin = person "Administrator" "Manages products, orders, and customers" "Person"
        databaseAdministrator = person "Database Administrator" "Manages and maintains the database infrastructure" "Person"

        group "ECommerce Solution" {

            ecommerceSystem = softwareSystem "ECommerce System" "A modular monolith e-commerce platform built with .NET 8" {

                group "ECommerce User Interfaces" {
                    adminUI = container "Admin UI" "The administrative interface for managing the e-commerce platform" "Blazor, ASP.NET Core" {
                        admin -> adminUI "Uses to manage products, orders, and customers"
                    }

                    customerUI = container "Customer UI" "The customer-facing interface for browsing and purchasing products" "Blazor, ASP.NET Core" {
                        customer -> customerUI "Uses to browse products and place orders"
                    }
                }

                group "ECommerce Backend Services" {
                    ecommerceApi = container "ECommerce API" "The backend API for handling e-commerce operations" ".NET 8, ASP.NET Core" {
                        group "Domain Modules" {
                            productModule = component "Product Module" "Handles product catalog management" ".NET 8"
                            orderModule = component "Order Module" "Handles order processing and management" ".NET 8"
                            customerModule = component "Customer Module" "Handles customer account management" ".NET 8"
                        }
                        customerUI -> ecommerceApi "Uses to browse products and place orders"
                        adminUI -> ecommerceApi "Uses to manage products, orders, and customers"
                    }
                }

                group "ECommerce Data Stores" {
                    database = container "Database" "Stores all e-commerce data including products, orders, and customer information" "SQL Server" {
                        ecommerceApi -> database "Reads from and writes to"
                        databaseAdministrator -> database "Manages and maintains"
                    }
                }
            }

            accountingSystem = softwareSystem "Accounting System" "Handles financial transactions and reporting for the e-commerce platform" {
                ecommerceSystem -> accountingSystem "Sends order and payment data to"
                accountingService = container "Accounting Service" "Handles financial transactions and reporting" ".NET 8, ASP.NET Core"
            }

            shippingSystem = softwareSystem "Shipping System" "Manages shipping and delivery of orders" {
                ecommerceSystem -> shippingSystem "Sends order and shipping data to"
                shippingService = container "Shipping Service" "Handles shipping and delivery operations" ".NET 8, ASP.NET Core"
            }

            notificationSystem = softwareSystem "Notification System" "Sends notifications to customers about their orders" {
                ecommerceSystem -> notificationSystem "Sends order confirmation and shipping notifications to"
                notificationService = container "Notification Service" "Handles sending notifications via email and SMS" ".NET 8, ASP.NET Core"
            }

        }


        ecommerceSystem.ecommerceApi -> accountingSystem.accountingService "Sends order and payment data to"
        ecommerceSystem.ecommerceApi -> shippingSystem.shippingService "Sends order and shipping data to"
        ecommerceSystem.ecommerceApi -> notificationSystem.notificationService "Sends order confirmation and shipping notifications to"
        ecommerceSystem.customerUI -> ecommerceSystem.ecommerceApi.customerModule "Retrieves and updates customer information from"
        ecommerceSystem.customerUI -> ecommerceSystem.ecommerceApi.productModule "Retrieves product catalog from"

        ecommerceSystem.customerUI -> ecommerceSystem.ecommerceApi.orderModule "Places orders through"

        ecommerceSystem.ecommerceApi.orderModule -> accountingSystem.accountingService "Sends order data to"
        ecommerceSystem.ecommerceApi.orderModule -> shippingSystem.shippingService "Sends shipping data to"
        ecommerceSystem.ecommerceApi.orderModule -> ecommerceSystem.ecommerceApi.productModule "Retrieves product information from"
        ecommerceSystem.ecommerceApi.orderModule -> ecommerceSystem.ecommerceApi.customerModule "Retrieves customer information from"

        notificationSystem.notificationService -> customer "Sends notifications to"
    }

    views {

        systemLandscape {
            include *
            autolayout
        }

        systemContext ecommerceSystem "system_context" {
            include *
            autolayout
        }

        container ecommerceSystem "ecommerce_container" {
            include *
            autolayout lr
        }

        component ecommerceSystem.ecommerceApi "ecommerce_api_components" {
            include *
            autolayout lr
        }

        styles {
            element "Software System" {
                background #1168bd
                color #ffffff
            }
            element "Person" {
                background #08427b
                color #ffffff
                shape "Person"
            }
            element "External" {
                background #999999
                color #ffffff
            }
            element "Database" {
                shape "Cylinder"
                background #f5f5f5
                color #000000
            }

            relationship "Relationship" {
                routing Orthogonal
            }
        }
    }
}
