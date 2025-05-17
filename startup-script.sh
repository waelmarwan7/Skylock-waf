#!/bin/bash

rm /usr/local/openresty/nginx/logs/allowed.log
rm /usr/local/openresty/nginx/logs/ai_model.log
rm /usr/local/openresty/nginx/logs/error.log
rm /usr/local/openresty/nginx/logs/rules_engine.log
rm /var/log/modsec_audit.log
 
touch /usr/local/openresty/nginx/logs/allowed.log
touch /usr/local/openresty/nginx/logs/ai_model.log
touch /usr/local/openresty/nginx/logs/error.log
touch /usr/local/openresty/nginx/logs/rules_engine.log
touch /var/log/modsec_audit.log


# Get the metadata value for target-ip
TARGET_IP="{{TARGET_IP}}"




# Validate we got an IP
if [ -z "$TARGET_IP" ]; then
  echo "Error: No target-ip metadata found"
  exit 1
fi

NGINX_CONF="/usr/local/openresty/nginx/conf/nginx.conf"

# Check if the file exists
if [ ! -f "$NGINX_CONF" ]; then
  echo "Error: nginx.conf not found at $NGINX_CONF"
  exit 1
fi

# Update the proxy_pass directive
echo "Updating proxy_pass to use $TARGET_IP"
sed -i "s|proxy_pass http.*;|proxy_pass https://$TARGET_IP;|g" "$NGINX_CONF"

# Validate the change
if ! grep -q "proxy_pass https://$TARGET_IP;" "$NGINX_CONF"; then
  echo "ERROR: Failed to update nginx.conf"
  exit 1
fi

# Reload OpenResty
echo "Reloading OpenResty configuration"
openresty -s reload

echo "Successfully updated nginx configuration"
exit 0