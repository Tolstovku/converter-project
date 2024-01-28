#!/bin/bash

# Wait for RabbitMQ to be ready
until rabbitmqctl wait /var/lib/rabbitmq/mnesia/rabbit@localhost.pid; do
  sleep 1
done

# Set up RabbitMQ to create a queue named "Files"
rabbitmq-plugins enable --offline rabbitmq_management
exec "$@"


