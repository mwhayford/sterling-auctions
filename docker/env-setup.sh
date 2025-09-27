#!/bin/sh

# Environment setup script for frontend container
# This script sets up environment variables for the React app

# Set default API URL if not provided
if [ -z "$REACT_APP_API_URL" ]; then
    export REACT_APP_API_URL="http://localhost:5000"
fi

# Set default SignalR URL if not provided
if [ -z "$REACT_APP_SIGNALR_URL" ]; then
    export REACT_APP_SIGNALR_URL="http://localhost:5000"
fi

# Set default environment
if [ -z "$REACT_APP_ENVIRONMENT" ]; then
    export REACT_APP_ENVIRONMENT="development"
fi

echo "Environment variables set:"
echo "REACT_APP_API_URL=$REACT_APP_API_URL"
echo "REACT_APP_SIGNALR_URL=$REACT_APP_SIGNALR_URL"
echo "REACT_APP_ENVIRONMENT=$REACT_APP_ENVIRONMENT"
