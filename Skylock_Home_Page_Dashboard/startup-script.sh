#!/bin/bash

> /usr/local/openresty/nginx/logs/allowed.log
> /usr/local/openresty/nginx/logs/ai_model.log
> /usr/local/openresty/nginx/logs/error.log
> /usr/local/openresty/nginx/logs/rules_engine.log
> /usr/local/openresty/nginx/logs/modsec_audit.log
> /usr/local/openresty/nginx/logs/modsec_parsed.jsonl
> /usr/local/openresty/nginx/logs/top_blocked.json

exit 0