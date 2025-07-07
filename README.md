# SkyLock Overview

(Cloud Design](https://lucid.app/lucidchart/653b6064-8614-4b5c-a439-d4f0fa0e74bb/edit?viewport_loc=-1206%2C-1031%2C7461%2C3444%2CHowzCfBBZfS3&invitationId=inv_cc63fb54-d18e-45d9-b8c4-75c0f7fe9e22)


(System image Link](https://drive.google.com/file/d/1FExwSmgswaue6Ygw_5T2fkShns-ckD_E/view?usp=sharing](https://drive.google.com/file/d/1XtX9uBUTntPDa3CXv7CdmXowPeqtMa_u/view?usp=sharing)


![image](https://github.com/user-attachments/assets/35f638f0-6c7e-47b3-b612-8df1c6cf68c7)

## Setup Project

### Create Instance named "Home-waf-page"

- Generate a New Service Account Key (If Compromised) If the key was exposed in Git history: Go to Google Cloud Console
- Navigate to IAM & Admin → Service Accounts
- Select your service account → Keys tab
- Delete the exposed key (click 🗑️)
- Create new key:
- Click + Create Key → JSON
> The key file will download automatically

### Download the image from google drive in your local.
- copy the downloaded instance image to your google bucket in your project.
  
```bash
gsutil -d cp "YOUR/FILE/LOCATION" gs://YOUR-NAME-BUCKET/
```

### Import the image in the compute images
```bash
gcloud compute images create final-image --source-uri=gs://YOUR-BUCKET-NAME/final-image-export.tar.gz --storage-location=us-central1
```

> Use this command in home-waf-page server in google sdk

- Copy the folder skylock from this repo to the windows server
- Put the service-account key file in the folder

### Activate the service account on the home-page-server

```bash
gcloud auth activate-service-account --key-file=service-account.json
```

### Set google cloud project defualt on the server
```bash
cloud config set project PROJECT_ID
```

> Put ssl certificate inside the folder also.

### Finally run the project by copy this command in cmd inside folder location

```bash
dotnet run
```






