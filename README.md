# Flow
- Frontend connects to server (a.k.a. Uploader service) via websocket in order to get real-time updates on the statuses of the files sent to convert.
- When client sends file for conversion it is saved in DB and message sent to Converter service.
- Converter service receives message and converts file to PDF, saving new pdf version of the file and deleting old html version. Then converter sends message to Uploader that file has been converted
- Uploader receives message about finishing conversion and sends update through websocket to the client

Video demo: https://disk.yandex.ru/i/PQtEdeQSCASTfQ

# Important technical decisions

## Scaling

- Service is split into 2 microservices: Uploader (server) and Converter, which communicate via RabbitMQ. This allows for:
  - Asynchronous converting process. User can upload new files while previous are converting.
  - Horizontal scaling: any number of Converter can be spin up to enhance converting capabilities - RabbitMQ will split messages evenly between them; same way more Uploaders can be spin up with potential load-balancer splitting traffic evenly between them.

## Fault-tolerance

- Each file has status. When file is uploaded it has "Uploaded" status. When message is sent to RabbitMQ to convert it, it gets "Converting" status. If Uploader service crashed in between file was uploaded and message was sent to RabbitMQ, it will send messages for all "Uploaded" files on startup.
- Message queue for "convert this file" messages is persistent. If Converter crashes, it will start from the last message it received.

## Authentication

- Very simple implicit authentication mechanism was implemented in order to link files to users.
- User gets set random GUID UserId in Cookie if he doesn't already have one on "authenticate" request, which happens on page load.
- Not a long-term authentication solution, just something to differentiate user for the duration of the cookie lifetime.


# Potential improvements

- Code can be refactored and improved, I focused more on designing and implementing architecture needed for scalability and fault-tolerance, rather than code abstractions and composition.
- Everything would be better put into docker containers and run in docker-compose, but I tried to install chrome in Converter service docker image and failed after multiple hours of attempts. Possibly it has something to do with the fact I'm on ARM (mac m1) platform.
- Improve converter with parallel conversions.
- Cheaper to use something like AWS S3 for file storage and not database.

# How to run

Prerequisites:
- .NET 7+ SDK
- node.js
- npm
- docker

How to start:
- run `run-infra.sh` to build and boot up rabbitmq and postgres docker containers
- Build and start Converter and Uploader projects
- `npm install && npm run start` in frontend directory
- Go to localhost:3000
