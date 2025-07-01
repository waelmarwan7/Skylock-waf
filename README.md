Generate a New Service Account Key (If Compromised) If the key was exposed in Git history:
Go to Google Cloud Console

Navigate to IAM & Admin → Service Accounts

Select your service account → Keys tab

Delete the exposed key (click 🗑️)

Create new key:

Click + Create Key → JSON

The key file will download automatically



#Activate for current session
gcloud auth activate-service-account --key-file=service-account.json

#set google cloud project defualt on the server
gcloud config set project PROJECT_ID




cloud design : https://lucid.app/lucidchart/653b6064-8614-4b5c-a439-d4f0fa0e74bb/edit?viewport_loc=-1206%2C-1031%2C7461%2C3444%2CHowzCfBBZfS3&invitationId=inv_cc63fb54-d18e-45d9-b8c4-75c0f7fe9e22
