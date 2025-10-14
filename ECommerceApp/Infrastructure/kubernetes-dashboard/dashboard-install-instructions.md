Kubernetes Dashboard manifests and helper files

This folder contains helper manifests for installing and accessing the Kubernetes Dashboard in a local cluster.

Files:
- `dashboard-admin-user.yaml` - Creates an `admin-user` ServiceAccount in the `kubernetes-dashboard` namespace and a cluster-admin ClusterRoleBinding. Intended for local development only.
- `dashboard-nodeport.yaml` - Optional NodePort service to expose the dashboard on port 30044 on the host.

How to use:
1. Apply the official Dashboard manifest (recommended.yaml) or use the version checked into this repository if you prefer.

   ```bash
   kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/recommended.yaml
   ```

2. Apply the admin user and nodeport manifests from this directory:

   ```bash
   kubectl apply -f ECommerceApp/Infrastructure/kubernetes-dashboard/dashboard-admin-user.yaml
   kubectl apply -f ECommerceApp/Infrastructure/kubernetes-dashboard/dashboard-nodeport.yaml
   ```

3. Get the token for login (k8s 1.24+):

   ```bash
   kubectl -n kubernetes-dashboard create token admin-user
   ```

4. If not using the NodePort, port-forward the service:

   ```bash
   kubectl -n kubernetes-dashboard port-forward svc/kubernetes-dashboard 8443:443
   ```

5. Open https://localhost:8443 and paste the token.

Security note: The `admin-user` ServiceAccount is bound to `cluster-admin`. Only use this in local development clusters.
