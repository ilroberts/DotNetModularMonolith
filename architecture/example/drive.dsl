workspace "Drive" "RMV Modernization" {

  !identifiers hierarchical
  
  model {
    
    externalUser = person "External User" "An external user of the drive system" "Person"
    dealershipUser = person "Dealership User" "A user from an authorized dealership managing vehicle sales and registrations" "Person"
    supportEngineer = person "Support Engineer" "Provides technical support for the Drive system" "Person"
    databaseAdministrator = person "Database Administrator" "Manages and maintains the Drive system databases" "Person"
    cloudAdministrator = person "Cloud Administrator" "Manages cloud infrastructure and services for the Drive system" "Person"
    devOpsEngineer = person "DevOps Engineer" "Responsible for the deployment and maintenance of the Drive system" "Person"
    group "Register of Motor Vehicles" {
        internalUser = person "Internal User" "An internal user of the drive system" "Person"

        drive = softwareSystem "Drive" "The RMV Modernization Drive system" {
          perspectives {
            "owner" "RMV Platform Team"
            "serviceTier" "gold"
            "trustBoundary" "internal"
          }
          group "APIs" {
            driveApi = container "Drive API" "The API for the Drive system" "C# .NET Core" {
                group "Domain Modules" {
                  clientModule = component "Client Module" "Client profile functionality" "C# .Net Core"
                  vehicleModule = component "Vehicle Module" "Vehicle functionality" "C# .Net Core"
                  roadTestModule = component "Road Test Module" "Road test functionality" "C# .Net Core"
                  notifyModule = component "Notify Module" "Notification functionality for Email and SMS" "C# .Net Core"
                  auditingModule = component "Auditing Module" "Tracks and logs system activities and data changes" "C# .Net Core"
                }
                apiHost = component "Web API" "Surfaces the Drive Modules" "C# .Net Core"
                paymentModule = component "    Payment Module    " "Handles payment integration with NS Pay" "C# .Net Core"
                contracts = component "Contracts" "Contracts for inter module method calls" "C# .Net Core"
                acl = component "Anti Corruption Layer" "Maps between the Drive and RMV3 domains" "C# .Net Core"
                aggregationModule = component "Aggregation Module" "Aggregates data from multiple modules" "C# .Net Core"
            }
            manageClientApi = container "Manage Client API" "The legacy API for managing client profiles" "C# .NET Core" "API" 
            driveApiGateway = container "Drive API Gateway" "Authentication proxy for validating driver license information with My NS Account" "C# .NET Core" "API"
          }
          
          group "User Interfaces" {
            externalUi = container "External UI" "The external user interface for the Drive system" "C# .NET Core"
            internalUi = container "Internal UI" "The internal user interface for the Drive system" "C# .NET Core"
            mdsUi = container "MDS UI" "The UI for the Manage Driver Suspensions" "C# .NET Core"
            cart = container "Manage Wait Time UI" "The UI for Manage Wait Time/CaRT" "C# .NET Core" {
              frontendcontroller = component "Frontend Controller" "The controller for the Manage Wait Time UI" "C# .NET Core"
              locationSelector = component "Test Location Selector" "Uses reference data to determine whether location has been migrated to BRT" "C# .NET Core"
              brtService = component "BRT Service" "The service for the BRT location" "C# .NET Core"
              rmv3Service = component "RMV3 Service" "The service for the RMV3 location" "C# .NET Core"
            }
          }

          group "Databases" {
            clientDatabase = container "Client Database" "The database for the Drive client profiles" "SQL Server" "Database" {
              perspectives {
                "security" "data should be encrypted at rest" "status: false"
                "performance" "data should be indexed for performance" "status: true"
              }
            }
            vehicleDatabase = container "Vehicle Database" "The database for the Drive vehicle profiles" "SQL Server" "Database" 
            roadTestDatabase = container "Road Test Database" "The database for the Drive road test appointments" "SQL Server" "Database"
            auditDatabase = container "Audit Database" "The database for storing audit logs and system activity tracking" "SQL Server" "Database"
            redisCache = container "Redis Session Cache" "Session cache for Drive Internal and External UIs" "Redis" "Cache"
          }
        }
        
        rmv3 = softwareSystem "RMV3" "Legacy RMV system"  "External" {

          application = container "RMV3 Application" "The RMV3 application" "Java" "External"
          api = container "RMV3 API" "API interface for the RMV3 system" "Java" "External"
          database = container "RMV3 Database" "The RMV3 database" "Oracle RDBMS" "Database"
        }

        cloudflare = softwareSystem "Cloudflare" "Cloudflare Application Gateway and WAF" "External" {
          gateway = container "Cloudflare Application Gateway" "The Cloudflare application gateway and WAF" "Cloudflare" "External"
        }

        ods = softwareSystem "Online Dealership System" "System for managing dealership operations and vehicle sales" {
          group "User Interfaces" {
            odsUi = container "ODS Frontend" "React-based frontend application for dealership operations" "React" "Web Application"
          }
          
          group "APIs" {
            odsApi = container "ODS API" "API for the Online Dealership System" "C# .NET Core" "API"
          }
          
          group "Databases" {
            odsDatabase = container "ODS Database" "Database for dealership and vehicle inventory data" "Oracle RDBMS" "Database"
          }
        }
    }

    group "Azure Services" {
      dynamics = softwareSystem "Dynamics 365" "Road test schedule information" "External" 
      entraId = softwareSystem "Azure Entra Id" "Authenticate internal users" "External" 
    }

    azure = softwareSystem "Azure Tenant" {
      blobStorage = container "Azure Blob Storage" "Stores uploaded legislative documents" "External"
      openAi = container "Azure Open AI" "Generates responses based on legislative documents" "External"
      cognitiveSearch = container "Azure Cognitive Search" "Generates responses based on legislative documents" "External"
      adf = container "Azure Data Factory" "Data integration service" "External"
      dataSyncServiceBus = container "Data Sync Service Bus" "Service Bus for RMV3 to Drive data sync" "External"
      dataSyncFunction = container "Data Sync Azure Function" "Processes data sync messages from Service Bus" "External"
    }
    
    vintelligence = softwareSystem "Vintelligence" "VIN lookup API" "External"

    fileNet = softwareSystem "FileNet" "Document management and file storage system" "External"
    
    MyNsAccount = softwareSystem "My NS Account" "Nova Scotia Identity and Authentication system" "External"

    nsNotify = softwareSystem "NS Notify" "Nova Scotia notification service for sending Email and SMS notifications" "External"

    nsPay = softwareSystem "NS Pay" "Nova Scotia Payment Gateway" "External" {
      group "NS Pay" {
        hostedPaymentUI = container "Hosted Payment UI" "The payment interface for users" "External"
        paymentService = container "Payment Service" "The backend payment processing service" "External"
      }
    }

    // Current externalUser relationships:
    externalUser -> cloudflare "Book a road test"
    internalUser -> cloudflare "Manage client and vehicle information"
    internalUser -> rmv3.application "Manage driver and vehicle information"
    externalUser -> cloudflare.gateway "Routes to External UI"
    nsPay.hostedPaymentUI -> externalUser "Display payment UI"
    externalUser -> nsPay.hostedPaymentUI "Complete payment"

    // New actor relationships
    supportEngineer -> drive "Provides technical support and incident response"
    databaseAdministrator -> drive.clientDatabase "Manages and maintains client database"
    databaseAdministrator -> drive.vehicleDatabase "Manages and maintains vehicle database"
    databaseAdministrator -> drive.roadTestDatabase "Manages and maintains road test database"
    databaseAdministrator -> drive.auditDatabase "Manages and maintains audit database"
    cloudAdministrator -> drive "Manages cloud infrastructure and services"
    devOpsEngineer -> drive "Responsible for the deployment and maintenance of the Drive system"
    
    // My NS Account authentication relationships
    externalUser -> MyNsAccount "Authenticate with driver licence"
    drive.externalUi -> MyNsAccount "Redirect for authentication"
    MyNsAccount -> drive.driveApiGateway "Validate driver licence"
    drive.driveApiGateway -> drive.driveApi "Validate driver licence"
    MyNsAccount -> drive.externalUi "Redirect after successful authentication"
    
    drive -> vintelligence "VIN lookup"
    internalUser -> dynamics "Schedule road tests"
    drive -> dynamics "Get road test schedules"
    drive -> entraId "Authenticate internal users"

    # ODS (Online Dealership System) relationships
    dealershipUser -> cloudflare.gateway "Access dealership system"
    cloudflare.gateway -> ods.odsUi "Routes to ODS Frontend"
    ods.odsUi -> ods.odsApi "Manage dealership operations"
    ods.odsApi -> ods.odsDatabase "Store/retrieve dealership data"
    ods.odsApi -> rmv3.api "Get/Update vehicle registration data"
    ods.odsApi -> vintelligence "VIN lookup and validation"
    ods.odsApi -> fileNet "Store/retrieve dealership documents"
    ods.odsUi -> MyNsAccount "Authenticate dealership users"

    # RMV3 internal relationships
    rmv3.api -> rmv3.application "Process registration requests"
    rmv3.application -> rmv3.database "Get/Update RMV3 data"

    # data synchronization relationships
    azure.adf -> rmv3.database "Monitor data updates"
    azure.adf -> drive "Apply data updates"
    azure.adf -> azure.dataSyncFunction "Capture database changes"
    azure.dataSyncFunction -> azure.dataSyncServiceBus "Writes changes to topic"
    drive.driveApi.clientModule -> azure.dataSyncServiceBus "Reads changes from topic"
    drive.driveApi.vehicleModule -> azure.dataSyncServiceBus "Reads changes from topic"
    drive.driveApi.vehicleModule -> drive.vehicleDatabase "Update vehicle profile"
    
    drive.internalUi -> entraId "Authenticate internal user"
    
    drive.driveApi -> drive.manageClientApi "Uses"
    drive.manageClientApi -> rmv3.application "Get/Update driver and vehicle details"
    
    # CaRT relationships
    drive.cart -> drive.roadTestDatabase "Get/Update road test data"
    drive.cart -> rmv3.application "Get/Update appointment data"
    drive.cart -> drive.driveApi "Get/Update appointment data (BRT)"
    drive.driveApi -> drive.roadTestDatabase "Get/Update road test data (BRT)"
    rmv3.application -> rmv3.database "Get/Update appointment data"
    drive.mdsUi -> drive.manageClientApi "Get/Update suspension data"
    drive.cart.frontendcontroller -> drive.cart.locationSelector "Get location reference data"
    drive.cart.frontendcontroller -> drive.cart.brtService "Get/Update appointment data (BRT)"
    drive.cart.frontendcontroller -> drive.cart.rmv3Service "Get/Update appointment data (RMV3)"
    drive.cart.brtService -> drive.driveApi "Get/Update appointment data (BRT)"
    drive.cart.rmv3Service -> rmv3.application "Get/Update appointment data (RMV3)"
    
    drive.manageClientApi -> drive.clientDatabase "Uses"
    drive.manageClientApi -> rmv3.application "Get/Update RMV3 data"

    drive.externalUi -> drive.driveApi "Uses"
    drive.driveApi -> drive.clientDatabase "Get/Update client profile data"
    drive.driveApi -> drive.vehicleDatabase "Get/Update vehicle data"
    drive.driveApi -> drive.roadTestDatabase "Get/Update road test data"
    drive.driveApi -> vintelligence "VIN lookup"
    
    drive.driveApi -> dynamics "Get/Update road test schedules"

    drive.driveApi -> azure.blobStorage "Store legislative documents"
    drive.driveApi -> azure.openAi "Generate responses"
    drive.driveApi -> azure.cognitiveSearch "Generate indexes and responses"
    azure.cognitiveSearch -> azure.blobStorage "Generate index from documents"
    
    # Payment relationships
    drive.externalUi -> drive.driveApi.apiHost "Retrieve workstream specific payment data"
    drive.driveApi.apiHost -> drive.driveApi.roadTestModule "Retrieve workstream specific payment data"
    drive.externalUi -> drive.driveApi.apiHost "Retrieve payment service specific data"
    drive.driveApi.apiHost -> drive.driveApi.paymentModule "Retrieve payment service specific data"
    drive.externalUi -> nsPay.hostedPaymentUI "Redirect to hosted payment UI using payment service data"
    nsPay.hostedPaymentUI -> drive.externalUi "Redirect back using return or cancel URLs"
    drive.externalUi -> drive.driveApi.apiHost "Confirm payment via payment module"
    drive.driveApi.apiHost -> drive.driveApi.paymentModule "Confirm payment via payment module"
    drive.driveApi.paymentModule -> nsPay.paymentService "Confirm payment with payment service"
    drive.externalUi -> externalUser "Display payment result notification"

    # relationships to/from components
    drive.externalUi -> drive.driveApi.apiHost "Uses"
    drive.internalUi -> drive.driveApi.apiHost "Uses"
    drive.externalUi -> drive.redisCache "Uses for session caching"
    drive.internalUi -> drive.redisCache "Uses for session caching"
    
    drive.driveApi.apiHost -> drive.driveApi.clientModule "Uses"
    drive.driveApi.apiHost -> drive.driveApi.vehicleModule "Uses"
    drive.driveApi.apiHost -> drive.driveApi.roadTestModule "Uses"
    drive.driveApi.apiHost -> drive.driveApi.auditingModule "Uses"
    # Notification relationships
    drive.driveApi.roadTestModule -> drive.driveApi.contracts "Request notification (Email/SMS)"
    drive.driveApi.contracts -> drive.driveApi.notifyModule "Send notification request"
    
    # Auditing relationships
    drive.driveApi.clientModule -> drive.driveApi.contracts "Log client activities"
    drive.driveApi.vehicleModule -> drive.driveApi.contracts "Log vehicle activities"
    drive.driveApi.roadTestModule -> drive.driveApi.contracts "Log road test activities"
    drive.driveApi.paymentModule -> drive.driveApi.contracts "Log payment activities"
    drive.driveApi.contracts -> drive.driveApi.auditingModule "Send audit events"
    drive.driveApi.clientModule -> drive.clientDatabase "Uses"
    drive.driveApi.clientModule -> drive.driveApi.acl "Get/Update client data"
    drive.driveApi.vehicleModule -> drive.driveApi.acl "Get/Update vehicle data"
    drive.driveApi.auditingModule -> drive.auditDatabase "Log audit events and system activities"
    
    drive.driveApi.acl -> rmv3.application "Get/Update RMV3 data"
    
    drive.driveApi.vehicleModule -> drive.driveApi.contracts "Get/Update client profile"
    drive.driveApi.contracts -> drive.driveApi.clientModule "Get/Update client profile"
    drive.driveApi.vehicleModule -> vintelligence "VIN lookup"
    
    drive.driveApi.roadTestModule -> dynamics "Get/Update road test schedules"
    drive.driveApi.roadTestModule -> drive.driveApi.contracts "Get/Update client profile"

    drive.driveApi.notifyModule -> nsNotify "Send Email and SMS notifications"

    # aggregation of data from multiple modules
    drive.driveApi.apiHost -> drive.driveApi.aggregationModule "Get service restrictions"
    drive.driveApi.aggregationModule -> drive.driveApi.contracts "Get client service restrictions"
    drive.driveApi.aggregationModule -> drive.driveApi.contracts "Get vehicle service restrictions"
    drive.driveApi.aggregationModule -> drive.driveApi.clientModule "Get client profile (service restrictions)"
    drive.driveApi.aggregationModule -> drive.driveApi.vehicleModule "Get vehicle profile (service restrictions)"
    drive.driveApi.contracts -> drive.driveApi.vehicleModule "Get/Update vehicle data"

    # cloudflare load balancer relationships
    internalUser -> cloudflare.gateway "Routes to"
    externalUser -> cloudflare.gateway "Routes to"
    dealershipUser -> cloudflare.gateway "Routes to"
    cloudflare.gateway -> drive.externalUi "Routes to"
    cloudflare.gateway -> drive.internalUi "Routes to"
    cloudflare.gateway -> drive.mdsUi "Routes to"
    cloudflare.gateway -> drive.cart "Routes to"
    cloudflare.gateway -> ods.odsUi "Routes to"
    cloudflare.gateway -> drive.driveApiGateway "Routes to Drive API Gateway"

    # deployment diagrams
    development = deploymentEnvironment "RMV Modernization: Development/Test" {
      deploymentNode "Drive OpenShift\n(ext.novascotia.ca)" {
        containerInstance drive.driveApi "Drive API" "API" 
        containerInstance drive.externalUi "External UI" "Web Application"
        containerInstance drive.internalUi "Internal UI" "Web Application"
        containerInstance drive.redisCache "Redis Session Cache" "Cache"
        containerInstance drive.cart "Manage Wait Time UI" "Web Application"
        containerInstance rmv3.api "RMV API Gateway" "External"
        containerInstance drive.driveApiGateway "Drive API Gateway" "API"
      }

      deploymentNode "MDS OpenShift\n(nonprod-hfx1.novascotia.ca)" {
        containerInstance drive.mdsUi "MDS UI" "Web Application"
        containerInstance drive.manageClientApi "Manage Client API" "API"
      }

      deploymentNode "ODS OpenShift\n(ext.novascotia.ca)" {
        containerInstance ods.odsUi "ODS Frontend" "Web Application"
        containerInstance ods.odsApi "ODS API" "API"
      }

      deploymentNode "Drive Database\n(db10173.prov.gov.ns.ca)" {
        containerInstance drive.clientDatabase "Client Database" "Database"
        containerInstance drive.vehicleDatabase "Vehicle Database" "Database"
        containerInstance drive.roadTestDatabase "Road Test Database" "Database"
        containerInstance drive.auditDatabase "Audit Database" "Database"
      }

      deploymentNode "ODS Database\n(ods-db.prov.gov.ns.ca)" {
        containerInstance ods.odsDatabase "ODS Database" "Database"
      }

      deploymentNode "RMV3-1" {
        containerInstance rmv3.application "RMV3" "External" 
        containerInstance rmv3.database "RMV3 Database" "Database" 
      }
    }

production = deploymentEnvironment "RMV Modernization: Production" {

      deploymentNode "Cloudflare" {
        containerInstance cloudflare.gateway "Cloudflare Application Gateway" "External" 
      }
      
      deploymentNode "Drive OpenShift\n(prod-hfx2.novascotia.ca)" {
        containerInstance drive.driveApi "Drive API" "API" 
        containerInstance drive.externalUi "External UI" "Web Application" 
        containerInstance drive.internalUi "Internal UI" "Web Application"       
        containerInstance drive.redisCache "Redis Session Cache" "Cache"
        containerInstance drive.cart "Manage Wait Time UI" "Web Application" 
        containerInstance drive.mdsUi "MDS UI" "Web Application" 
        containerInstance drive.manageClientApi "Manage Client API" "API"
        containerInstance ods.odsUi "ODS Frontend" "Web Application"
        containerInstance ods.odsApi "ODS API" "API"
        containerInstance rmv3.api "RMV API Gateway" "External"
        containerInstance drive.driveApiGateway "Drive API Gateway" "API"
      }

      deploymentNode "Drive Database\n(db10181.prov.gov.ns.ca)" {
        containerInstance drive.clientDatabase "Client Database" "Database"
        containerInstance drive.vehicleDatabase "Vehicle Database" "Database"
        containerInstance drive.roadTestDatabase "Road Test Database" "Database"
        containerInstance drive.auditDatabase "Audit Database" "Database"
      }

      deploymentNode "ODS Database\n(ods-prod-db.prov.gov.ns.ca)" {
        containerInstance ods.odsDatabase "ODS Database" "Database"
      }

      deploymentNode "RMV3-1" {
        containerInstance rmv3.application "RMV3" "External" 
        containerInstance rmv3.database "RMV3 Database" "Database"
      }
    }
  }

  views {

    systemLandscape {
      include *
      autolayout
    }

    systemContext drive "system_context" {
      include *
      include internalUser
      autolayout
    }

    container drive "drive_container" {
      include *
      autolayout lr
    }

    container azure "azure_tenant_container" {
      include *
      autolayout lr
    }

    systemContext dynamics "dynamics_context" {
      include *
      autolayout
    }

    systemContext ods "ods_system_context" {
      include *
      include dealershipUser
      autolayout
    }

    container ods "ods_container" {
      include *
      autolayout lr
    }

    dynamic drive {
      title "Drive Service Eligibility (High Level)"

      1: internalUser -> cloudflare.gateway "Get client profile (service eligibility)"
      2: cloudflare.gateway -> drive.internalUi "Upload legislative documents"
      3: drive.internalUi -> drive.driveApi "Upload legislative documents"
    }

    dynamic drive.driveApi {
      title "Drive Service Eligibility (DriveAPI)"
      
      1: drive.driveApi.apiHost -> drive.driveApi.aggregationModule "Get service eligibility"
      2: drive.driveApi.aggregationModule -> drive.driveApi.contracts "Get client service eligibility"
      3: drive.driveApi.aggregationModule -> drive.driveApi.clientModule "Get client profile (service eligibility)"
      4: drive.driveApi.clientModule -> drive.clientDatabase "Get drive client profile (service eligibility)"
      5: drive.driveApi.clientModule -> drive.driveApi.acl "Get RMV3 client profile (service eligibility)"
      6: drive.driveApi.aggregationModule -> drive.driveApi.vehicleModule "Get vehicle service eligibility"
      7: drive.driveApi.vehicleModule -> drive.vehicleDatabase "Get drive vehicle profile (service eligibility)"
      8: drive.driveApi.vehicleModule -> drive.driveApi.acl "Get RMV3 vehicle profile (service eligibility)"
    }

    dynamic  drive {
      title "AI Implementation"
      
      1: internalUser -> cloudflare.gateway "Upload legislative documents"
      2: cloudflare.gateway -> drive.internalUi "Upload legislative documents"
      3: drive.internalUi -> drive.driveApi "Upload legislative documents"
      4: drive.driveApi -> azure.blobStorage "Store legislative documents"

      5: azure.cognitiveSearch -> azure.blobStorage "Generate index from documents"

      6: externalUser -> cloudflare.gateway "Query legislation"
      7: cloudflare.gateway -> drive.externalUi "Query legislation"
      8: drive.externalUi -> drive.driveApi "Query legislation"
      9: drive.driveApi -> azure.openAi "Generate responses"
      10: drive.driveApi -> azure.cognitiveSearch "Generate indexes and responses"
    }

    dynamic drive "data_sync"{
      title "Data Synchronization"

      1: azure.adf -> rmv3.database "Monitor data updates"
      2: azure.adf -> azure.dataSyncFunction "Receives database changes"
      3: azure.dataSyncFunction -> azure.dataSyncServiceBus "Writes data and metadata to topic"
      4: drive.driveApi -> azure.dataSyncServiceBus "Reads data and metadata from topic"
      5: drive.driveApi -> drive.clientDatabase "Update client profile"
      6: drive.driveApi -> drive.vehicleDatabase "Update vehicle profile"
    }

    dynamic drive.driveApi "data_sync_detail" {
      title "Data Synchronization Detail"

      1: drive.driveApi.clientModule -> azure.dataSyncServiceBus "Reads changes from topic"
      2: drive.driveApi.clientModule -> drive.clientDatabase "Update client profile"
      3: drive.driveApi.vehicleModule -> azure.dataSyncServiceBus "Reads changes from topic"
      4: drive.driveApi.vehicleModule -> drive.vehicleDatabase "Update vehicle profile"
    }
    
    dynamic drive "cart_flow" {
      title "Manage Wait Time (CaRT) Flow"

      1: internalUser -> cloudflare.gateway "Book or manage road test"
      2: cloudflare.gateway -> drive.cart "Route to Manage Wait Time UI"
      
      # Branch based on location migration status
      3: drive.cart -> rmv3.application "Get/Update appointment data (legacy location)"
      4: rmv3.application -> rmv3.database "Get/Update appointment data"
      
      5: drive.cart -> drive.driveApi "Get/Update appointment data (BRT location)"
      6: drive.driveApi -> drive.roadTestDatabase "Get/Update road test data"
      7: drive.driveApi -> dynamics "Get/Update road test schedules in Dynamics 365"
    }

    dynamic drive.cart "cart_flow_component" {
      title "Manage Wait Time (CaRT) Flow (Component Detail)"

      1: drive.cart.frontendcontroller -> drive.cart.locationSelector "Get location reference data"
      2: drive.cart.frontendcontroller -> drive.cart.brtService "Get/Update appointment data (BRT)"
      3: drive.cart.brtService -> drive.driveApi "Get/Update appointment data (BRT)"
      4: drive.cart.frontendcontroller -> drive.cart.rmv3Service "Get/Update appointment data (RMV3)"
      5: drive.cart.rmv3Service -> rmv3.application "Get/Update appointment data (RMV3)"
    }

    dynamic drive.driveApi "payment_flow" {
      title "NS Pay Payment Flow"
      
      1: externalUser -> cloudflare.gateway "Clicks Pay Now"
      2: cloudflare.gateway -> drive.externalUi "Route to External UI" 
      3: drive.externalUi -> drive.driveApi.apiHost "Retrieve workstream specific payment data"
      4: drive.driveApi.apiHost -> drive.driveApi.roadTestModule "Retrieve workstream specific payment data"
      5: drive.driveApi.roadTestModule -> drive.driveApi.apiHost "Return payment data"
      6: drive.externalUi -> drive.driveApi.apiHost "Retrieve payment service specific data"
      7: drive.driveApi.apiHost -> drive.driveApi.paymentModule "Retrieve payment service specific data"
      8: drive.driveApi.paymentModule -> drive.driveApi.apiHost "Return payment service data"
      9: drive.driveApi.apiHost -> drive.externalUi "Return payment service data"
      10: drive.externalUi -> nsPay.hostedPaymentUI "Redirect to hosted payment UI using payment service data"
      
      11: nsPay.hostedPaymentUI -> externalUser "Display payment UI"
      12: externalUser -> nsPay.hostedPaymentUI "Complete payment"
      13: nsPay.hostedPaymentUI -> drive.externalUi "Redirect back using return or cancel URLs"
      14: drive.externalUi -> drive.driveApi.apiHost "Confirm payment"
      15: drive.driveApi.apiHost -> drive.driveApi.paymentModule "Confirm payment with NS Pay"
      16: drive.driveApi.paymentModule -> nsPay.paymentService "Confirm payment with payment service"
      17: nsPay.paymentService -> drive.driveApi.paymentModule "Payment confirmation response"
      18: drive.driveApi.paymentModule -> drive.driveApi.apiHost "Payment confirmation response"
      19: drive.driveApi.apiHost -> drive.externalUi "Display payment confirmation"
      20: drive.externalUi -> externalUser "Display payment result notification"
    }

    dynamic ods "dealership_workflow" {
      title "Online Dealership System Workflow"
      
      1: dealershipUser -> cloudflare.gateway "Access dealership portal"
      2: cloudflare.gateway -> ods.odsUi "Route to ODS Frontend"
      3: ods.odsUi -> MyNsAccount "Authenticate dealership user"
      4: ods.odsUi -> ods.odsApi "Load authenticated dealership data"
      5: ods.odsApi -> ods.odsDatabase "Retrieve dealership inventory"
      6: ods.odsApi -> rmv3.api "Get vehicle registration requirements"
      7: rmv3.api -> rmv3.application "Process registration request"
      8: rmv3.application -> rmv3.database "Retrieve registration data"
      9: ods.odsApi -> ods.odsUi "Display inventory and registration status"
      10: ods.odsUi -> ods.odsApi "Process vehicle sale with VIN"
      11: ods.odsApi -> vintelligence "Validate VIN and get vehicle details"
      12: ods.odsApi -> ods.odsDatabase "Update inventory and sale records"
      13: ods.odsApi -> fileNet "Store sale documentation"
      14: ods.odsApi -> rmv3.api "Submit registration update"
      15: rmv3.api -> rmv3.application "Process registration update"
    }

    dynamic drive "my_ns_account_auth" {
      title "My NS Account Authentication Flow"
      
      1: externalUser -> cloudflare.gateway "Access External UI"
      2: cloudflare.gateway -> drive.externalUi "Route to External UI"
      3: drive.externalUi -> MyNsAccount "Redirect for authentication"
      4: MyNsAccount -> externalUser "Request driver licence information"
      5: externalUser -> MyNsAccount "Provide driver licence information"
      6: MyNsAccount -> drive.driveApiGateway "Validate driver licence"
      7: drive.driveApiGateway -> drive.driveApi "Validate driver licence"
      8: drive.driveApi -> drive.clientDatabase "Check driver licence validity"
      9: drive.driveApi -> drive.driveApiGateway "Return validation result"
      10: drive.driveApiGateway -> MyNsAccount "Return validation result"
      11: MyNsAccount -> drive.externalUi "Redirect after successful authentication"
      12: drive.externalUi -> externalUser "Display authenticated user interface"
    }

    component drive.driveApi "drive_api_component" {
        include *
        autolayout lr
    }

    deployment * development {
      description "Deployment Diagram for Drive Modernization Dev/Test"
      include *
      autolayout lr
    }

    deployment * production {
      description "Deployment Diagram for Drive Modernization Production"
      include *
      autolayout tb
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
