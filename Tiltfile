# Tiltfile for ModularMonolith ECommerceApp

# Load Docker image
docker_build(
    'modularmonolith',
    '.',
    dockerfile='ECommerceApp/Dockerfile'
)

# Apply Kubernetes manifests
k8s_yaml([
    'ECommerceApp/k8s/sealedsecret.yaml',
    'ECommerceApp/k8s/hpa.yaml',
    'ECommerceApp/k8s/deployment.yaml',
    'ECommerceApp/k8s/ingress.yaml',
    'ECommerceApp/k8s/service.yaml',
])

# Resource configuration
k8s_resource(
    'modularmonolith',  # This should match the name in your deployment.yaml
    port_forwards=['8080:8080'],
    labels=["app"]
)

# Enable file watching for live updates
watch_file('ECommerceApp/**')
watch_file('ECommerce.Common/**')
watch_file('ECommerce.Contracts/**')
watch_file('ECommerce.BusinessEvents/**')
watch_file('ECommerce.Modules.Customers/**')
watch_file('ECommerce.Modules.Orders/**')
watch_file('ECommerce.Modules.Products/**')

# Specify where to find logs
k8s_resource(
    'modularmonolith'
)

# Let's add metrics server if it's needed
k8s_yaml('ECommerceApp/k8s/metrics-server.yaml')
k8s_yaml('ECommerceApp/k8s/metrics-rbac.yaml')
