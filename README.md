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