# Tiltfile for ModularMonolith ECommerceApp

# Load Docker images
docker_build(
    'modularmonolith',
    '.',
    dockerfile='ECommerceApp/Dockerfile'
)

docker_build(
    'admin-ui',
    '.',
    dockerfile='ECommerce.AdminUI/Dockerfile'
)

# Adding database migrator with correct image name to match migrate-job.yaml
docker_build(
    'ecommerce-db-migrator',  # Match the image name in migrate-job.yaml
    '.',
    dockerfile='ECommerce.DatabaseMigrator/Dockerfile'
)

# Apply Kubernetes manifests
k8s_yaml([
    'ECommerceApp/k8s/sealedsecret.yaml',
    'ECommerceApp/k8s/hpa.yaml',
    'ECommerceApp/k8s/deployment.yaml',
    'ECommerceApp/k8s/ingress.yaml',
    'ECommerceApp/k8s/service.yaml',
    'ECommerce.DatabaseMigrator/k8s/migrate-job.yaml'
])

# Use kustomize for AdminUI
k8s_yaml(kustomize('ECommerce.AdminUI/k8s/overlays/dev'))

# Resource configuration
k8s_resource(
    'modularmonolith',  # This should match the name in your deployment.yaml
    port_forwards=['8080:8080'],
    labels=["app"],
    trigger_mode=TRIGGER_MODE_MANUAL
)

k8s_resource(
    'admin-ui',
    port_forwards=['8081:8080'],
    labels=["app"],
    trigger_mode=TRIGGER_MODE_MANUAL
)

k8s_resource(
    'db-migrator',  # This should match the name in migrate-job.yaml
    labels=["migrations"],
    trigger_mode=TRIGGER_MODE_MANUAL
)

# Enable file watching for live updates
watch_file('ECommerceApp/**')
watch_file('ECommerce.Common/**')
watch_file('ECommerce.Contracts/**')
watch_file('ECommerce.BusinessEvents/**')
watch_file('ECommerce.Modules.Customers/**')
watch_file('ECommerce.Modules.Orders/**')
watch_file('ECommerce.Modules.Products/**')
watch_file('ECommerce.AdminUI/**')
watch_file('ECommerce.DatabaseMigrator/**')

# Specify where to find logs
k8s_resource(
    'modularmonolith'
)

# Let's add metrics server if it's needed
k8s_yaml('Infrastructure/k8s/metrics-server.yaml')
k8s_yaml('Infrastructure/k8s/metrics-rbac.yaml')
