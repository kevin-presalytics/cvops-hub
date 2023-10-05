Prerequisitives: 

- [ ] Create minio bucket with workspace

Upload model process:

- [ ] Create record in models collection for the the workspace
- [ ] Create presigned Url for models/{id}
= [ ] Upload via python client? "upload_model" method


Deploy 

- [ ] deplooy via python client "upload_and_deploy_to_device"
- [ ] Run model checks
- [ ] create download presigned url
- [ ] Send message over mqtt to to device with presigned url
- [ ] Device to download model 
- [ ] Device to start inference
- [ ] Device to delete old model



Command to start minio for testing: 

```bash
docker run -p 9000:9000 -d -p 9001:9001 -e "MINIO_ROOT_USER=hub" -e "MINIO_ROOT_PASSWORD=Password@1" --name minio quay.io/minio/minio server /data --console-address ":9001"
```

Guidelines for Events:

1. Any device can create an event, so long at the hub can process the event (write to db).


Deployment Sequence:

1. Device creates a deployment on the `workspace/{workspaceId}/deployments` channel with `DeploymentCreateRequest`.
2. Hub validates the deployment request.  Do the requested devices support the supplied model type?  Do the requested devices exist inthe workspce?
3. Hub Response to the with the deployment created platform event on the `events/workspace/{workspaceId}`channel.  For an invalid request, use the DeploymentFailed Status.  
3. Where device id matches deploymentInitiatorId, device sends an and `UploadUrlRequest` message `workspace/{workspaceId}/deployments`
4. Hub Reponds to the Repsonse topic, typically `device/{deviceId}/commands` with and `UploadUrlRepsonse` containing a signed Url for storage
5. device emits a `DeploymentUpdated` platform event with status `ModelUploading`.
5. Device uploads to to Url. When upload is complete, device emits a `DeploymentUpdated` platform event with status `ModelReady`.
8. Target devices request a model download Url.
9. Target device emits a `DeploymentUpdated` with `ModelDownloading` status
10. Target deice emits a `DeploymentUpdated` with `Active` status


