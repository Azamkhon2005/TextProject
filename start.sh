#!/bin/bash

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${YELLOW}Starting development services...${NC}"

API_GATEWAY_DIR="./ApiGateway"
FILE_STORING_SVC_DIR="./FileStoringService.Api"
FILE_ANALYSIS_SVC_DIR="./FileAnalysisService.Api"

FILE_STORING_SWAGGER_URL="http://localhost:5001/swagger/index.html"
FILE_ANALYSIS_SWAGGER_URL="http://localhost:5002/swagger/index.html"

start_service_background() {
    PROJECT_DIR=$1
    SERVICE_NAME=$2
    PROFILE_NAME=$3

    echo -e "${GREEN}Starting $SERVICE_NAME in background...${NC}"
    cd "$PROJECT_DIR"
    if [ -z "$PROFILE_NAME" ]; then
        dotnet run & 
    else
        dotnet run --launch-profile "$PROFILE_NAME" &
    fi
    cd .. 
    echo -e "${GREEN}$SERVICE_NAME started. PID: $!${NC}"
    sleep 2 
}

start_service_background "$FILE_STORING_SVC_DIR" "File Storing Service" "FileStoringService.Api"

start_service_background "$FILE_ANALYSIS_SVC_DIR" "File Analysis Service" "FileAnalysisService.Api"

start_service_background "$API_GATEWAY_DIR" "API Gateway" "ApiGateway"


echo -e "${YELLOW}Waiting for services to initialize (approx. 10 seconds)...${NC}"
sleep 10

echo -e "${YELLOW}Opening Swagger UI pages...${NC}"

if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    OPEN_CMD="xdg-open"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    OPEN_CMD="open"
elif [[ "$OSTYPE" == "cygwin" ]] || [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "win32" ]]; then
    OPEN_CMD="start"
else
    echo "Unsupported OS for automatic browser opening. Please open URLs manually."
    OPEN_CMD=""
fi

if [ ! -z "$OPEN_CMD" ]; then
    $OPEN_CMD "$FILE_STORING_SWAGGER_URL"
    sleep 1 # Небольшая пауза между открытиями
    $OPEN_CMD "$FILE_ANALYSIS_SWAGGER_URL"
else
    echo "Please open manually:"
    echo "- File Storing Service Swagger: $FILE_STORING_SWAGGER_URL"
    echo "- File Analysis Service Swagger: $FILE_ANALYSIS_SWAGGER_URL"
fi

echo -e "${GREEN}All services should be running. API Gateway is at http://localhost:5000 ${NC}"
echo -e "${YELLOW}To stop services, you might need to find and kill the 'dotnet' processes manually or close the terminal windows they are running in.${NC}"
echo -e "${YELLOW}Alternatively, press Ctrl+C in this terminal if you didn't run them in truly detached background windows.${NC}"